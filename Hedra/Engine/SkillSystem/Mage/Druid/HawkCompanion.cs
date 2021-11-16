using System.Numerics;
using Hedra.Engine.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class HawkCompanion : CompanionSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/HawkCompanion.png");

        protected override MobType CompanionType => MobType.Crow;

        protected override string Keyword => "hawk";

        protected override void SpawnEffect(Vector3 TargetPosition)
        {
            SkillUtils.SpawnParticles(TargetPosition, Vector4.One);
        }
    }
}