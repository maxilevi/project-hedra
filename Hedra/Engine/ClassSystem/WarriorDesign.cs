using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ClassSystem
{
    public class WarriorDesign : ClassDesign
    {
        public override string Logo => "Assets/UI/WarriorLogo.png";
        public override HumanType Human => HumanType.Warrior;
        public override float BaseSpeed => 1.25f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new WarriorAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonBronzeSword);
        public override float AttackResistance => 1.075f;
        public override float MaxStamina => 100;

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 9.5f + RandomFactor;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 7f + RandomFactor;
        }
    }
}
