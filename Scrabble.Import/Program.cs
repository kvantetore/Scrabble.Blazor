using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrabble.Data.Models;

namespace Scrabble.Import
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.local.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[] { })
                .Build();

            // create db context
            var connectionString = config.GetConnectionString("Scrabble");
            var dbContextOptions = new DbContextOptionsBuilder<ScrabbleContext>()
                .UseNpgsql(connectionString)
                .Options;
            var dbContext = new ScrabbleContext(dbContextOptions);

            var players = dbContext.Players.ToList();
            var existingGames = dbContext.Games.ToList();

            // load file data
            var filename = args[0];
            var filetext = System.IO.File.ReadAllText(filename);
            var filedata = JObject.Parse(filetext);

            var rowdata = filedata["sheets"][0]["data"][0]["rowData"];
            Console.WriteLine($"{filedata["sheets"] == null}");
            Console.WriteLine($"{filedata["sheets"][0] == null}");
            Console.WriteLine($"{filedata["sheets"][0]["data"][0]["rowData"] == null}");

            var currentGame = null as Game;
            foreach (var (col, columnIndex) in rowdata[0]["values"].Enumerate()) {
                var dateStr = col["formattedValue"]?.ToString();
                if (dateStr != null) {
                    var date = DateTimeOffset.ParseExact(dateStr, "d.MM.yyyy", System.Globalization.CultureInfo.CurrentCulture);
                    Console.WriteLine(date);
                    var existingGame = existingGames.Any(g => g.Start.Date == date);
                    if (existingGame) {
                        Console.WriteLine("Game already imported, skipping");
                        currentGame = null;
                    }
                    else {
                        dbContext.SaveChanges();
                        currentGame = new Game()
                        {
                            Start = date,
                            End = date,
                            GamePlayers = new List<GamePlayer>(),
                            Rounds = new List<Round>(),
                        };
                        dbContext.Games.Add(currentGame);
                    }
                }

                if (currentGame != null) {
                    var playerName = rowdata[1]["values"][columnIndex]["formattedValue"].ToString();
                    Console.WriteLine($"  Player {playerName}");

                    var player = players.FirstOrDefault(p => string.Equals(p.Name, playerName, StringComparison.CurrentCultureIgnoreCase));
                    if (player == null) {
                        player = new Player()
                        {
                            Name = playerName,
                        };
                        players.Add(player);
                    }
                    currentGame.GamePlayers.Add(new GamePlayer
                    {
                        Player = player,
                        Order = currentGame.GamePlayers.Count(),
                    });

                    for (var rowIndex = 3; rowIndex < 30; rowIndex++)
                    {
                        var scoreStr = rowdata[rowIndex]["values"][columnIndex]?["formattedValue"]?.Value<string>();
                        if (scoreStr == null)
                        {
                            break;
                        }

                        if (!int.TryParse(scoreStr, out var score))
                        {
                            Console.WriteLine($"Parse error in score: {scoreStr}");
                            break;
                        }

                        var roundNumber = rowIndex - 2;
                        var round = currentGame.Rounds.FirstOrDefault(r => r.RoundNumber == roundNumber);
                        if (round == null) {
                            round = new Round
                            {
                                RoundNumber = roundNumber,
                                PlayerRounds = new List<PlayerRound>(),
                            };
                            currentGame.Rounds.Add(round);
                        }

                        round.PlayerRounds.Add(new PlayerRound {
                            Player = player,
                            Score = score,
                        });

                        Console.WriteLine($"    {score}");
                    }
                }
            }

            dbContext.SaveChanges();
        }
    }

    public static class EnumerateExtension {
        public static IEnumerable<(T, int)> Enumerate<T>(this IEnumerable<T> source) {
            return source.Select((x, i) => (x, i));
        }
    }
}
