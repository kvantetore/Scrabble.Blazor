using Microsoft.AspNetCore.Components;
using Scrabble.Data.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Scrabble.Web.Server.Pages
{
    public partial class NewGame
    {
        [Inject] ScrabbleContext DbContext { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        protected List<Player> UnselectedPlayers { get; set; }
        protected List<Player> SelectedPlayers { get; set; }

        protected string NewPlayerName { get; set; }

        protected override async Task OnInitializedAsync()
        {
            UnselectedPlayers = DbContext.Players
                .ToList()
                .OrderBy(p => p.Name)
                .ToList();
            SelectedPlayers = new List<Player>();
        }

        protected async Task AddPlayer(Player player)
        {
            if (SelectedPlayers.Contains(player)) {
                return;
            }
            
            SelectedPlayers.Add(player);
        }

        protected async Task MoveUp(Player player)
        {
            var index = SelectedPlayers.IndexOf(player);
            if (index > 0)
            {
                SelectedPlayers.Remove(player);
                SelectedPlayers.Insert(index - 1, player);
            }
        }

        protected async Task MoveDown(Player player)
        {
            var index = SelectedPlayers.IndexOf(player);
            if (index < SelectedPlayers.Count - 1)
            {
                SelectedPlayers.Remove(player);
                SelectedPlayers.Insert(index + 1, player);
            }
        }

        protected async Task RemovePlayer(Player player)
        {
            SelectedPlayers.Remove(player);
        }

        protected async Task NewPlayer(string playerName)
        {
            var player = new Player
            {
                Name = playerName,
            };
            DbContext.Players.Add(player);
            await DbContext.SaveChangesAsync();

            SelectedPlayers.Add(player);
        }

        protected async Task StartGame()
        {
            var players = new List<Player>();

            var game = new Game()
            {
                Start = DateTimeOffset.Now,
            };

            game.GamePlayers = SelectedPlayers
                .Select((p, i) => new GamePlayer
                {
                    Player = p,
                    Order = i,
                })
                .ToList();

            DbContext.Games.Add(game);
            await DbContext.SaveChangesAsync();

            NavigationManager.NavigateTo($"/game/{game.GameId}");
        }

    }
}