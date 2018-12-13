using System;
using Model = ClashWrapper.Models.WarLog.WarLogModel;

namespace ClashWrapper.Entities.WarLog
{
    public class WarLog
    {
        private readonly Model _model;

        internal WarLog(Model model)
        {
            _model = model;
        }

        public WarResult Result
        {
            get
            {
                switch (_model.Result)
                {
                    case "lose":
                        return WarResult.Lose;

                    case "tie":
                        return WarResult.Tie;

                    case "win":
                        return WarResult.Win;

                    default:
                        return WarResult.Default;
                }
            }
        }

        public DateTimeOffset EndTime => Utilities.FromClashTime(_model.EndTime);

        public int TeamSize => _model.TeamSize;

        private WarLogClan _clan;
        public WarLogClan Clan => _clan ?? (_clan = new WarLogClan(_model.Clan));

        private WarLogClan _opponent;
        public WarLogClan Opponent => _opponent ?? (_opponent = new WarLogClan(_model.Opponent));
    }

    public enum WarResult
    {
        Default,
        Lose,
        Tie,
        Win
    }
}
