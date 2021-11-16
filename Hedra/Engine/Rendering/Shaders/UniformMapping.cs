namespace Hedra.Engine.Rendering.Shaders
{
    public class UniformMapping
    {
        public UniformMapping(int Location, object Value, MappingType Type)
        {
            this.Location = Location;
            this.Value = Value;
            this.Type = Type;
        }

        public int Location { get; }
        public object Value { get; set; }
        public bool Loaded { get; set; }
        public MappingType Type { get; }
    }
}