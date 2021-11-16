using Hedra.Engine.ClassSystem;

namespace Hedra.Engine.ModuleSystem.Templates
{
    public class HumanoidTemplate
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Behaviour { get; set; }
        public string Class { get; set; } = ClassDesign.None.ToString();
        public bool Immune { get; set; }
        public float XP { get; set; }
        public HumanoidWeaponTemplate[] Weapons { get; set; }
        public HumanoidModelTemplate Model { get; set; }
        public HumanoidModelTemplate[] Models { get; set; }
        public HumanoidModelTemplate RandomModel => Models?[Utils.Rng.Next(0, Models.Length)] ?? Model;
        public HumanoidComponentsItemTemplate[] Components { get; set; } = new HumanoidComponentsItemTemplate[0];

        public HumanoidTemplate Clone()
        {
            return new HumanoidTemplate
            {
                Name = Name,
                DisplayName = DisplayName,
                Behaviour = Behaviour,
                Class = Class,
                Immune = Immune,
                Weapons = Weapons,
                Model = Model,
                Models = Models,
                Components = Components
            };
        }
    }
}