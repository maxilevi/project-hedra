using System;
using Hedra.Core;
using Hedra.Engine.Player;
using Hedra.Sound;

namespace Hedra.Engine.SkillSystem
{
    public abstract class PlayerDrainSkill : DrainSkill<IPlayer>
    {
        
    }

    public abstract class DrainSkill<T> : SwitchSkill<T> where T : class, IObjectWithAnimation, ISkillUser, IObjectWithMovement, IObjectWithWeapon, IObjectWithLifeCycle
    {
        protected override string AnimationPath => throw new NotImplementedException();
        protected override SoundType SoundType => throw new NotImplementedException();
        protected override bool HasAnimation => false;
        protected override bool HasSound => false;
        protected override bool Orientate => false;

        public override void Update()
        {
            base.Update();
            if (Casting)
            {
                if(User.Mana > ManaPerSecond)
                    User.Mana -= ManaPerSecond * Time.DeltaTime;
                else
                    KeyUp();
            }
        }
        
        protected abstract float ManaPerSecond { get; }
    }
}