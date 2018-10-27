using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Hedra.Engine.ModuleSystem
{
    public class HumanoidBehaviourTemplate
    {
        public static Vector4 Hostile = new Vector4(0.878f, 0.196f, 0.235f, 1);
        public static Vector4 Friendly = new Vector4(0.4f, 0.6627451f, 0.4f, 1);
        public static Vector4 Neutral = Vector4.One;

        public Vector4 Color { get; set; }
        public string Name { get; set; }

        public HumanoidBehaviourTemplate(Vector4 Color)
        {
            this.Color = Color;
        }
    }
}
