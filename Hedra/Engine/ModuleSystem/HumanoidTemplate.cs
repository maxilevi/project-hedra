
using Hedra.Engine.ClassSystem;

namespace Hedra.Engine.ModuleSystem
{
    public class HumanoidTemplate
    {
        public string Name { get; set; }
        public float XP { get; set; } = 1.0f;
        public float MaxHealth { get; set; }
        public string DisplayName { get; set; }
        public string Behaviour { get; set; }
        public string Class { get; set; } = ClassDesign.None.ToString();
        public string ModelType { get; set; }
        public bool Immune { get; set; }
        public float Speed { get; set; }
        public float AttackPower { get; set; }
        public float AttackSpeed { get; set; }
        public HumanoidWeaponTemplate[] Weapons { get; set; }

        public HumanoidModelTemplate Model => HumanoidLoader.ModelTemplater[ModelType];
    }
}
