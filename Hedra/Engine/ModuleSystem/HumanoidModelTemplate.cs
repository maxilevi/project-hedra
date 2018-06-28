using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Hedra.Engine.ModuleSystem
{
    internal class HumanoidModelTemplate
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public float Scale { get; set; }
        public ColorTemplate[] Colors { get; set; }
    }
}
