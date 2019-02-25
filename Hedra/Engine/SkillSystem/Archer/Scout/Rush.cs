using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Rush : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Rush.png");
        public override void Use()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {

        }

        private float Duration => 5;
        public override string Description => Translations.Get("rush_desc", Duration.ToString("0.0", CultureInfo.InvariantCulture));
        public override string DisplayName => Translations.Get("rush_skill");
    }
}