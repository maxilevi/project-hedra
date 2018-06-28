﻿using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Skills
{
    internal abstract class LearningSkill : BaseSkill
    {
        public override bool Passive => true;
        public override abstract uint TexId { get; }

        public override void Update()
        {
            if (base.Level == 0) return;
            if (base.Level > 1) Player.AbilityTree.SetPoints(this.GetType(), 1);

            this.Learn();
        }

        public abstract void Learn();

        public override void Use() { }
    }
}
