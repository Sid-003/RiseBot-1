using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Services
{
    [Service]
    public class BigBrotherService
    {
        private readonly ClashClient _clash;
        private readonly DiscordSocketClient _client;
        private readonly WarReminderService _warReminder;

        private readonly Dictionary<string, AttackData> _warInfo;

        public BigBrotherService(ClashClient clash, DiscordSocketClient client, WarReminderService warReminder)
        {
            _clash = clash;
            _client = client;
            _warReminder = warReminder;

            _warInfo = new Dictionary<string, AttackData>(50);
        }

        public async Task RunServiceAsync()
        {
            var guild = _client.GetGuild(351002726207062017);
            var category = guild.GetCategoryChannel(609494635106271267);

            while(true)
            {
                //await _warReminder.WaitTillWarAsync();
                var currentWar = await _clash.GetCurrentWarAsync("#2GGCRC90");

                var channel = await guild.CreateTextChannelAsync(currentWar.Opponent.Name, x => x.CategoryId = category.Id);

                while (currentWar?.State == WarState.InWar)
                {
                    var toSend = new List<(string, Color)>();

                    (int, string) GetOpponent(string tag)
                    {
                        var opp = currentWar.Opponent.Members.FirstOrDefault(x => x.Tag == tag);

                        return (opp.MapPosition, opp.Name);
                    }

                    Color GetColor(int cPos, int oPos, int stars, TimeSpan remaining)
                    {
                        if (remaining > TimeSpan.FromHours(12) && cPos != oPos && oPos != 1)
                            return Color.Red;

                        if (_warReminder.IsWin() && cPos == oPos && stars < 3)
                            return Color.Red;
                        else if (!_warReminder.IsWin() && stars > 2)
                            return Color.Red;

                        return Color.Green;
                    }

                    foreach (var member in currentWar.Clan.Members)
                    {
                        var attacks = member.Attacks.ToArray();

                        if (_warInfo.TryGetValue(member.Tag, out var data))
                        {
                            if (attacks.Length == 2 && data.SecondAttack is null)
                            {
                                var attack = attacks[1];

                                var (pos, name) = GetOpponent(attack.DefenderTag);

                                data.SecondAttack = pos;
                                data.SecondStars = attack.Stars;

                                var timeLeft = currentWar.EndTime - DateTimeOffset.UtcNow;

                                var remaining = currentWar.EndTime - DateTimeOffset.UtcNow;

                                toSend.Add(($"{member.MapPosition}:{member.Name} attacked {pos}:{name} for {attack.Stars}", GetColor(member.MapPosition, pos, attack.Stars, remaining)));
                            }
                        }
                        else
                        {

                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5));
                }

                await channel.ModifyAsync(x => x.Name = "completed-" + currentWar.Opponent.Name);
                _warInfo.Clear();
            }
        }

        private struct AttackData
        {
            public int? FirstAttack { get; set; }
            public int? FirstStars { get; set; }
            public int? SecondAttack { get; set; }
            public int? SecondStars { get; set; }
        }
    }
}
