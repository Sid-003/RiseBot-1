using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseBot.Services
{
    [Service]
    public class EventService
    {
        private readonly DatabaseService _database;
        private readonly DiscordSocketClient _client;
        private readonly TimerService _timer;
        private Random _random;

        private Guild Guild => _database.Guild;
        private Random Random => _random ?? (_random = new Random());

        private static readonly IReadOnlyDictionary<int, string> Months = new Dictionary<int, string>
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

        public EventService(DatabaseService database, DiscordSocketClient client, TimerService timer, Random random)
        {
            _database = database;
            _client = client;
            _timer = timer;
            _random = random;
        }

        public async Task NewEventAsync(int month, int day, int end, string description, bool mention)
        {
            var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, month);

            if (month < 1 || day < 1 || end < day || day > daysInMonth)
            {
                throw new Exception("A valid date is required");
            }

            var when = CalculateRemoveTime(month, day);
            when = when < 0 ? 0 : when;

            var @event = new Event
            {
                Description = description,
                Mention = mention,
                Month = month,
                Day = day,
                End = end,
                Id = (ulong)Random.Next()
            };

            Guild.Events.Add(@event);
            _database.UpdateGuild();

            var message = await GetOrSendMessageAsync();

            if (message is null)
                return;

            await message.ModifyAsync(x => x.Content = BuildMessage());

            await _timer.EnqueueAsync(@event, when, async (key, removable) =>
            {
                @event = removable as Event;
                var guild = _client.GetGuild(Guild.Id);
                IRole role = guild.GetRole(Guild.EventRoleId);

                if (role is null)
                {
                    role = await guild.CreateRoleAsync("Events");
                    Guild.EventRoleId = role.Id;
                    _database.UpdateGuild();
                }

                await role.ModifyAsync(x => x.Mentionable = true);

                if (!(_client.GetChannel(Guild.GeneralId) is SocketTextChannel channel))
                    return;

                var str = @event?.Mention == true ? $"{role.Mention} " : "";

                await channel.SendMessageAsync($"{str}Event Started! - {@event?.Description}");

                await role.ModifyAsync(x => x.Mentionable = false);

                if (end > 0)
                {
                    await _timer.EnqueueAsync(@event, end,
                        (key2, removable2) =>
                            channel.SendMessageAsync(
                                $"{str}Event Ended! - {@event?.Description}"));
                }
            });

        }


        private static long CalculateRemoveTime(int month, int day)
        {
            var when = DateTimeOffset.UtcNow.AddMonths(month).AddDays(day);
            return (long)(when - DateTimeOffset.UtcNow).TotalMilliseconds;
        }

        private async Task<IUserMessage> GetOrSendMessageAsync()
        {
            if (!(_client.GetChannel(Guild.EventChannelId) is SocketTextChannel channel))
                return null;

            if (await channel.GetMessageAsync(Guild.EventMessageId) is IUserMessage message)
                return message;

            message = await channel.SendMessageAsync(BuildMessage());
            Guild.EventMessageId = message.Id;
            _database.UpdateGuild();

            return message;
        }

        private string BuildMessage()
        {
            return "";
        }
    }
}
