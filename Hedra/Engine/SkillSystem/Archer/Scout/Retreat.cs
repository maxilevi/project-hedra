using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Retreat : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Retreat.png");
        public override void Use()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {

        }

        private float Distance => 5;
        public override string Description => Translations.Get("retreat_desc");
        public override string DisplayName => Translations.Get("retreat_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("retreat_distance_change", Distance.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}