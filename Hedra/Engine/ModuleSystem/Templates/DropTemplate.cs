namespace Hedra.Engine.ModuleSystem.Templates
{
    public class DropTemplate
    {
        public string Tier { get; set; }
        public string Type { get; set; }
        public int Chance { get; set; }
        public int Max { get; set; } = 1;
        public int Min { get; set; } = 1;
    }
}