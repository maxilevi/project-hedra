using Hedra.Core;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.Engine.SkillSystem
{
    public abstract class PlayerSwitchSkill : SwitchSkill<IPlayer>
    {
    }

    public abstract class SwitchSkill<T> : CappedSkill<T>
        where T : class, IObjectWithAnimation, ISkillUser, IObjectWithMovement, IObjectWithWeapon, IObjectWithLifeCycle
    {
        private readonly Animation _stance;
        private float _lerpBlending;
        private int _previousLevel;

        protected SwitchSkill()
        {
            if (HasAnimation)
            {
                _stance = AnimationLoader.LoadAnimation(AnimationPath);
                _stance.Loop = false;
                _stance.OnAnimationEnd += delegate
                {
                    if (!Casting) return;
                    User.PlayAnimation(_stance);
                    User.BlendAnimation(_stance);
                };
            }
        }

        protected abstract string AnimationPath { get; }
        protected abstract SoundType SoundType { get; }
        protected virtual bool Orientate => true;
        protected override float OverlayBlending => _lerpBlending;
        public override bool PlaySound => false;
        protected virtual bool HasAnimation => true;
        protected virtual bool HasSound => true;

        public override float IsAffectingModifier => Casting ? 1 : 0;
        public sealed override float ManaCost => 0;
        public sealed override float MaxCooldown => 0;
        protected sealed override bool HasCooldown => false;

        protected override void DoUse()
        {
            if (!Casting) InvokeStateUpdated();
            Casting = true;
            if (HasSound) SoundPlayer.PlaySoundWhile(SoundType, () => Casting, () => 1, () => 1);
            if (HasAnimation)
            {
                User.PlayAnimation(_stance);
                User.BlendAnimation(_stance);
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
            if (_previousLevel != Level)
            {
                if (Level == 0 && Casting) KeyUp();
                _previousLevel = Level;
            }

            _lerpBlending = Mathf.Lerp(_lerpBlending, Casting ? 1f : 0f, Time.DeltaTime * 2f);
            if (!Casting) return;
            if (Orientate) User.Orientate();
        }
    }
}