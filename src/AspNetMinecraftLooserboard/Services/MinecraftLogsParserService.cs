using Minecraft4Dev.Web.Models;
using Minecraft4Dev.Web.Persistence;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Minecraft4Dev.Web.Services
{
    public class MinecraftLogsParserService
    {
        private readonly string _logFilesDirectoryPath;
        private readonly MinecraftLogsDatabase _logsDatabase;

        private readonly Regex _fileNameDateRegex = new Regex(@"(\d{4}-\d{2}-\d{2})-\d", RegexOptions.Compiled);
        private readonly List<Regex> _deathRegexList = new List<Regex>();

        public MinecraftLogsParserService(string logFilesDirectoryPath, MinecraftLogsDatabase logsDatabase)
        {
            if (string.IsNullOrEmpty(logFilesDirectoryPath))
            {
                throw new ArgumentNullException("logFilesDirectoryPath");
            }

            if (logsDatabase == null)
            {
                throw new ArgumentNullException("logsDatabase");
            }

            if (!Directory.Exists(logFilesDirectoryPath))
            {
                throw new DirectoryNotFoundException(string.Format("The log files directory was not found at {0}", logFilesDirectoryPath));
            }

            _logFilesDirectoryPath = logFilesDirectoryPath;
            _logsDatabase = logsDatabase;

            #region death regex 


            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was squashed by a falling anvil)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was pricked to death)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (walked into a cactus while trying to escape) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was shot by arrow)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was shot by) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (drowned)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (drowned whilst trying to escape) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (blew up)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was blown up by) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (hit the ground too hard)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (fell from a high place)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (fell off a ladder)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (fell off some vines)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (fell out of the water)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (fell into a patch of fire)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (fell into a patch of cacti)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was doomed to fall) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was shot off some vines by) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was shot off a ladder by) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was blown from a high place by) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (went up in flames)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (burned to death)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was burnt to a crisp whilst fighting) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (walked into a fire whilst fighting) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (tried to swim in lava)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (tried to swim in lava while trying to escape) (\w+)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (suffocated in a wall)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (starved to death)", RegexOptions.Compiled));
            _deathRegexList.Add(new Regex(@"\[(\d{2}:\d{2}:\d{2})\] \[Server thread\/INFO]: (\w+) (was killed by magic)", RegexOptions.Compiled));

            #endregion
        }

        public void Start(CancellationToken token)
        {
            new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    ParseLogs();
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            }).Start();
        }

        private void ParseLogs()
        {
            DirectoryInfo logsDirectoryInfo = new DirectoryInfo(_logFilesDirectoryPath);

            var archivedLogFiles = logsDirectoryInfo
                .GetFiles("*.log.gz")
                .Where(archivedLogFile => !_logsDatabase.ProcessedLogFiles.Contains(archivedLogFile.FullName))
                .ToList();

            foreach (var archivedLogFile in archivedLogFiles.OrderBy(a => a.Name))
            {
                DateTime logFileDate = ExtractDateFromFileName(archivedLogFile.Name);
                using (var fs = new FileStream(archivedLogFile.FullName, FileMode.Open, FileAccess.Read))
                {
                    using (var gzipStream = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        ProcessLogStream(logFileDate, gzipStream);
                    }
                }

                _logsDatabase.ProcessedLogFiles.Add(archivedLogFile.FullName);
            }

            var logFiles = logsDirectoryInfo
                .GetFiles("*.log")
                .Where(logFile => !_logsDatabase.ProcessedLogFiles.Contains(logFile.FullName))
                .ToList();

            foreach (var logFile in logFiles.OrderBy(a => a.Name))
            {
                DateTime logFileDate = ExtractDateFromFileName(logFile.Name);

                using (var fs = new FileStream(logFile.FullName, FileMode.Open, FileAccess.Read))
                {
                    ProcessLogStream(logFileDate, fs);
                }

                if (logFile.Name != "latest.log")
                {
                    _logsDatabase.ProcessedLogFiles.Add(logFile.FullName);
                }
            }

            _logsDatabase.Save();
        }

        private void ProcessLogStream(DateTime logFileDate, Stream fs)
        {
            using (var streamReader = new StreamReader(fs))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    DeathLog deathLog;
                    if (TryGetDeathLog(line, logFileDate, out deathLog))
                    {
                        if (!_logsDatabase.DeathLogs
                            .Any(d => d.DeathDate == deathLog.DeathDate && d.PlayerName == deathLog.PlayerName && d.DeathReason == deathLog.DeathReason))
                        {
                            _logsDatabase.DeathLogs.Add(deathLog);

                            var player = _logsDatabase.LooserBoard.FirstOrDefault(p => p.Name == deathLog.PlayerName);
                            if (player == null)
                            {
                                player = new Player();
                                player.Name = deathLog.PlayerName;
                                _logsDatabase.LooserBoard.Add(player);
                            }

                            player.Score += 1;
                        }
                    }
                }
            }
        }

        private bool TryGetDeathLog(string line, DateTime logDate, out DeathLog deathLog)
        {
            foreach (var deathRegex in _deathRegexList)
            {
                var match = deathRegex.Match(line);
                if (match.Success && match.Groups.Count >= 4)
                {
                    string[] timeInfo = match.Groups[1].Value.Split(new char[] { ':' });
                    string playerName = match.Groups[2].Value;
                    string deathReason = match.Groups[3].Value;

                    // the mob
                    if (match.Groups.Count == 5)
                    {
                        deathReason += " " + match.Groups[4].Value;
                    }

                    deathLog = new DeathLog();
                    deathLog.DeathDate = logDate
                        .AddHours(double.Parse(timeInfo[0]))
                        .AddMinutes(double.Parse(timeInfo[1]))
                        .AddSeconds(double.Parse(timeInfo[2]));

                    deathLog.DeathReason = deathReason;
                    deathLog.PlayerName = playerName;

                    return true;
                }
            }

            deathLog = null;
            return false;
        }

        private DateTime ExtractDateFromFileName(string name)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            if (_fileNameDateRegex.IsMatch(fileNameWithoutExtension))
            {
                return DateTime.ParseExact(
                    _fileNameDateRegex.Match(fileNameWithoutExtension).Groups[1].Value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);
            }

            return DateTime.Now.Date;
        }
    }
}