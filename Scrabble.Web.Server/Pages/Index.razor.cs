using Microsoft.AspNetCore.Components;
using Scrabble.Data.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Scrabble.Web.Server.Pages
{
    public partial class Index 
    {
        [Inject] ScrabbleContext DbContext { get;set;}

        protected List<Game> Games { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Games = DbContext.Games
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.PlayerRounds)
                .ToList()
                .OrderByDescending(g => g.Start)
                .ToList();
        }
    }

}
