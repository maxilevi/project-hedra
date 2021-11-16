using System.Numerics;
using Hedra.Engine.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class BearCompanion : CompanionSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BearCompanion.png");
        protected override string Keyword => "bear";

        protected override MobType CompanionType => MobType.Bear;

        protected override void SpawnEffect(Vector3 TargetPosition)
        {
            SkillUtils.SpawnParticles(TargetPosition, Colors.Brown);
        }
    }
}