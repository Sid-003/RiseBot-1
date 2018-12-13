using ClashWrapper;
using ClashWrapper.Entities.WarLog;
using RiseBot.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot
{
    public static class Utilites
    {
        public static async Task<BaseLottoResult> CalculateWarWinnerAsync(ClashClient client, string clanTag)
        {
            var currentWar = await client.GetCurrentWarAsync(clanTag);

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

            var clanTotal = CalculateTotal(clanWarLog);
            var opponentTotal = CalculateTotal(opponentWarLog);

            var draw = clanTotal == opponentTotal;

            if (!draw)
            {
                return new LottoResult
                {
                    ClanName = currentWar.Clan.Name,
                    ClanTag = clanTag,
                    OpponentName = currentWar.Opponent.Name,
                    OpponentTag = opponentTag,
                    ClanWin = clanTotal > opponentTotal
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
                HighSyncWinnerTag = result < 0 ? clanTag : opponentTag
            };
        }

        private static int CalculateTotal(IEnumerable<WarLog> warLog)
        {
            var filtered = warLog.Where(x => x.Opponent.Level > 0);
            var trimmed = filtered.Take(7);

            return trimmed.Count(x => x.Result == WarResult.Lose);
        }
    }
}
