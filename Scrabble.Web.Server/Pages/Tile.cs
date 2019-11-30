namespace Scrabble.Web.Server.Pages
{
    public class Tile 
    {
        public (int i, int j) Position { get; set; }
        public TileType TileType { get; set; }
        public string CurrentLetter { get; set; }
        public bool Scored {get;set;}
    }        

}
