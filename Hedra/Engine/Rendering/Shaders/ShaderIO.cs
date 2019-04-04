using System;

namespace Hedra.Engine.Rendering.Shaders
{
    public abstract class ShaderIO
    {
        public string Name { get; }
        public Type Type { get; }
        public uint Location { get; }

        protected ShaderIO(uint Location, string Name, Type Type)
        {
            this.Location = Location;
            this.Name = Name;
            this.Type = Type;
        }
    }
}