using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ModuleSystem
{
    public class EffectTemplate
    {
        public string Name { get; set; }
        public float Chance { get; set; } = -1;
        public float Damage { get; set; } = -1;
        public float Duration { get; set; } = -1;
    }
}
