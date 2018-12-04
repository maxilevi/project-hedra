using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using Hedra.Sound;

namespace Hedra.Engine.Player.Skills
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

        protected SwitchSkill()
        {
            _stance = AnimationLoader.LoadAnimation(AnimationPath);
            _stance.Loop = false;
            _stance.OnAnimationEnd += delegate
            {
                if (!Casting) return;
                Player.Model.PlayAnimation(_stance);
                Player.Model.Blend(_stance);
            };
        }

        public override void Use()
        {
            Casting = true;
            SoundPlayer.PlaySoundWhile(SoundType, () => Casting, () => 1, () => 1);
            Player.Model.PlayAnimation(_stance);
            Player.Model.Blend(_stance);
            Activate();
        }

        public override void KeyUp()
        {
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

    }
}