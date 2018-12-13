using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord.WebSocket;
using RiseBot.Results;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RiseBot.Services
{
    [Service]
    public class WarReminderService
    {
        private readonly DiscordSocketClient _client;
        private readonly ClashClient _clash;
        private readonly DatabaseService _database;

        private CancellationTokenSource _cancellationTokenSource;

        private const string ClanTag = "#2GGCRC90";

        public WarReminderService(DiscordSocketClient client, ClashClient clash, DatabaseService database)
        {
            _client = client;
            _clash = clash;
            _database = database;

            _clash.ErrorReceived += (message) =>
            {
                if (message.Error != "inMaintenance")
                    return Task.CompletedTask;

                _cancellationTokenSource.Cancel(true);
                _cancellationTokenSource = new CancellationTokenSource();
                return Task.CompletedTask;
            };
        }

        public async Task StartService()
        {
            var previousState = WarState.Default;
            var remindersSet = false;

            while (true)
            {
                await Task.Delay(30000);
                var currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                if (previousState == currentWar.State)
                    continue;

                previousState = currentWar.State;

                if(remindersSet)
                    continue;

                remindersSet = true;

                var startTime = currentWar.StartTime;
                var endTime = currentWar.EndTime;

                var cancellationToken = _cancellationTokenSource.Token;

                _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(startTime - DateTimeOffset.UtcNow, cancellationToken);

                            var channelId = _database.Guild.WarChannelId;

                            if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                                return;

                            var result = await Utilites.CalculateWarWinnerAsync(_clash, ClanTag);
                            var sb = new StringBuilder();

                            switch (result)
                            {
                                case LottoDraw lottoDraw:
                                    //TODO high/low sync
                                    break;

                                case LottoFailed lottoFailed:
                                    await channel.SendMessageAsync(lottoFailed.Reason);
                                    remindersSet = false;
                                    _cancellationTokenSource.Cancel(true);
                                    _cancellationTokenSource = new CancellationTokenSource();
                                    break;

                                case LottoResult lottoResult:
                                    sb.Append("It is ");
                                    sb.Append(lottoResult.ClanWin ? $"{lottoResult.ClanName}" : $"{lottoResult.OpponentName}");
                                    sb.Append("'s win!");

                                    await channel.SendMessageAsync(sb.ToString());
                                    break;
                            }

                            await Task.Delay(endTime - DateTimeOffset.UtcNow.AddHours(1), cancellationToken);

                            //TODO @ people
                            await channel.SendMessageAsync("War ends in one hour!");

                            await Task.Delay(endTime - DateTimeOffset.UtcNow, cancellationToken);

                            //TODO @ people
                            await channel.SendMessageAsync("War has ended!");
                        }
                        catch (TaskCanceledException)
                        {
                            remindersSet = false;
                            previousState = WarState.Default;
                        }

                    },
                    cancellationToken);
            }
        }
    }
}
