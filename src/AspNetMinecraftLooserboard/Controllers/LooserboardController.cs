using System.Linq;
using Microsoft.AspNet.Mvc;
using Minecraft4Dev.Web.Persistence;
using Minecraft4Dev.Web.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Minecraft4Dev.Web.Controllers
{
    public class LooserboardController : Controller
    {
        private readonly MinecraftLogsDatabase _logsDatabase;

        public LooserboardController(MinecraftLogsDatabase logsDatabase)
        {
            _logsDatabase = logsDatabase;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var loserBoard = _logsDatabase.LooserBoard
                .OrderByDescending(l => l.Score)
                .ToList();

            var loserBoardViewModel = new LoserBoardViewModel();
            int position = 1;

            foreach(var loser in loserBoard.Take(3))
            {
                loserBoardViewModel.TopLosers.Add(new LoserViewModel()
                {
                    Position = position,
                    Name = loser.Name,
                    Score = loser.Score
                });

                position++;
            }

            foreach(var loser in loserBoard.Skip(3))
            {
                loserBoardViewModel.Losers.Add(new LoserViewModel()
                {
                    Position = position,
                    Name = loser.Name,
                    Score = loser.Score
                });

                position++;
            }

            return View(loserBoardViewModel);
        }

        [Route("details/{name}")]
        public IActionResult Details(string name)
        {
            var player = _logsDatabase.LooserBoard.FirstOrDefault(p => p.Name == name);
            if(player == null)
            {
                return new HttpNotFoundResult();
            }

            var deathLogs = _logsDatabase.DeathLogs
                .Where(d => d.PlayerName == name)
                .OrderByDescending(d => d.DeathDate)
                .ToList();

            PlayerDetailsViewModel detailsViewModel = new PlayerDetailsViewModel();
            detailsViewModel.Name = name;

            foreach(var log in deathLogs)
            {
                detailsViewModel.DeathLogs.Add(new DeathLogViewModel()
                {
                    Date = log.DeathDate,
                    Reason = log.DeathReason
                });
            }

            return View(detailsViewModel);
        }
    }
}
