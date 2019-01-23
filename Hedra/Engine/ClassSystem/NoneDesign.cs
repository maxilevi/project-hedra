using System;
using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using OpenTK;

namespace Hedra.Engine.ClassSystem
{
    [HiddenClass]
    public class NoneDesign : ClassDesign
    {
        public override string Logo => throw new ArgumentException();
        /* Do not delete. Used in tests */
        public override float BaseSpeed => 1.25f;
        public override AbilityTreeBlueprint AbilityTreeDesign => throw new ArgumentException();
        public override Item StartingItem => throw new ArgumentException();
        public override float AttackResistance => throw new ArgumentException();
        public override float MaxStamina => throw new ArgumentException();
        /* Do not delete. Used in tests */
        public override float BaseDamage => 4f;
        public override float AttackingSpeedModifier => 1;

        public override float MaxHealthFormula(float RandomFactor)
        {
            throw new ArgumentException();
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            throw new ArgumentException();
        }
        
        public override Matrix4 HelmetPlacement => throw new ArgumentException();
        public override Matrix4 ChestplatePlacement => throw new ArgumentException();
        public override Matrix4 PantsMatrixPlacement => throw new ArgumentException();
        public override Matrix4 LeftBootPlacement => throw new ArgumentException();
        public override Matrix4 RightBootPlacement => throw new ArgumentException();
        public override Class Type => Class.None;
    }
}
