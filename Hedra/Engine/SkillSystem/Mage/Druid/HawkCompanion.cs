using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class HawkCompanion : CompanionSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/HawkCompanion.png");
        
        protected override void SpawnEffect(Vector3 TargetPosition)
        {
            SkillUtils.SpawnParticles(TargetPosition, Vector4.One);
        }

        protected override MobType CompanionType => MobType.Bee;
        
        protected override string Keyword => "hawk";
    }
}