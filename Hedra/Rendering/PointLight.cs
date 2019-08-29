using System.Runtime.InteropServices;
using OpenTK;

namespace Hedra.Rendering
{
    public class PointLight
    {
        public const float DefaultRadius = 20f;
        public Vector3 Position { get; set; }
        public Vector3 Color { get; set; }
        public float Radius { get; set; } = DefaultRadius;
        public bool Locked { get; set; }

        public bool Collides(Vector3 Point)
        {
            return (Position - Point).LengthFast < Radius;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AlignedPointLight
    {
        [FieldOffset(0)] 
        public Vector3 _position;
        [FieldOffset(16)]
        public Vector3 _color;
        [FieldOffset(28)]
        public float _radius;
        
        public AlignedPointLight(PointLight Light)
        {
            _position = Light.Position;
            _color = Light.Color;
            _radius = Light.Radius;
        }
    }
}
