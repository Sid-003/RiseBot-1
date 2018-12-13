using Model = ClashWrapper.Models.WarLog.WarLogClanModel;

namespace ClashWrapper.Entities.WarLog
{
    public class WarLogClan
    {
        private readonly Model _model;

        internal WarLogClan(Model model)
        {
            _model = model;
        }

        public string Tag => _model.Tag;
        public string Name => _model.Name;

        private BadgeUrls _badgeUrls;
        public BadgeUrls BadgeUrls => _badgeUrls ?? (_badgeUrls = new BadgeUrls(_model.BadgeUrls));

        public int Level => _model.ClanLevel;
        public int Attacks => _model.Attacks;
        public int Stars => _model.Stars;

        public double DestructionPercentage => _model.DestructionPercentage;

        public int? ExperienceEarned => _model.ExperienceEarned;
    }
}
