using Minecraft4Dev.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Minecraft4Dev.Web.Persistence
{
    public class MinecraftLogsDatabase
    {
        public MinecraftLogsDatabase()
        {
            this.ProcessedLogFiles = new List<string>();
            this.LooserBoard = new List<Player>();
            this.DeathLogs = new List<DeathLog>();
        }

        [JsonProperty("processedLogFiles")]
        public IList<string> ProcessedLogFiles { get; private set; }

        [JsonProperty("looserBoard")]
        public IList<Player> LooserBoard { get; private set; }

        [JsonProperty("deathLogs")]
        public IList<DeathLog> DeathLogs { get; private set; }

        [JsonIgnore]
        public string JsonFilePath { get; private set; }

        public static MinecraftLogsDatabase LoadFrom(string jsonFilePath)
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                throw new ArgumentNullException("jsonFilePath");
            }

            if (!File.Exists(jsonFilePath))
            {
                return new MinecraftLogsDatabase() { JsonFilePath = jsonFilePath };
            }

            using(var fs = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read))
            {
                using(var reader = new StreamReader(fs))
                {
                    string jsonDb = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(jsonDb))
                    {
                        var database = JsonConvert.DeserializeObject<MinecraftLogsDatabase>(jsonDb);
                        database.JsonFilePath = jsonFilePath;

                        var latestLog = database.ProcessedLogFiles.FirstOrDefault(l => l.Contains("\\latest.log") || l.Contains("/latest.log"));
                        if (latestLog != null)
                        {
                            database.ProcessedLogFiles.Remove(latestLog);
                        }

                        return database;
                    }

                    return new MinecraftLogsDatabase() { JsonFilePath = jsonFilePath };
                }
            }
        }

        public void Save()
        {
            string jsonDb = JsonConvert.SerializeObject(this);
            using(var fs = new FileStream(this.JsonFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(jsonDb);
                fs.SetLength(buffer.Length);
                fs.Seek(0, SeekOrigin.Begin);

                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
            }
        }
    }
}