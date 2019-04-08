using System;
using System.Collections.Generic;

namespace RiseBot
{
    public static class LogFactory
    {
        private static readonly IReadOnlyDictionary<Discord.LogSeverity, Severity> DiscordSeverity =
            new Dictionary<Discord.LogSeverity, Severity>
            {
                { Discord.LogSeverity.Verbose, Severity.Verbose },
                { Discord.LogSeverity.Critical, Severity.Critical },
                { Discord.LogSeverity.Debug, Severity.Debug },
                { Discord.LogSeverity.Error, Severity.Error },
                { Discord.LogSeverity.Info, Severity.Info },
                { Discord.LogSeverity.Warning, Severity.Verbose },
            };

        public static (Source Source, Severity Severity, string Message, Exception Exception) FromDiscord(
            Discord.LogMessage log)
        {
            return (Source.Discord, DiscordSeverity[log.Severity], log.Message, log.Exception);
        }
    }
}
