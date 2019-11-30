using System;

namespace Scrabble.Data.Models
{
    public class GamePlayer
    {
        public Game Game { get; set; }
        public Guid GameId { get; set; }

        public Player Player { get; set; }
        public Guid PlayerId { get; set; }

        public int Order { get; set; }

    }
}
