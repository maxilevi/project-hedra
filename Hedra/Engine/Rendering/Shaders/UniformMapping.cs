using System;
using OpenTK;

namespace Hedra.Engine.Rendering.Shaders
{
    internal class UniformMapping
    {
        public int Location { get; }
        public object Value { get; set; }
        public MappingType Type { get; private set; }

        public UniformMapping(int Location, object Value)
        {
            this.Location = Location;
            this.Value = Value;
            this.DetectType();
        }

        private void DetectType()
        {
            Type= Value is Matrix4x3 ? MappingType.Matrix4X3
                : Value is Matrix4x2 ? MappingType.Matrix4X2
                : Value is Matrix4 ? MappingType.Matrix4
                : Value is Matrix3x4 ? MappingType.Matrix3X4
                : Value is Matrix3x2 ? MappingType.Matrix3X2
                : Value is Matrix3 ? MappingType.Matrix3
                : Value is Matrix2x4 ? MappingType.Matrix2X4
                : Value is Matrix2x3 ? MappingType.Matrix2X3
                : Value is Matrix2 ? MappingType.Matrix2
                : Value is Vector4 ? MappingType.Vector4
                : Value is Vector3 ? MappingType.Vector3
                : Value is Vector2 ? MappingType.Vector2
                : Value is float ? MappingType.Float
                : Value is double ? MappingType.Double
                : Value is int ? MappingType.Integer
                : MappingType.Unknown;
        }
    }
}
