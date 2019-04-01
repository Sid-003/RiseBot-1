using ClashWrapper.Entities;
using ClashWrapper.Entities.ClanMembers;
using ClashWrapper.Entities.Player;
using ClashWrapper.Entities.War;
using ClashWrapper.Entities.WarLog;
using ClashWrapper.Models.ClanMembers;
using ClashWrapper.Models.Player;
using ClashWrapper.Models.War;
using ClashWrapper.Models.WarLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashWrapper
{
    public class ClashClient
    {
        private readonly RequestClient _request;

        public ClashClient(ClashClientConfig config)
        {
            _request = new RequestClient(this, config);
        }

        public event Func<ErrorMessage, Task> Error;

        internal Task InternalErrorReceivedAsync(ErrorMessage message)
        {
            return Error is null ? Task.CompletedTask : Error(message);
        }

        public event Func<string, Task> Log;

        internal Task InternalLogReceivedAsync(string message)
        {
            return Log is null ? Task.CompletedTask : Log(message);
        }

        public async Task<CurrentWar> GetCurrentWarAsync(string clanTag)
        {
            if(string.IsNullOrWhiteSpace(clanTag))
                throw new ArgumentNullException(clanTag);

            clanTag = clanTag[0] == '#' ? clanTag.Replace("#", "%23") : clanTag;

            var model = await _request.SendAsync<CurrentWarModel>($"/v1/clans/{clanTag}/currentwar")
                .ConfigureAwait(false);

            return model is null ? null : new CurrentWar(model);
        }

        public async Task<PagedEntity<IReadOnlyCollection<WarLog>>> GetWarLogAsync(string clanTag, int? limit = null,
            string before = null, string after = null)
        {
            if (string.IsNullOrWhiteSpace(clanTag))
                throw new ArgumentNullException(clanTag);

            if(limit < 0)
                throw new ArgumentOutOfRangeException(nameof(limit));

            clanTag = clanTag[0] == '#' ? clanTag.Replace("#", "%23") : clanTag;

            var sb = new StringBuilder();
            sb.Append($"/v1/clans/{clanTag}/warlog?");

            if (limit.HasValue)
                sb.Append($"limit={limit.Value}&");

            if (!string.IsNullOrWhiteSpace(before))
                sb.Append($"before={before}&");

            if (!string.IsNullOrWhiteSpace(after))
                sb.Append($"after={after}&");

            var model = await _request.SendAsync<PagedWarlogModel>(sb.ToString()).ConfigureAwait(false);

            if (model is null)
            {
                var empty = ReadOnlyCollection<WarLog>.EmptyCollection();

                return new PagedEntity<IReadOnlyCollection<WarLog>>
                {
                    Entity = empty
                };
            }

            var collection = new ReadOnlyCollection<WarLog>(model.WarLogs.Select(x => new WarLog(x)),
                () => model.WarLogs.First().TeamSize);

            var paged = new PagedEntity<IReadOnlyCollection<WarLog>>
            {
                After = model.Paging.Cursors.After,
                Before = model.Paging.Cursors.Before,
                Entity = collection
            };

            return paged;
        }

        public async Task<IReadOnlyCollection<ClanMember>> GetClanMembersAsync(string clanTag)
        {
            if (string.IsNullOrWhiteSpace(clanTag))
                throw new ArgumentNullException(clanTag);

            clanTag = clanTag[0] == '#' ? clanTag.Replace("#", "%23") : clanTag;

            var model = await _request.SendAsync<PagedClanMembersModel>($"/v1/clans/{clanTag}/members")
                .ConfigureAwait(false);

            if (model is null)
                return ReadOnlyCollection<ClanMember>.EmptyCollection();

            var collection = new ReadOnlyCollection<ClanMember>(model.ClanMembers.Select(x => new ClanMember(x)),
                () => model.ClanMembers.Length);

            return collection;
        }

        public async Task<Player> GetPlayerAsync(string playerTag)
        {
            if (string.IsNullOrWhiteSpace(playerTag))
                throw new ArgumentNullException(playerTag);

            playerTag = playerTag[0] == '#' ? playerTag.Replace("#", "%23") : playerTag;

            var model = await _request.SendAsync<PlayerModel>($"/v1/players/{playerTag}")
                .ConfigureAwait(false);

            if (model is null)
                return null;

            return new Player(model);
        }
    }
}
