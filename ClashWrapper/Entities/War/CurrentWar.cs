using System;
using Model = ClashWrapper.Models.War.CurrentWarModel;

namespace ClashWrapper.Entities.War
{
    public class CurrentWar
    {
        private readonly Model _model;

        internal CurrentWar(Model model)
        {
            _model = model;
        }

        public WarState State
        {
            get
            {
                switch (_model.State)
                {
                    case "preparation":
                        return WarState.Preparation;

                    case "inWar":
                        return WarState.InWar;

                    case "warEnded":
                        return WarState.Ended;

                    default:
                        return WarState.Default;
                }
            }
        }

        public int Size => _model.TeamSize;

        public DateTimeOffset PreparationTime => Utilities.FromClashTime(_model.PreparationStartTime);
        public DateTimeOffset StartTime => Utilities.FromClashTime(_model.StartTime);
        public DateTimeOffset EndTime => Utilities.FromClashTime(_model.EndTime);

        private WarClan _clan;
        public WarClan Clan => _clan ?? (_clan = new WarClan(_model.Clan));

        private WarClan _opponent;
        public WarClan Opponent => _opponent ?? (_opponent = new WarClan(_model.Opponent));
    }

    public enum WarState
    {
        Default,
        Preparation,
        InWar,
        Ended
    }
}
