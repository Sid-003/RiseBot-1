using Model = ClashWrapper.Models.War.WarAttackModel;

namespace ClashWrapper.Entities.War
{
    public class WarAttack
    {
        private readonly Model _model;

        internal WarAttack(Model model)
        {
            _model = model;
        }

        public string AttackerTag => _model.AttackerTag;
        public string DefenderTag => _model.DefenderTag;

        public int Stars => _model.Stars;
        public int Destruction => _model.DestructionPercentage;
        public int Order => _model.Order;
    }
}
