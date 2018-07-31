using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Hedra.Engine.ModuleSystem
{
    public class ColorTemplate
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public Vector4 Vector4 => new Vector4(R,G,B,1.0f);
    }
}
