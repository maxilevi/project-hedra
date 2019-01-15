using System;
using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ClassSystem
{
    public class ArcherDesign : ClassDesign
    {
        public override string Logo => "Assets/UI/ArcherLogo.png";
        public override HumanType Human => HumanType.Archer;
        public override float BaseSpeed => 1.4f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new ArcherAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonWoodenBow);
        public override float AttackResistance => 0.975f;
        public override float MaxStamina => 115;
        public override float BaseDamage => 2.75f;
        public override float AttackingSpeedModifier => .15f;

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 8f + RandomFactor * 1f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 9f + RandomFactor;
        }
        
        public override Matrix4 HelmetPlacement { get; } = Matrix4.Identity;
        public override Matrix4 ChestplatePlacement { get; } = Matrix4.Identity;
        public override Matrix4 PantsMatrixPlacement { get; } = Matrix4.Identity;
        public override Matrix4 LeftBootPlacement { get; } = Matrix4.Identity;
        public override Matrix4 RightBootPlacement { get; } = Matrix4.Identity;
        public override Class Type => Class.Archer;
    }
}
