using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord.WebSocket;
using System;
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
        private readonly StartTimeService _start;
        private readonly LogService _logger;

        private CancellationTokenSource _cancellationTokenSource;

        private const string ClanTag = "#2GGCRC90";

        private ReminderState _lastState;
        private CurrentWarState _currentWarState;

        public WarReminderService(DiscordSocketClient client, ClashClient clash, DatabaseService database,
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

                _lastState = ReminderState.Maintenance;

                return _logger.LogAsync(Source.Reminder, Severity.Verbose, "Maintenance break");
            };
        }

        public async Task StartRemindersAsync()
        {
            while (true)
            {
                switch (_lastState)
                {
                    case ReminderState.InPrep:
                        break;

                    case ReminderState.Start:
                        break;

                    case ReminderState.BeforeEnd:
                        break;

                    case ReminderState.None:
                    case ReminderState.Ended:

                        var (inPrep, currentWar) = await CheckForWarPrepAsync();

                        if (!inPrep)
                            continue;

                        _lastState = ReminderState.InPrep;

                        _currentWarState = new CurrentWarState
                        {
                            ReminderState = _lastState,
                            StartTime = currentWar.StartTime,
                            EndTime = currentWar.EndTime
                        };

                        try
                        {
                            var delay = _currentWarState.StartTime - DateTimeOffset.UtcNow;
                            await Task.Delay(delay, _cancellationTokenSource.Token);
                        }
                        catch (TaskCanceledException)
                        {
                        }

                        break;

                    case ReminderState.Maintenance:
                        break;
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private async Task<(bool InPrep, CurrentWar CurrentWar)> CheckForWarPrepAsync()
        {
            var currentWar = await _clash.GetCurrentWarAsync(ClanTag);

            if (currentWar is null || currentWar.State != WarState.Preparation)
                return (false, null);

            return (true, currentWar);
        }

        public async Task StartPollingServiceAsync()
        {
            while (true)
            {
                await _clash.GetCurrentWarAsync(ClanTag);
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private enum ReminderState
        {
            None,
            InPrep,
            Start,
            BeforeEnd,
            Ended,
            Maintenance
        }

        private struct CurrentWarState
        {
            public ReminderState ReminderState { get; set; }

            public DateTimeOffset StartTime { get; set; }
            public DateTimeOffset EndTime { get; set; }

            public ulong MatchMessageId { get; set; }
        }
    }
}
