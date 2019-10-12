using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class BearCompanion : CompanionSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BearCompanion.png");
        protected override string Keyword => "bear";
        
        protected override void SpawnEffect(Vector3 TargetPosition)
        {
            SkillUtils.SpawnParticles(TargetPosition, Colors.Brown);
        }

        protected override MobType CompanionType => MobType.Bear;
    }
}