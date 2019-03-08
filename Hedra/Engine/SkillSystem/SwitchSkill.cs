using Hedra.Core;
using Hedra.Engine.Rendering.Animation;
using Hedra.Sound;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SwitchSkill : CappedSkill
    {
        private readonly Animation _stance;
        private float _lerpBlending;
        protected abstract string AnimationPath { get; }
        protected abstract SoundType SoundType { get; }
        protected virtual bool Orientate => true;
        protected override float OverlayBlending => _lerpBlending;
        public override bool PlaySound => false;
        protected virtual bool HasAnimation => true;
        protected virtual bool HasSound => true; 

        protected SwitchSkill()
        {
            if (HasAnimation)
            {
                _stance = AnimationLoader.LoadAnimation(AnimationPath);
                _stance.Loop = false;
                _stance.OnAnimationEnd += delegate
                {
                    if (!Casting) return;
                    Player.Model.PlayAnimation(_stance);
                    Player.Model.BlendAnimation(_stance);
                };
            }
        }

        protected override void DoUse()
        {
            Casting = true;
            if(HasSound) SoundPlayer.PlaySoundWhile(SoundType, () => Casting, () => 1, () => 1);
            if (HasAnimation)
            {
                Player.Model.PlayAnimation(_stance);
                Player.Model.BlendAnimation(_stance);
            }
            Activate();
        }

        public override void KeyUp()
        {
            if (!Casting) return;
            Casting = false;
            Deactivate();
        }

        protected abstract void Deactivate();
        
        protected abstract void Activate();
        
        public override void Update()
        {
            _lerpBlending = Mathf.Lerp(_lerpBlending, Casting ? 1f : 0f, Time.DeltaTime * 2f);
            if (!Casting) return;
            if (Orientate) Player.Movement.Orientate();
        }

        public override float IsAffectingModifier => Casting ? 1 : 0;
        public sealed override float ManaCost => 0;
        public sealed override float MaxCooldown => 0;
        protected sealed override bool HasCooldown => false;
    }
}