using Hedra.API;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.WorldBuilding
{
    public class BanditOptions
    {
        private MissionBuilder _questBuilder;
        public Class PossibleClasses { get; set; } = Class.Archer | Class.Mage | Class.Warrior | Class.Rogue;
        public HumanType? ModelType { get; set; }
        public bool Friendly { get; set; }
        public bool IsFromQuest => _questBuilder != null;

        public static BanditOptions Default => new BanditOptions
        {
            Friendly = false,
            ModelType = Utils.Rng.Next(0, 7) == 1 ? HumanType.Gnoll :
                Utils.Rng.Next(0, 7) == 1 ? (HumanType?)HumanType.Beasthunter : null
        };

        public static BanditOptions Undead => new BanditOptions
        {
            Friendly = false,
            ModelType = Utils.Rng.Next(0, 7) == 1 ? HumanType.VillagerGhost : HumanType.Skeleton
        };

        public static BanditOptions Quest(MissionBuilder Builder)
        {
            var options = Default;
            options._questBuilder = Builder;
            return options;
        }

        public void ApplyQuestStatus(IHumanoid Humanoid)
        {
            if (!IsFromQuest) return;
            Humanoid.Removable = false;
            _questBuilder.MissionDispose += () =>
            {
                Humanoid.Removable = true;
                Humanoid.AddComponent(new DisposeComponent(Humanoid, 1024));
                _questBuilder = null;
            };
        }
    }
}