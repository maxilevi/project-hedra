using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SpikeTrap : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SpikeTrap.png");
        protected override int MaxLevel { get; }
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
        
        protected override void OnExecution()
        {
            throw new System.NotImplementedException();
        }
        
        public override string Description => Translations.Get("spike_trap_desc");
        public override string DisplayName => Translations.Get("spike_trap");
    }
}