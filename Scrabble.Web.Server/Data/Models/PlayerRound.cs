using System;
using System.Collections.Generic;

namespace Scrabble.Data.Models
{
    public class PlayerRound
    {
        public Round Round { get; set; }
        public Guid RoundId { get; set; }
        public Player Player { get; set; }
        public Guid PlayerId { get; set; }

        public int Score { get; set; }
        public List<PlayerRoundLetter> Letters { get; set; }
    }
}
