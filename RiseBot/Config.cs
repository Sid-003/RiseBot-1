using System.IO;
using Newtonsoft.Json.Linq;

namespace RiseBot
{
    public class Config
    {
        public string BotToken { get; private set; }
        public string ClashToken { get; private set; }
        public string BandToken { get; private set; }

        private Config() { }

        public static Config Create(string dir)
        {
            var c = new Config();

            var config = JObject.Parse(File.ReadAllText(dir));
            c.BotToken = $"{config["Token"]}";
            c.ClashToken = $"{config["ClashKey"]}";
            c.BandToken = $"{config["BandKey"]}";

            return c;
        }
    }
}
