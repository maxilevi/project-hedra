
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
        public HumanoidWeaponTemplate[] Weapons { get; set; }
        public HumanoidModelTemplate Model { get; set; }
        public HumanoidComponentsItemTemplate[] Components { get; set; } = new HumanoidComponentsItemTemplate[0];
    }
}
