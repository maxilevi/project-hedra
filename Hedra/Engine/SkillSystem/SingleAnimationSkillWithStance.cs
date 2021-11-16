using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SingleAnimationSkillWithStance : SingleAnimationSkill<IPlayer>
    {
        protected abstract Animation StanceAnimation { get; }
        protected override bool ShouldDisable => IsActive;
        protected override bool CanMoveWhileCasting => false;
        protected abstract bool ShouldQuitStance { get; }
        protected bool IsActive { get; private set; }

        public override float IsAffectingModifier => IsActive ? 1 : 0;

        public override void Update()
        {
            base.Update();
            if (IsActive)
            {
                User.Model.PlayAnimation(StanceAnimation);
                User.LeftWeapon.InAttackStance = true;
                if (ShouldQuitStance)
                    End();
            }
        }

        protected void Start()
        {
            IsActive = true;
            Cooldown = 0;
            DoStart();
            InvokeStateUpdated();
        }

        protected void End()
        {
            IsActive = false;
            User.Model.Reset();
            DoEnd();
            Cooldown = MaxCooldown;
        }

        protected override void OnAnimationEnd()
        {
            Start();
        }

        protected abstract void DoStart();
        protected abstract void DoEnd();
    }
}