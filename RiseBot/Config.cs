using Newtonsoft.Json.Linq;
using System.IO;

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

            //why am I retarded
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
