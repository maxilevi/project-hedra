using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player.Skills
{
    public abstract class SwitchSkill : CappedSkill
    {
        private readonly Animation _stance;
        protected abstract string AnimationPath { get; }
        protected virtual  bool Orientate => true;

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
            if (!Casting) return;
            if (Orientate) Player.Movement.Orientate();
        }

    }
}