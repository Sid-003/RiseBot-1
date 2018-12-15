using Model = ClashWrapper.Models.ClanMembers.LeagueModel;

namespace ClashWrapper.Entities.ClanMembers
{
    public sealed class League
    {
        private readonly Model _model;

        internal League(Model model)
        {
            _model = model;
        }

        public int Id => _model.Id;

        public string Name => _model.Name;

        private IconUrls _iconUrls;
        public IconUrls IconUrls => _iconUrls ?? (_iconUrls = new IconUrls(_model.IconUrls));
    }
}
