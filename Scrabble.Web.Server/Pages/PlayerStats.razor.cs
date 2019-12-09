using Microsoft.AspNetCore.Components;
using Scrabble.Data.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Scrabble.Web.Server.Pages
{
    public partial class PlayerStats
    {
        [Inject] ScrabbleContext DbContext { get; set; }

        public List<Stat> Stats { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var games = DbContext.Games
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.PlayerRounds)
                        .ThenInclude(pr => pr.Player)
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.PlayerRounds)
                        .ThenInclude(pr => pr.Letters)
                .ToList();

            Stats = games
                .SelectMany(g => g.Rounds.SelectMany(r => r.PlayerRounds.Select(pr => new
                {
                    Game = g,
                    Round = r,
                    PlayerRound = pr,
                    Player = pr.Player,
                })))
                .GroupBy(x => x.Player)
                .Select(x => new Stat {
                    Player = x.Key,
                    PositiveScore = x.Where(y => y.PlayerRound.Score > 0).Sum(y => y.PlayerRound.Score),
                    PenaltyScore = x.Where(y => y.PlayerRound.Score < 0).Sum(y => y.PlayerRound.Score),
                    RoundsPlayed = x.Where(y => y.PlayerRound.Score > 0).Count(),
                    RoundsSkipped = x.Where(y => y.PlayerRound.Score == 0).Count(),
                    ScoreWithLetters = x.Where(y => y.PlayerRound.Score > 0 && y.PlayerRound.Letters.Any()).Sum(y => y.PlayerRound.Score),
                    GamesPlayedWithLetters = x.Where(y => y.PlayerRound.Score > 0 && y.PlayerRound.Letters.Any()).GroupBy(y => y.Game).Count(),
                    RoundsPlayedWithLetters = x.Where(y => y.PlayerRound.Score > 0 && y.PlayerRound.Letters.Any()).Count(),
                    LetterCount = x.Where(y => y.PlayerRound.Score > 0).Sum(y => y.PlayerRound.Letters.Count()),
                    PenaltyLetterCount = x.Where(y => y.PlayerRound.Score < 0).Sum(y => y.PlayerRound.Letters.Count()),
                    GamesPlayed = x.GroupBy(y => y.Game).Count(),
                    GamesWon = x.GroupBy(y => y.Game).Where(g => g.Key.GetWinner() == x.Key).Count()
                })
                .OrderByDescending(x => x.PositiveScore + x.PenaltyScore)
                .ToList();
        }

        public class Stat
        {
            public Player Player { get; internal set; }
            public int PositiveScore { get; internal set; }
            public int PenaltyScore { get; internal set; }
            public int RoundsPlayed { get; internal set; }
            public int RoundsPlayedWithLetters { get; internal set; }
            public int GamesPlayedWithLetters { get; internal set; }
            public int RoundsSkipped { get; internal set; }
            public int LetterCount { get; internal set; }
            public int PenaltyLetterCount { get; internal set; }
            public int ScoreWithLetters { get; internal set; }
            public int GamesPlayed { get; internal set; }
            public int GamesWon { get; internal set; }
        }

    }
}