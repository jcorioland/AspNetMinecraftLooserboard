using Newtonsoft.Json;

namespace Minecraft4Dev.Web.Models
{
    public class Player
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }
    }
}