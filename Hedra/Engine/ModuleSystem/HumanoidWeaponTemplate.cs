using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ModuleSystem
{
    public class HumanoidWeaponTemplate
    {
        public string Type { get; set; }
        public string Material { get; set; } = "Random";
        public int Damage { get; set; } = 1;
    }
}
