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
        public override HumanType Human => throw new ArgumentException();
        public override float BaseSpeed => throw new ArgumentException();
        public override AbilityTreeBlueprint AbilityTreeDesign => throw new ArgumentException();
        public override Item StartingItem => throw new ArgumentException();
        public override float AttackResistance => throw new ArgumentException();
        public override float MaxStamina => throw new ArgumentException();
        public override float BaseDamage => throw new ArgumentException();

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
