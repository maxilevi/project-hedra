using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ModuleSystem
{
    public class ModelTemplate
    {
        public string Path { get; set; }
        public float Scale { get; set; }
        public AnimationTemplate IdleAnimation { get; set; }
        public AnimationTemplate WalkAnimation { get; set; }
        public AnimationTemplate AttackAnimation { get; set; }
    }
}
