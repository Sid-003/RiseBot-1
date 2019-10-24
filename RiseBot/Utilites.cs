using ClashWrapper;
using ClashWrapper.Entities.WarLog;
using RiseBot.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClashWrapper.Entities.War;
using Discord;

namespace RiseBot
{
    public static class Utilites
    {
        public static async Task<BaseLottoResult> CalculateWarWinnerAsync(ClashClient client, string clanTag)
        {
            var currentWar = await client.GetCurrentWarAsync(clanTag);

            if (currentWar is null)
            {
                return new LottoFailed
                {
                    Reason = "API maintenance"
                };
            }

            if (currentWar.Size == 0)
            {
                return new LottoFailed
                {
                    Reason = "API hasn't updated yet"
                };
            }

            clanTag = currentWar.Clan.Tag;
            var opponentTag = currentWar.Opponent.Tag;

            var opponentWarLog = (await client.GetWarLogAsync(opponentTag, 10)).Entity;

            if (opponentWarLog.Count == 0)
            {
                return new LottoFailed
                {
                    Reason = "Opposition war log is private"
                };
            }

            var clanWarLog = (await client.GetWarLogAsync(clanTag, 10)).Entity;

            var trimmedClanLog = TrimWarLog(clanWarLog).ToArray();
            var trimmedOpponentLog = TrimWarLog(opponentWarLog).ToArray();

            var clanTotal = trimmedClanLog.Count(x => x.Result == WarResult.Lose);
            var opponentTotal = trimmedOpponentLog.Count(x => x.Result == WarResult.Lose);

            var comparison = BuildWarLongComparison(trimmedClanLog, trimmedOpponentLog);

            var draw = clanTotal == opponentTotal;

            if (!draw)
            {
                return new LottoResult
                {
                    ClanName = currentWar.Clan.Name,
                    ClanTag = clanTag,
                    OpponentName = currentWar.Opponent.Name,
                    OpponentTag = opponentTag,
                    ClanWin = clanTotal > opponentTotal,
                    WarLogComparison = comparison
                };
            }

            var chars = "#0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var result = 0;
            for (var i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(chars, clanTag[i]) == Array.IndexOf(chars, opponentTag[i])) continue;
                if (Array.IndexOf(chars, clanTag[i]) > Array.IndexOf(chars, opponentTag[i]))
                {
                    result = -1;
                    break;
                }
                result = 1;
                break;
            }
            
            return new LottoDraw
            {
                ClanName = currentWar.Clan.Name,
                ClanTag = clanTag,
                OpponentName = currentWar.Opponent.Name,
                OpponentTag = opponentTag,
                HighSyncWinnerTag = result < 0 ? clanTag : opponentTag,
                WarLogComparison = comparison
            };
        }

        public static IEnumerable<WarLog> TrimWarLog(IEnumerable<WarLog> warLog)
        {
            var filtered = warLog.Where(x => x.Opponent.Level > 0);
            var trimmed = filtered.Take(7);

            return trimmed;
        }

        public static string BuildWarLongComparison(WarLog[] clanWarLog, WarLog[] opponentWarLog)
        {
            var sb = new StringBuilder();

            var clan = clanWarLog[0].Clan;
            var opponent = opponentWarLog[0].Clan;

            sb.AppendLine($"{clan.Name}{clan.Tag} VS {opponent.Name}{opponent.Tag}");
            sb.AppendLine();

            for (var i = 0; i < 7; i++)
            {
                var clanRes = clanWarLog[i].Result;
                var oppRes = opponentWarLog[i].Result;

                //leaving here to show how stupid I am
                //sid: leaving a comment here to truly emphasize how stupid you are
                var clanStr = clanRes == WarResult.Win ? "(WIN)".PadRight(6) : "[LOSS]";
                var oppStr = oppRes == WarResult.Win ? "(WIN)".PadRight(6) : "[LOSS]";

                sb.AppendLine($"{clanStr}\t\t{oppStr}");
            }

            return sb.ToString();
        }

        public static async Task<string> GetOrderedMembersAsync(ClashClient clash, string clanTag)
        {
            var clanMembers = await clash.GetClanMembersAsync(clanTag);
            var currentWar = await clash.GetCurrentWarAsync(clanTag);

            if (currentWar.Clan.Members is null)
            {
                return "";
            }

            var warMembers = currentWar.Clan.Members;

            var membersByDonations = clanMembers.OrderByDescending(x => x.Donations);

            var i = 1;

            var donationList = string.Join('\n', membersByDonations.Select(x => $"{i++}: {Format.Sanitize(x.Name)} - **{x.Donations}**"));

            var sb = new StringBuilder();
            sb.AppendLine($"__**Members Ordered By Donations**__: \n{donationList}");

            if (currentWar.State == WarState.Ended)
            {
                var missedAttackers = warMembers.Where(x => x.Attacks.Count == 0);

                var missedList = string.Join('\n', missedAttackers.Select(x => Format.Sanitize(x.Name)));

                sb.AppendLine($"\n\n__**Missed Attackers**__:\n{missedList}");
            }

            return sb.ToString();
        }
    }
}
