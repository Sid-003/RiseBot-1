using Qmmands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    //TODO this stupid shit
    public class EventCommands : RiseBase
    {
        [Command("view")]
        public Task ViewEventsAsync()
        {
            var month = DateTime.UtcNow.Month;
            var monthString = DateTime.UtcNow.ToString("MMMM", CultureInfo.InvariantCulture);
            var monthLength = DateTime.DaysInMonth(DateTime.UtcNow.Year, month);

            var sb = new StringBuilder();
            sb.Append("```css\n");
            sb.Append($"[Events For {monthString}]\n");

            var lastAdd = "";

            for (var i = 1; i <= monthLength; i++)
            {
                var toAdd = $"{i,-4}";
                lastAdd = toAdd;
                sb.Append(toAdd);

                if (i % 7 != 0) continue;

                lastAdd = "\n";
                sb.Append('\n');
            }

            sb.Append(lastAdd == "\n" ? "```" : "\n```");

            return SendMessageAsync(sb.ToString());
        }

        [Command("addevent")]
        public async Task AddEventAsync()
        {

        }
    }
}
