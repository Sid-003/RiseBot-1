using Newtonsoft.Json;

namespace ClashWrapper.Models.ClanMembers
{
    internal class PagedClanMembersModel
    {
        [JsonProperty("items")]
        public ClanMemberModel[] ClanMembers { get; set; }

        [JsonProperty("paging")]
        public PagingModel Paging { get; set; }
    }
}
