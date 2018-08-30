using System;

namespace Hedra.Engine.Player.Skills
{
    public abstract class PassiveSkill : BaseSkill
    {
        public override bool Passive => true;
        protected abstract int MaxLevel { get; }

        public override void Update()
        {
            if(Level > MaxLevel)
                Player.AbilityTree.SetPoints(GetType(), MaxLevel);
            
            OnChange();
        }

        protected abstract void OnChange();
        
        public override void Use()
        {
            throw new ArgumentException($"Passive skills cannot be used.");
        }
    }
}