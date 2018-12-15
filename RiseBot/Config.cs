using System.IO;
using Newtonsoft.Json.Linq;

namespace RiseBot
{
    public class Config
    {
        public string BotToken { get; private set; }
        public string ClashToken { get; private set; }
        public string BandToken { get; private set; }
        public string PushBulletToken { get; private set; }

        private Config() { }

        public static Config Create(string dir)
        {
            var config = JObject.Parse(File.ReadAllText(dir));

            return new Config
            {
                BotToken = $"{config["Token"]}",
                ClashToken = $"{config["ClashKey"]}",
                BandToken = $"{config["BandKey"]}",
                PushBulletToken = $"{config["PushBulletKey"]}"
            }; ;
        }
    }
}
