using System.Collections.Generic;
using System.Linq;
using Model = ClashWrapper.Models.War.WarClanModel;

namespace ClashWrapper.Entities.War
{
    public class WarClan
    {
        private readonly Model _model;

        internal WarClan(Model model)
        {
            _model = model;
        }

        public string Tag => _model.Tag;
        public string Name => _model.Name;

        private BadgeUrls _badgeUrls;
        public BadgeUrls BadgeUrls => _badgeUrls ?? (_badgeUrls = new BadgeUrls(_model.BadgeUrls));

        public int Level => _model.ClanLevel;
        public int Attacked => _model.Attacks;
        public int Starts => _model.Stars;

        public double Destruction => _model.DestructionPercentage;

        private IReadOnlyCollection<WarMember> _members;
        public IReadOnlyCollection<WarMember> Members
        {
            get
            {
                return _members ?? (_members = new ReadOnlyCollection<WarMember>(
                           _model.Members?.Select(x => new WarMember(x)),
                           () => _model.Members.Length));
            }
        }
    }
}
