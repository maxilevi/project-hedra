using System;

namespace Hedra.Engine.Player.Skills
{
    internal abstract class PassiveSkill : BaseSkill
    {
        public override bool Passive => true;
        protected abstract int MaxLevel { get; }

        public override void Update()
        {
            if(Level > MaxLevel)
                Player.AbilityTree.SetPoints(GetType(), MaxLevel);
            
            Change();
        }

        protected abstract void Change();
        
        public override void Use()
        {
            throw new ArgumentException($"Passive skills cannot be used.");
        }
    }
}