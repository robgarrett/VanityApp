using Newtonsoft.Json;

namespace RobGarrett365.VanityApp
{
    public class Mapping
    {
        [JsonProperty("vanity")]
        public string Vanity { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}