using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Scrabble.Data.Models;
using Microsoft.EntityFrameworkCore;
using Scrabble.Web.Server.Shared.Modal;

namespace Scrabble.Web.Server.Pages.PlayGame
{
    partial class PlayGame
    {
        [Inject] ScrabbleContext DbContext { get; set; }
        [Inject] ModalService ModalService { get; set; }

        [Parameter] public string ParameterId { get; set; }

        protected Tile[,] Tiles = new Tile[15, 15];
        protected Game Game { get; set; }
        protected (int i, int j) SelectedIndex;
        (int i, int j) LastMovement;

        protected IEnumerable<Player> Players => Game.GamePlayers.OrderBy(gp => gp.Order).Select(gp => gp.Player);

        protected bool IsGameEnded => Game?.End != null;

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

        static readonly Dictionary<string, int> LetterScore = new Dictionary<string, int> {
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

        protected async Task OnTileClick(int i, int j)
        {
            SelectedIndex = (i, j);
            LastMovement = (0, 0);
        }

        protected async Task OnKeyDown(KeyboardEventArgs args)
        {
            if (IsGameEnded) {
                return;
            }

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
                if (LetterScore.ContainsKey(args.Key))
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

        protected bool CanUndo() 
        {
            if (IsGameEnded) 
            {
                return false;
            }

            var currentRound = GetCurrentRound();
            if (currentRound == null) 
            {
                return false;
            }

            if (currentRound.RoundNumber > 1) {
                return true;
            }

            return currentRound.PlayerRounds.Any();
        }

        protected async Task Undo()
        {
            if (!CanUndo())
            {
                return;
            }

            var currentRound = GetCurrentRound();

            // if we don't have any scores in this round yet, remove the round and and use the previous round
            if (!currentRound.PlayerRounds.Any())
            {
                Game.Rounds.Remove(currentRound);
                DbContext.Set<Round>().Remove(currentRound);

                currentRound = GetCurrentRound();
            }

            // find the last player round 
            var players = Players.ToList();
            var previousPlayer = players
                .Select(p => new
                {
                    Player = p,
                    PlayerRound = currentRound.PlayerRounds.FirstOrDefault(pr => pr.Player == p),
                })
                .Where(x => x.PlayerRound != null)
                .LastOrDefault();

            //remove player round
            if (previousPlayer != null)
            {
                DbContext.Set<PlayerRound>().Remove(previousPlayer.PlayerRound);
            }

            await DbContext.SaveChangesAsync();

            ResetBoard();
            LoadGame();
        }

        protected int GetPlayerScore(Player player)
        {
            return Game.Rounds.Sum(r => r.PlayerRounds.FirstOrDefault(pr => pr.Player == player)?.Score ?? 0);
        }

        protected Round GetCurrentRound()
        {
            if (IsGameEnded)
            {
                return null;
            }
            return Game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        }

        protected Player GetNextPlayer()
        {
            if (IsGameEnded)
            {
                return null;
            }

            var currentRound = GetCurrentRound();
            var players = Players;

            return players.FirstOrDefault(p => (!currentRound?.PlayerRounds.Any(pr => pr.Player == p)) ?? false)
                ?? players.FirstOrDefault();
        }

        protected async Task ScoreTiles(Player player)
        {
            if (IsGameEnded)
            {
                return;
            }

            var score = GetCurrentScore();

            var scoringTiles = AllTiles.Where(t => IsFilled(t) && !t.Scored).ToList();
            foreach (var tile in scoringTiles)
            {
                tile.Scored = true;
            }

            var currentRound = GetCurrentRound();
            if (currentRound == null || currentRound.PlayerRounds.Any(pr => pr.PlayerId == player.PlayerId))
            {
                currentRound = new Round
                {
                    Game = Game,
                    RoundNumber = (currentRound?.RoundNumber ?? 0) + 1,
                    PlayerRounds = new List<PlayerRound>(),
                };
                Game.Rounds.Add(currentRound);
            }

            currentRound.PlayerRounds.Add(new PlayerRound
            {
                Round = currentRound,
                Player = player,
                Score = score,
                Letters = scoringTiles.Select(t => new PlayerRoundLetter
                {
                    Letter = t.CurrentLetter,
                    RowIndex = t.Position.i,
                    ColIndex = t.Position.j,
                })
                .ToList()
            });

            await DbContext.SaveChangesAsync();
        }

        protected async Task EndGame()
        {
            var result = await EndGameModal.ExecuteModalAsync(ModalService, new EndGameModal.Input
            {
                Players = Players.ToList()
            });

            if (result?.EndScores != null)
            {
                //create a final round with end scores
                var currentRound = GetCurrentRound();
                var endRound = new Round()
                {
                    RoundNumber = (currentRound?.RoundNumber ?? 0) + 1,
                    PlayerRounds = result.EndScores.Select(endScore =>
                    {
                        var letters = (endScore.RemainingLetters ?? "")
                            .Select(c => new PlayerRoundLetter
                            {
                                ColIndex = -1,
                                RowIndex = -1,
                                Letter = c.ToString()
                            })
                            .ToList();
                        var score = letters.Sum(l => - LetterScore[l.Letter]);

                        return new PlayerRound
                        {
                            Player = endScore.Player,
                            Letters = letters,
                            Score = score,
                        };
                    })
                    .ToList(),
                };
                Game.Rounds.Add(endRound);
                Game.End = DateTimeOffset.Now;
                await DbContext.SaveChangesAsync();
            }
        }

        protected int GetCurrentScore()
        {
            var words = GetCurrentWords();

            var score = 0;
            foreach (var word in words)
            {
                score += GetWordScore(word);
            }

            //bonus score for all characters
            var scoringTiles = AllTiles.Where(t => IsFilled(t) && !t.Scored);
            if (scoringTiles.Count() == 7) 
            {
                score += 50;
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
            var pos = tile.Position;
            do
            {
                pos = Subtract(pos, delta);
            } while (IsInsideBoard(pos) && IsFilled(Tiles[pos.i, pos.j]));
            pos = Add(pos, delta);

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
            return !IsGameEnded && !tile.Scored;
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
            ResetBoard();
            LoadGame();
        }

        private void ResetBoard()
        {
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

        private void LoadGame()
        {
            var gameId = Guid.Parse(ParameterId);
            Game = DbContext.Games
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.PlayerRounds)
                        .ThenInclude(pr => pr.Player)
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.PlayerRounds)
                        .ThenInclude(pr => pr.Letters)
                .FirstOrDefault(g => g.GameId == gameId);

            foreach (var round in Game.Rounds)
            {
                foreach (var playerRound in round.PlayerRounds)
                {
                    foreach (var letter in playerRound.Letters)
                    {
                        if (letter.RowIndex >= 0 && letter.ColIndex >= 0) 
                        {
                            var tile = Tiles[letter.RowIndex, letter.ColIndex];
                            tile.CurrentLetter = letter.Letter;
                            tile.Scored = true;
                        }
                    }
                }
            }
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
