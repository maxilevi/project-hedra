using System.Reflection;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public class PointLight
    {
        public const float DefaultRadius = 20f;
        public Vector3 Position { get; set; }
        public Vector3 Color { get; set; }
        public float Radius { get; set; } = DefaultRadius;
        public bool Locked { get; set; }
    }
}
