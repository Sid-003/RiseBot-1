using ClashWrapper.Entities;
using ClashWrapper.Entities.War;
using ClashWrapper.Entities.WarLog;
using ClashWrapper.Models.War;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClashWrapper.Models.WarLog;

namespace ClashWrapper
{
    public class ClashClient
    {
        private readonly RequestClient _request;

        public ClashClient(ClashClientConfig config)
        {
            _request = new RequestClient(this, config);
        }

        public event Func<ErrorMessage, Task> ErrorReceived;

        internal Task InternalErrorReceivedAsync(ErrorMessage message)
        {
            return ErrorReceived is null ? Task.CompletedTask : ErrorReceived.Invoke(message);
        }

        public async Task<CurrentWar> GetCurrentWarAsync(string clanTag)
        {
            if(string.IsNullOrWhiteSpace(clanTag))
                throw new ArgumentNullException(clanTag);

            clanTag = clanTag[0] == '#' ? clanTag.Replace("#", "%23") : clanTag;

            var model = await _request.SendAsync<CurrentWarModel>($"/clans/{clanTag}/currentwar")
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
            sb.Append($"/clans/{clanTag}/warlog?");

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
    }
}
