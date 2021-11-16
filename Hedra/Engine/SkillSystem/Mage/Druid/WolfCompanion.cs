using System.Numerics;
using Hedra.Engine.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class WolfCompanion : CompanionSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/WolfCompanion.png");
        protected override string Keyword => "wolf";

        protected override MobType CompanionType => MobType.Wolf;

        protected override void SpawnEffect(Vector3 TargetPosition)
        {
            SkillUtils.SpawnParticles(TargetPosition, Vector4.One * .5f);
        }
    }
}