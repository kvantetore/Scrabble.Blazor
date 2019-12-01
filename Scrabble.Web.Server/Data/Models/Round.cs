using System;
using System.Collections.Generic;

namespace Scrabble.Data.Models
{
    public class Round
    {
        public Guid RoundId { get; set; }
        public Game Game { get; set; }
        public Guid GameId { get; set; }

        public int RoundNumber {get;set;}
        public List<PlayerRound> PlayerRounds { get; set; }
    }
}
