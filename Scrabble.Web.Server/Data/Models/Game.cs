using System;
using System.Linq;
using System.Collections.Generic;

namespace Scrabble.Data.Models
{
    public class Game
    {
        public Guid GameId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset? End { get; set; }

        public List<GamePlayer> GamePlayers { get; set; }
        public List<Round> Rounds { get; set; }

        public Player GetWinner()
        {
            return Rounds
                .SelectMany(r => r.PlayerRounds.Select(pr => new
                {
                    Player = pr.Player,
                    Score = pr.Score,
                }))
                .GroupBy(x => x.Player)
                .Select(x => new
                {
                    Player = x.Key,
                    TotalScore = x.Sum(y => y.Score)
                })
                .OrderByDescending(x => x.TotalScore)
                .FirstOrDefault()
                ?.Player;
        }
    }
}
