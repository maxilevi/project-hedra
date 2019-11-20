using Hedra.API;
using Hedra.Engine.Player;

namespace Hedra.Engine.WorldBuilding
{
    public class BanditOptions
    {
        public Class PossibleClasses { get; set; } = Class.Archer | Class.Mage | Class.Warrior | Class.Rogue;
        public HumanType? ModelType { get; set; }
        public bool Friendly { get; set; }

        public static BanditOptions Default => new BanditOptions
        {
            Friendly = false,
            ModelType = Utils.Rng.Next(0, 7) == 1 ? HumanType.Gnoll : Utils.Rng.Next(0, 7) == 1 ? (HumanType?) HumanType.Beasthunter : null
        };

        public static BanditOptions Undead => new BanditOptions
        {
            Friendly = false,
            ModelType = Utils.Rng.Next(0, 7) == 1 ? HumanType.VillagerGhost : HumanType.Skeleton
        };
    }
}