using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage
{
    public abstract class RadiusEffectSkill : SingleAnimationSkill
    {
        protected sealed override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageStaffGroundHit.dae");
        protected sealed override bool CanMoveWhileCasting => false;
        protected sealed override float AnimationSpeed => .75f;
    }
}