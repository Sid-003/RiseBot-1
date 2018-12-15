using System.Collections.Generic;
using System.Linq;
using Model = ClashWrapper.Models.War.WarMemberModel;

namespace ClashWrapper.Entities.War
{
    public class WarMember
    {
        private readonly Model _model;

        internal WarMember(Model model)
        {
            _model = model;
        }

        public string Tag => _model.Tag;
        public string Name => _model.Name;

        public int TownhallLevel => _model.TownhallLevel;
        public int MapPosition => _model.MapPosition;
        public int OpponentAttack => _model.OpponentAttacks;

        private WarAttack _bestOppenentAttack;
        public WarAttack BestOpponentAttack =>
            _bestOppenentAttack ?? (_bestOppenentAttack = new WarAttack(_model.BestOpponentAttack));

        private IReadOnlyCollection<WarAttack> _attacks;
        public IReadOnlyCollection<WarAttack> Attacks
        {
            get
            {
                if (!(_attacks is null))
                    return _attacks;

                if (_model.Attacks is null)
                    return _attacks = ReadOnlyCollection<WarAttack>.EmptyCollection();

                return _attacks = new ReadOnlyCollection<WarAttack>(_model.Attacks.Select(x => new WarAttack(x)),
                    () => _model.Attacks.Length);
            }
        }
    }
}
