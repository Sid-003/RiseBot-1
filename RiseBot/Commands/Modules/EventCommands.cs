using System;
using System.Collections.Generic;
using System.Text;
using Qmmands;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    public class EventCommands : RiseBase
    {
        private static readonly IReadOnlyDictionary<int, string> Month = new Dictionary<int, string>
        {
            [1] = "January",
            [2] = "February",
            [3] = "March",
            [4] = "April",
            [5] = "May",
            [6] = "June",
            [7] = "July",
            [8] = "August",
            [9] = "September",
            [10] = "October",
            [11] = "November",
            [12] = "Decemeber"
        };

        [Command("view")]
        public Task ViewEventsAsync()
        {
            var month = DateTime.UtcNow.Month;
            var monthString = Month[month];
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
