using Model = ClashWrapper.Models.ClanMembers.ClanMemberModel;

namespace ClashWrapper.Entities.ClanMembers
{
    public sealed class ClanMember
    {
        private readonly Model _model;

        internal ClanMember(Model model)
        {
            _model = model;
        }

        public string Tag => _model.Tag;
        public string Name => _model.Name;

        public ClanRole Role
        {
            get
            {
                switch (_model.Role)
                {
                    case "member":
                        return ClanRole.Member;

                    case "leader":
                        return ClanRole.Leader;

                    case "coLeader":
                        return ClanRole.CoLeader;

                    case "admin":
                        return ClanRole.Elder;

                    default:
                        return ClanRole.Unknown;
                }
            }
        }

        public int Level => _model.Level;
        public int Trophies => _model.Trophies;
        public int VersusTrophies => _model.VersusTrophies;
        public int ClanRank => _model.ClanRank;
        public int PreviousClanRank => _model.PreviousClanRank;
        public int Donations => _model.Donations;
        public int Received => _model.DonationsReceived;

        private League _league;
        public League League => _league ?? (_league = new League(_model.League));
    }

    public enum ClanRole
    {
        Unknown,
        Member,
        Elder,
        CoLeader,
        Leader
    }
}
