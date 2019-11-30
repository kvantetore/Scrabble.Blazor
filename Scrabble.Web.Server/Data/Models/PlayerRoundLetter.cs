using System;

namespace Scrabble.Data.Models
{
    public class PlayerRoundLetter
    {
        public Guid PlayerRoundLetterId { get; set; }
        public PlayerRound PlayerRound { get; set; }
        public Guid PlayerRoundId { get; set; }

        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string Letter { get; set; }
    }
}
