﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Scrabble.Data.Models;

namespace Scrabble.Web.Server.Migrations
{
    [DbContext(typeof(ScrabbleContext))]
    [Migration("20191207091102_GameEnd")]
    partial class GameEnd
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Scrabble.Data.Models.Game", b =>
                {
                    b.Property<Guid>("GameId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("GameId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("Scrabble.Data.Models.GamePlayer", b =>
                {
                    b.Property<Guid>("GameId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("uuid");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.HasKey("GameId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("GamePlayer");
                });

            modelBuilder.Entity("Scrabble.Data.Models.Player", b =>
                {
                    b.Property<Guid>("PlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("PlayerId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("Scrabble.Data.Models.PlayerRound", b =>
                {
                    b.Property<Guid>("RoundId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("uuid");

                    b.Property<int>("Score")
                        .HasColumnType("integer");

                    b.HasKey("RoundId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("PlayerRound");
                });

            modelBuilder.Entity("Scrabble.Data.Models.PlayerRoundLetter", b =>
                {
                    b.Property<Guid>("PlayerRoundLetterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ColIndex")
                        .HasColumnType("integer");

                    b.Property<string>("Letter")
                        .HasColumnType("text");

                    b.Property<Guid>("PlayerRoundId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("PlayerRoundPlayerId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("PlayerRoundRoundId")
                        .HasColumnType("uuid");

                    b.Property<int>("RowIndex")
                        .HasColumnType("integer");

                    b.HasKey("PlayerRoundLetterId");

                    b.HasIndex("PlayerRoundRoundId", "PlayerRoundPlayerId");

                    b.ToTable("PlayerRoundLetter");
                });

            modelBuilder.Entity("Scrabble.Data.Models.Round", b =>
                {
                    b.Property<Guid>("RoundId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("GameId")
                        .HasColumnType("uuid");

                    b.Property<int>("RoundNumber")
                        .HasColumnType("integer");

                    b.HasKey("RoundId");

                    b.HasIndex("GameId");

                    b.ToTable("Round");
                });

            modelBuilder.Entity("Scrabble.Data.Models.GamePlayer", b =>
                {
                    b.HasOne("Scrabble.Data.Models.Game", "Game")
                        .WithMany("GamePlayers")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Scrabble.Data.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Scrabble.Data.Models.PlayerRound", b =>
                {
                    b.HasOne("Scrabble.Data.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Scrabble.Data.Models.Round", "Round")
                        .WithMany("PlayerRounds")
                        .HasForeignKey("RoundId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Scrabble.Data.Models.PlayerRoundLetter", b =>
                {
                    b.HasOne("Scrabble.Data.Models.PlayerRound", "PlayerRound")
                        .WithMany("Letters")
                        .HasForeignKey("PlayerRoundRoundId", "PlayerRoundPlayerId");
                });

            modelBuilder.Entity("Scrabble.Data.Models.Round", b =>
                {
                    b.HasOne("Scrabble.Data.Models.Game", "Game")
                        .WithMany("Rounds")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
