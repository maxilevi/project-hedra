using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.Engine.ClassSystem.Templates
{
    public class ClassTemplate : INamedTemplate
    {
        public string Name { get; set; }
        public HumanoidModelTemplate Model { get; set; }
        public string Logo { get; set; }
        public float BaseSpeed { get; set; }
        public string MainAbilityTree { get; set; }
        public string FirstSpecializationTree { get; set; }
        public string SecondSpecializationTree { get; set; }
        public string StartingItem { get; set; }
        public string[] StartingRecipes { get; set; } = new string[0];
        public float AttackResistance { get; set;}
        public float MaxStamina { get; set; }
        public float BaseDamage { get; set; }
        public float AttackingSpeedModifier { get; set; }
        public float BaseHealthPerLevel { get; set; }
        public float BaseManaPerLevel { get; set; }
    }
}