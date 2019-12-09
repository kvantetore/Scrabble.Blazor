using Microsoft.AspNetCore.Components;
using Scrabble.Data.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Scrabble.Web.Server.Shared.Modal;

namespace Scrabble.Web.Server.Pages
{
    public partial class Index 
    {
        [Inject] ScrabbleContext DbContext { get; set; }
        [Inject] ModalService ModalService { get; set; }

        protected List<GameInfo> Games { get; set; }

        protected override async Task OnInitializedAsync()
        {
            LoadGames();
        }

        private void LoadGames() {
            Games = DbContext.Games
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.PlayerRounds)
                        .ThenInclude(pr => pr.Letters)
                .ToList()
                .OrderByDescending(g => g.Start)
                .Select(g => {
                    var winner = g.GetWinner();
                    var playerScore = g.Rounds
                        .SelectMany(r => r.PlayerRounds.Select(pr => new
                        {
                            Player = pr.Player,
                            Score = pr.Score,
                        }))
                        .GroupBy(x => x.Player)
                        .ToLookup(x => x.Key.PlayerId, x => x.Sum(y => y.Score));

                    return new GameInfo
                    {
                        Game = g,
                        Players = g.GamePlayers
                            .OrderBy(gp => gp.Order)
                            .Select(gp => new GamePlayerInfo
                            {
                                Player = gp.Player,
                                Score = playerScore[gp.PlayerId].FirstOrDefault(),
                                Winner = gp.Player == winner,
                            })
                            .ToList()
                    };
                })
                .ToList();
        }

        public async Task DeleteGame(Game game) {
            var result = await ConfirmModal.ExecuteModalAsync(ModalService, new ConfirmModal.Input
            {
                Title = "Delete game",
                Message = "Really delete game? This cannot be undone",
                OkButtonCaption = "Delete game",
                CancelButtonCaption = "Cancel"

            });

            if (result == ConfirmModal.Result.Ok) {
                foreach (var round in game.Rounds) {
                    foreach (var playerRound in round.PlayerRounds) {
                        foreach (var letter in playerRound.Letters) {
                            DbContext.Remove(letter);
                        }
                        DbContext.Remove(playerRound);
                    }
                    DbContext.Remove(round);
                    
                    foreach (var gamePlayer in game.GamePlayers) {
                        DbContext.Remove(gamePlayer);
                    }
                }
                DbContext.Remove(game);
                await DbContext.SaveChangesAsync();

                Games.RemoveAll(g => g.Game == game);
            }
        }

        public class GameInfo
        {
            public Game Game { get; set; }
            public List<GamePlayerInfo> Players { get; set; }
        }

        public class GamePlayerInfo {
            public Player Player { get; set; }
            public int Score { get; set; }
            public bool Winner { get; set; }
        }
    }

}
