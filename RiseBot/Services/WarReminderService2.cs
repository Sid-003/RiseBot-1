using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RiseBot.Services
{
    [Service]
    public class WarReminderService2
    {
        private readonly DiscordSocketClient _client;
        private readonly ClashClient _clash;
        private readonly DatabaseService _database;
        private readonly StartTimeService _start;
        private readonly LogService _logger;

        private CancellationTokenSource _cancellationTokenSource;

        private const string ClanTag = "#2GGCRC90";

        public WarReminderService2(DiscordSocketClient client, ClashClient clash, DatabaseService database,
            StartTimeService start, LogService logger)
        {
            _client = client;
            _clash = clash;
            _database = database;
            _start = start;
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();

            _clash.Error += (message) =>
            {
                if (message.Reason != "inMaintenance")
                    return Task.CompletedTask;

                _cancellationTokenSource.Cancel(true);
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();

                return _logger.LogAsync(Source.Reminder, Severity.Verbose, "Maintenance break");
            };
        }

        public async Task StartRemindersAsync()
        {
            var lastState = LastState.None;

            while (true)
            {
                switch (lastState)
                {
                    case LastState.Start:
                        break;

                    case LastState.BeforeEnd:
                        break;

                    case LastState.None:
                    case LastState.Ended:
                        break;
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private async Task<bool> CheckForWarStartAsync()
        {
            var currentWar = await _clash.GetCurrentWarAsync(ClanTag);

            if (currentWar is null || currentWar.State != WarState.Preparation)
                return false;

            return true;
        }

        public async Task StartPollingServiceAsync()
        {
            while (true)
            {
                await _clash.GetCurrentWarAsync(ClanTag);
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private enum LastState
        {
            None,
            Start,
            BeforeEnd,
            Ended
        }
    }
}
