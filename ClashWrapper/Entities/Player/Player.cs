using ClashWrapper.Models.Player;
using System.Collections.Generic;
using System.Linq;

namespace ClashWrapper.Entities.Player
{
    public sealed class Player
    {
        private readonly PlayerModel _model;

        public string Tag => _model.Tag;
        public string Name => _model.Name;
        public int TownHallLevel => _model.TownHallLevel;

        private IReadOnlyCollection<Entity> _heroes;
        public IReadOnlyCollection<Entity> Heroes
        {
            get
            {
                if(_heroes is null)
                {
                    _heroes = 
                        new ReadOnlyCollection<Entity>(_model.Heroes.Select(x => new Entity(x)), () => _model.Heroes.Length);
                }

                return _heroes;
            }
        }

        private IReadOnlyCollection<Entity> _troops;
        public IReadOnlyCollection<Entity> Troops
        {
            get
            {
                if (_troops is null)
                {
                    _troops =
                        new ReadOnlyCollection<Entity>(_model.Troops.Select(x => new Entity(x)), () => _model.Troops.Length);
                }

                return _troops;
            }
        }

        private IReadOnlyCollection<Entity> _spells;
        public IReadOnlyCollection<Entity> Spells
        {
            get
            {
                if (_spells is null)
                {
                    _spells =
                        new ReadOnlyCollection<Entity>(_model.Spells.Select(x => new Entity(x)), () => _model.Spells.Length);
                }

                return _spells;
            }
        }

        internal Player(PlayerModel model)
        {
            _model = model;
        }
    }
}
