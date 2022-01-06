namespace Hedra.Engine.ModuleSystem.Templates
{
    public class SpawnTemplate : ISpawnTemplate
    {
        public int MinGroup { get; set; } = 1;
        public int MaxGroup { get; set; } = 1;
        public string Type { get; set; }
        public float Chance { get; set; }
        public string Distribution { get; set; }
    }

    public interface ISpawnTemplate
    {
        string Type { get; set; }
        float Chance { get; set; }
    }
}