namespace Hedra.Engine.ModuleSystem.Templates
{
    public class SpawnerSettings
    {
        public MiniBossTemplate[] MiniBosses { get; set; }
        public SpawnTemplate[] Shore { get; set; }     
        public SpawnTemplate[] Plains { get; set; }
        public SpawnTemplate[] Mountain { get; set; }
        public SpawnTemplate[] Forest { get; set; }
        public int ExplorerRatio { get; set; }
    }

    public class MiniBossTemplate
    {
        public string Type { get; set; }
        public float Chance { get; set; }
        public bool IsCustom { get; set; }
    }
}
