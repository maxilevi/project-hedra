using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SingleAnimationSkillWithStance : SingleAnimationSkill<IPlayer>
    {
        protected abstract Animation StanceAnimation { get; }
        protected override bool ShouldDisable => _isActive;
        protected override bool CanMoveWhileCasting => false;
        private bool _isActive;
        
        public override void Update()
        {
            base.Update();
            if (_isActive)
            {
                User.Model.PlayAnimation(StanceAnimation);
                User.LeftWeapon.InAttackStance = true;
                if (ShouldQuitStance)
                    End();
            }
        }

        protected void Start()
        {
            _isActive = true;
            Cooldown = 0;
            DoStart();
            InvokeStateUpdated();
        }

        protected void End()
        {
            _isActive = false;
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
        protected abstract bool ShouldQuitStance { get; }
        protected bool IsActive => _isActive;
        public override float IsAffectingModifier => _isActive ? 1 : 0;
    }
}