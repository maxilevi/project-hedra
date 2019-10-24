using System;
using System.Numerics;

namespace Hedra.Engine.Rendering.Shaders
{
    public class UniformMapping
    {
        public int Location { get; }
        public object Value { get; set; }
        public bool Loaded { get; set; }
        public MappingType Type { get; private set; }

        public UniformMapping(int Location, object Value, MappingType Type)
        {
            this.Location = Location;
            this.Value = Value;
            this.Type = Type;
        }
    }
}
