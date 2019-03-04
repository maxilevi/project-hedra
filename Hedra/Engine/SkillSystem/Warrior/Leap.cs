using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.SkillSystem.Archer.Scout;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class Leap : Retreat
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Leap.png");
        protected override Vector3 JumpDirection => -base.JumpDirection;
        public override string Description => Translations.Get("leap_desc");
        public override string DisplayName => Translations.Get("leap_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("leap_distance_change", Distance.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}