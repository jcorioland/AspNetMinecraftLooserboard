using Newtonsoft.Json;
using System;

namespace Minecraft4Dev.Web.Models
{
    public class DeathLog
    {
        [JsonProperty("playerName")]
        public string PlayerName { get; set; }

        [JsonProperty("deathReason")]
        public string DeathReason { get; set; }

        [JsonProperty("deathDate")]
        public DateTime DeathDate { get; set; }
    }
}