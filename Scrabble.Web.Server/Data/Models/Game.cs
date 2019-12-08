using System;
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
    }
}
