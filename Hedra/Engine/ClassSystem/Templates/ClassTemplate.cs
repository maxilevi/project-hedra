using System.Collections.Generic;
using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.Engine.ClassSystem.Templates
{
    public class ClassTemplate : INamedTemplate
    {
        public string Name { get; set; }
        public HumanoidModelTemplate Model { get; set; }
        public HumanoidModelTemplate HeadModel { get; set; }
        public HumanoidModelTemplate ChestModel { get; set; }
        public HumanoidModelTemplate LegsModel { get; set; }
        public HumanoidModelTemplate FeetModel { get; set; }
        public HumanoidModelTemplate FemaleHeadModel { get; set; }
        public HumanoidModelTemplate FemaleChestModel { get; set; }
        public HumanoidModelTemplate FemaleLegsModel { get; set; }
        public HumanoidModelTemplate FemaleFeetModel { get; set; }
        public string DefaultSkinColor { get; set; }
        public string DefaultFirstHairColor { get; set; }
        public string DefaultSecondHairColor { get; set; }
        public string FemaleDefaultSkinColor { get; set; }
        public string FemaleDefaultFirstHairColor { get; set; }
        public string FemaleDefaultSecondHairColor { get; set; }
        public string Logo { get; set; }
        public float BaseSpeed { get; set; }
        public string MainAbilityTree { get; set; }
        public string FirstSpecializationTree { get; set; }
        public string SecondSpecializationTree { get; set; }
        public StartingItemTemplate[] StartingItems { get; set; }
        public string[] StartingRecipes { get; set; } = new string[0];
        public float AttackResistance { get; set;}
        public float MaxStamina { get; set; }
        public float BaseDamage { get; set; }
        public float AttackingSpeedModifier { get; set; }
        public float BaseHealthPerLevel { get; set; }
        public float BaseManaPerLevel { get; set; }
        public bool IsRanged { get; set; }
        public float BaseHealth { get; set; }
    }
}