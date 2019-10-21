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

        public Config(string dir)
        {
            var config = JObject.Parse(File.ReadAllText(dir));

            BotToken = config["Token"].ToString();
            ClashToken = config["ClashKey"].ToString();
            BandToken = config["BandKey"].ToString();
            PushBulletToken = config["PushBulletKey"].ToString();
        }
    }
}
