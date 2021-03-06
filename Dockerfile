###################################################################################################
# Setup SDK Image
# - .net core 3.1
# - yarn
###################################################################################################

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder

# set up environment
ENV \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    # Skip extraction of XML docs - generally not useful within an image/container - helps perfomance
    NUGET_XMLDOC_MODE=skip

ENV DOTNET_ROLL_FORWARD_ON_NO_CANDIDATE_FX=2 \
    FAKE_DETAILED_ERRORS=true \
    PATH="/root/.dotnet/tools:${PATH}"

RUN apt-get -qq update \
    && apt-get install -y build-essential --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

# set up node
ENV NODE_VERSION 13.2.0
ENV YARN_VERSION 1.19.2
ENV NODE_DOWNLOAD_SHA dcf3954ecf6a34d65cab277d3565c654996b1d3e6d07cbbd98939cee0792c668
ENV NODE_DOWNLOAD_URL https://nodejs.org/dist/v$NODE_VERSION/node-v$NODE_VERSION-linux-x64.tar.gz

RUN wget "$NODE_DOWNLOAD_URL" -O nodejs.tar.gz \
    && echo "$NODE_DOWNLOAD_SHA  nodejs.tar.gz" | sha256sum -c - \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && npm i -g yarn@$YARN_VERSION \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

# Trigger first run experience by running arbitrary cmd to populate local package cache
RUN dotnet help \
    && dotnet tool install -g fake-cli \
    && dotnet tool install -g paket


###################################################################################################
# Restore & Compile app
###################################################################################################

WORKDIR /source

# caches restore result by copying csproj file separately
COPY ./global.json ./
COPY ./Scrabble.Web.Server/*.csproj ./Scrabble.Web.Server/
COPY ./Scrabble.Web.Server/package.json ./Scrabble.Web.Server/
COPY ./Scrabble.Web.Server/yarn.lock ./Scrabble.Web.Server/

RUN cd ./Scrabble.Web.Server \
    && yarn \
    && dotnet restore

# copies the rest of your code
COPY ./Scrabble.Web.Server ./Scrabble.Web.Server
RUN cd ./Scrabble.Web.Server \
    && yarn run build \
    && dotnet publish --output publish --configuration Release


###################################################################################################
# Setup rutime image with build output
###################################################################################################

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=builder /source/Scrabble.Web.Server/publish .
ENTRYPOINT ["dotnet", "Scrabble.Web.Server.dll"]
