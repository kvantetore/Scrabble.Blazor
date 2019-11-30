using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Web;

namespace Scrabble.Web.Server.Pages
{
    public class Player
    {
        public string Name { get; set; }
        public List<int> Scores { get; set; }
    }

    partial class Index
    {
        protected Tile[,] Tiles = new Tile[15, 15];
        protected List<Player> Players = new List<Player>();

        protected (int i, int j) SelectedIndex;
        (int i, int j) LastMovement;

        protected IEnumerable<Tile> AllTiles
        {
            get
            {
                foreach (var tile in Tiles)
                {
                    yield return (Tile)tile;
                }
            }
        }

        static Dictionary<string, int> LetterScore = new Dictionary<string, int> {
            { "a", 1 },
            { "b", 4 },
            { "c", 10},
            { "d", 1 },
            { "e", 1 },
            { "f", 2 },
            { "g", 2 },
            { "h", 3 },
            { "i", 1 },
            { "j", 4 },
            { "k", 2 },
            { "l", 1 },
            { "m", 2 },
            { "n", 1 },
            { "o", 2 },
            { "p", 4 },
            { "r", 1 },
            { "s", 1 },
            { "t", 1 },
            { "u", 4 },
            { "v", 4 },
            { "w", 8 },
            { "y", 6 },
            { "æ", 6 },
            { "ø", 5 },
            { "å", 4 },
            { " ", 0 },
        };

        static string[] AllowedCharacters = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "æ", "ø", "å" };

        protected async Task OnTileClick(int i, int j)
        {
            SelectedIndex = (i, j);
            LastMovement = (0, 0);
        }

        protected async Task OnKeyDown(KeyboardEventArgs args)
        {
            var (i, j) = SelectedIndex;
            var currentTile = Tiles[SelectedIndex.i, SelectedIndex.j];

            switch (args.Code)
            {
                case "ArrowRight": MoveCursor((0, 1)); break;
                case "ArrowLeft": MoveCursor((0, -1)); break;
                case "ArrowUp": MoveCursor((-1, 0)); break;
                case "ArrowDown": MoveCursor((1, 0)); break;
            }

            if (CanEdit(currentTile))
            {
                if (AllowedCharacters.Contains(args.Key))
                {
                    currentTile.CurrentLetter = args.Key;
                    MoveCursor(Normalize(LastMovement));
                }

                if (args.Code == "Delete" || args.Code == "Backspace")
                {
                    currentTile.CurrentLetter = null;
                    MoveCursor(Subtract((0, 0), Normalize(LastMovement)));
                }
            }
        }

        protected async Task ScoreTiles(Player player)
        {
            var score = GetCurrentScore();

            var scoringTiles = AllTiles.Where(t => IsFilled(t) && !t.Scored).ToList();
            foreach (var tile in scoringTiles)
            {
                tile.Scored = true;
            }

            player.Scores.Add(score);
        }

        protected int GetCurrentScore()
        {
            var words = GetCurrentWords();

            var score = 0;
            foreach (var word in words)
            {
                score += GetWordScore(word);
            }

            return score;
        }

        protected List<List<Tile>> GetCurrentWords()
        {
            var scoringTiles = AllTiles.Where(t => IsFilled(t) && !t.Scored).ToList();
            var words = new List<List<Tile>>();

            //horizontal words
            var horizWordTiles = new HashSet<Tile>();
            foreach (var tile in scoringTiles)
            {
                if (!horizWordTiles.Contains(tile))
                {
                    var word = FindWord(tile, WordDirection.Horizontal);
                    if (word != null)
                    {
                        words.Add(word);
                        horizWordTiles.UnionWith(word);
                    }
                }
            }

            //find vertical words
            var vertWordTiles = new HashSet<Tile>();
            foreach (var tile in scoringTiles)
            {
                if (!vertWordTiles.Contains(tile))
                {
                    var word = FindWord(tile, WordDirection.Vertical);
                    if (word != null)
                    {
                        words.Add(word);
                        vertWordTiles.UnionWith(word);
                    }
                }
            }

            return words;
        }

        public enum WordDirection
        {
            Horizontal,
            Vertical,
        }

        private List<Tile> FindWord(Tile tile, WordDirection wordDir)
        {
            if (!IsFilled(tile))
            {
                throw new ArgumentException("Tile must be filled");
            }

            var delta = wordDir == WordDirection.Horizontal ? (0, 1) : (1, 0);

            //find start
            Console.WriteLine($"Finding start of word at {tile.Position}");
            var pos = tile.Position;
            do
            {
                Console.WriteLine($"{pos}: {Tiles[pos.i, pos.j].CurrentLetter}");
                pos = Subtract(pos, delta);
            } while (IsInsideBoard(pos) && IsFilled(Tiles[pos.i, pos.j]));
            pos = Add(pos, delta);

            Console.WriteLine($"Found start of word at {pos}");

            // add letters until end
            var word = new List<Tile>();
            while (IsInsideBoard(pos) && IsFilled(pos))
            {
                word.Add(Tiles[pos.i, pos.j]);

                pos = Add(pos, delta);
            }

            if (word.Count <= 1)
            {
                return null;
            }

            Console.WriteLine($"Found word {string.Join("", word.Select(l => l.CurrentLetter))}");

            return word;
        }

        private int GetWordScore(List<Tile> word)
        {
            var score = 0;
            var multiplier = 1;
            foreach (var tile in word)
            {
                var letterScore = LetterScore[tile.CurrentLetter];

                var letterMultiplier = tile.Scored ? 1 : tile.TileType switch
                {
                    TileType.DoubleLetter => 2,
                    TileType.TripleLetter => 3,
                    _ => 1
                };

                var wordMultiplier = tile.Scored ? 1 : tile.TileType switch
                {
                    TileType.DoubleWord => 2,
                    TileType.Start => 2,
                    TileType.TripleWord => 3,
                    _ => 1
                };

                score += letterScore * letterMultiplier;
                multiplier *= wordMultiplier;
            }

            return score * multiplier;
        }

        protected bool IsFilled(Tile tile)
        {
            return !string.IsNullOrEmpty(tile.CurrentLetter);
        }

        protected bool IsFilled((int i, int j) a)
        {
            return IsFilled(Tiles[a.i, a.j]);
        }

        protected bool CanEdit(Tile tile)
        {
            return !tile.Scored;
        }

        protected void MoveCursor((int i, int j) delta)
        {
            var nextIndex = ((SelectedIndex.i + delta.i + 15) % 15, (SelectedIndex.j + delta.j + 15) % 15);
            SelectedIndex = nextIndex;
            LastMovement = delta;
        }

        protected (int i, int j) Add((int i, int j) a, (int i, int j) b)
        {
            return (a.i + b.i, a.j + b.j);
        }

        protected (int i, int j) Subtract((int i, int j) a, (int i, int j) b)
        {
            return (a.i - b.i, a.j - b.j);
        }

        protected (int i, int j) Normalize((int i, int j) x)
        {
            return (Math.Abs(x.i), Math.Abs(x.j));
        }

        protected bool IsInsideBoard((int i, int j) a)
        {
            return a.i >= 0 && a.i < 15 && a.j >= 0 && a.j < 15;
        }

        protected async Task OnKeyPress(KeyboardEventArgs args)
        {
        }

        protected override async Task OnInitializedAsync()
        {
            Players.Add(new Player
            {
                Name = "Tore",
                Scores = new List<int>(),
            });

            Players.Add(new Player
            {
                Name = "Kjersti",
                Scores = new List<int>(),
            });

            for (var i = 0; i < Tiles.GetLength(0); i++)
            {
                for (var j = 0; j < Tiles.GetLength(1); j++)
                {
                    Tiles[i, j] = new Tile
                    {
                        Position = (i, j),
                        TileType = TileType.Normal,
                    };
                }
            }

            Tiles[7, 7].TileType = TileType.Start;
            SetTileTypeSymmetric(0, 0, TileType.TripleWord);
            SetTileTypeSymmetric(7, 0, TileType.TripleWord);
            SetTileTypeSymmetric(0, 7, TileType.TripleWord);

            SetTileTypeSymmetric(1, 1, TileType.DoubleWord);
            SetTileTypeSymmetric(2, 2, TileType.DoubleWord);
            SetTileTypeSymmetric(3, 3, TileType.DoubleWord);
            SetTileTypeSymmetric(4, 4, TileType.DoubleWord);
            SetTileTypeSymmetric(5, 5, TileType.TripleLetter);
            SetTileTypeSymmetric(6, 6, TileType.DoubleLetter);

            SetTileTypeSymmetric(0, 3, TileType.DoubleLetter);
            SetTileTypeSymmetric(1, 5, TileType.TripleLetter);
            SetTileTypeSymmetric(2, 6, TileType.DoubleLetter);
            SetTileTypeSymmetric(7, 3, TileType.DoubleLetter);

            SetTileTypeSymmetric(3, 0, TileType.DoubleLetter);
            SetTileTypeSymmetric(5, 1, TileType.TripleLetter);
            SetTileTypeSymmetric(6, 2, TileType.DoubleLetter);
            SetTileTypeSymmetric(3, 7, TileType.DoubleLetter);
        }

        private void SetTileTypeSymmetric(int i, int j, TileType tileType)
        {
            for (var q = 0; q < 4; q++)
            {
                var (x, y) = GetQuadrantIndex(i, j, q);
                Tiles[x, y].TileType = tileType;
            }
        }

        protected (int, int) GetQuadrantIndex(int i, int j, int quadrant)
        {
            return quadrant switch
            {
                0 => (i, j),
                1 => (14 - i, j),
                2 => (14 - i, 14 - j),
                3 => (i, 14 - j),
                _ => throw new Exception($"Unexpected quadrant {quadrant}"),
            };
        }

    }

}
