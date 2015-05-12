using System;
using System.Collections.Generic;

namespace Minecraft4Dev.Web.ViewModels
{
    public class PlayerDetailsViewModel
    {
        public PlayerDetailsViewModel()
        {
            this.DeathLogs = new List<DeathLogViewModel>();
        }

        public string Name { get; set; }
        public IList<DeathLogViewModel> DeathLogs { get; private set; }
    }
}