using System.Drawing;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Teleport : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Teleport.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageTeleport.dae");

        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
        }

        protected override int MaxLevel => 15;
        public override float MaxCooldown => 54;
        public override float ManaCost => 80;
        public override string Description => Translations.Get("teleport_desc");
        public override string DisplayName => Translations.Get("teleport_skill");
    }
}