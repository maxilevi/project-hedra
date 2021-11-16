using System;
using System.Collections.Generic;
using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.AbilityTreeSystem;

namespace Hedra.Engine.ClassSystem
{
    [HiddenClass]
    public class NoneDesign : ClassDesign
    {
        public override string Logo => throw new ArgumentException();

        /* Do not delete. Used in tests */
        public override float BaseSpeed => 1.25f;
        public override AbilityTreeBlueprint MainTree => throw new ArgumentException();
        public override AbilityTreeBlueprint FirstSpecializationTree => throw new ArgumentException();
        public override AbilityTreeBlueprint SecondSpecializationTree => throw new ArgumentException();
        public override KeyValuePair<int, Item>[] StartingItems => throw new ArgumentException();

        public override Item[] StartingRecipes => throw new ArgumentException();

        /* Do not delete. Used in tests */
        public override float AttackResistance => 1f;

        public override float MaxStamina => throw new ArgumentException();

        /* Do not delete. Used in tests */
        public override float BaseDamage => 4f;
        public override float AttackingSpeedModifier => 1;

        public override Class Type => Class.None;

        public override float MaxHealthFormula(float RandomFactor)
        {
            throw new ArgumentException();
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            throw new ArgumentException();
        }
    }
}