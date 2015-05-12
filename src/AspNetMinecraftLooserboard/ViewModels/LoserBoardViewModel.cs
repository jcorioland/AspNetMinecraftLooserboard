using System;
using System.Collections.Generic;

namespace Minecraft4Dev.Web.ViewModels
{
    public class LoserBoardViewModel
    {
        public LoserBoardViewModel()
        {
            this.TopLosers = new List<LoserViewModel>();
            this.Losers = new List<LoserViewModel>();
        }

        public IList<LoserViewModel> TopLosers { get; private set; }
        public IList<LoserViewModel> Losers { get; private set; }
    }
}