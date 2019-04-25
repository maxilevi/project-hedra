using System;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem
{
    public abstract class ActivateDurationSkillWithSingleAnimation : SingleAnimationSkill<IPlayer>
    {
        private readonly ActivateDurationSkillComposition _composition;

        protected ActivateDurationSkillWithSingleAnimation()
        {
            _composition = new ActivateDurationSkillComposition
            {
                DurationPublic = Duration,
                CooldownDurationPublic = CooldownDuration,
                DoEnablePublic = DoEnable,
                DoDisablePublic = DoDisable
            };
        }

        public override void Update()
        {
            base.Update();
            _composition.Update();
        }

        protected override void DoUse()
        {
            base.DoUse();
            _composition.DoUsePublic();
        }

        protected abstract void DoEnable();

        protected abstract void DoDisable();
        
        protected abstract float Duration { get; }
        
        protected abstract float CooldownDuration { get; }

        public sealed override float MaxCooldown => _composition.MaxCooldown;

        private class ActivateDurationSkillComposition : ActivateDurationSkill<IPlayer>
        {
            public override string Description => throw new NotImplementedException();
            public override string DisplayName => throw new NotImplementedException();
            public override float ManaCost => throw new NotImplementedException();
            public override uint IconId => throw new NotImplementedException();
            protected override int MaxLevel => 1;
            protected override void DoEnable() => DoEnablePublic();
            protected override void DoDisable() => DoDisablePublic();
            protected override float Duration => DurationPublic;
            protected override float CooldownDuration => CooldownDurationPublic;
            
            public void DoUsePublic() => DoUse();
            public float DurationPublic { get; set; }
            public float CooldownDurationPublic { get; set; }
            public Action DoEnablePublic { get; set; }
            public Action DoDisablePublic { get; set; }
        }
    }
}