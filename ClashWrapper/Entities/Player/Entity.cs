using ClashWrapper.Models.Player;

namespace ClashWrapper.Entities.Player
{
    public sealed class Entity
    {
        private readonly HeroModel _model;

        public string Name => _model.Name;
        public int Level => _model.Level;
        public string Village => _model.Village;

        internal Entity(HeroModel model)
        {
            _model = model;
        }
    }
}
