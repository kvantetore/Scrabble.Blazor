using Microsoft.AspNetCore.Components;
using Scrabble.Data.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Scrabble.Web.Server.Shared.Modal;

namespace Scrabble.Web.Server.Pages.PlayGame
{
    public partial class EndGameModal

    {
        List<EndScore> EndScores { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (Model.Players != null)
            {
                EndScores = Model.Players
                    .Select(p => new EndScore
                    {
                        Player = p,
                        RemainingLetters = "",
                    })
                    .ToList();
            }
        }

        public void Submit()
        {
            Context.Close(new Result
            {
                EndScores = EndScores,
            });
        }

        public void Cancel()
        {
            Context.Close(null);
        }

        public class Input
        {
            public List<Player> Players { get; set; }
        }

        public class Result
        {
            public List<EndScore> EndScores { get; set; }
        }

        public class EndScore
        {
            public Player Player { get; set; }
            public string RemainingLetters { get; set; }
        }
    }
}