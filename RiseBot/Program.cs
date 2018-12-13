using System.Threading.Tasks;

namespace RiseBot
{
    internal class Program
    {
        private static async Task Main()
        {
            var bot = new Bot();
            await bot.RunBotAsync();
        }
    }
}
