namespace ClashWrapper.Models.Player
{
    internal class PlayerModel
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int TownHallLevel { get; set; }
        public HeroModel[] Heroes { get; set; }
        public TroopModel[] Troops { get; set; }
        public SpellModel[] Spells { get; set; }
    }

    internal class HeroModel
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public string Village { get; set; }
    }

    internal class TroopModel : HeroModel
    {
    }

    internal class SpellModel : HeroModel
    {
    }
}
