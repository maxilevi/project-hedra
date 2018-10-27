using System;

namespace Hedra.Engine.ModuleSystem
{
    public class SpawnTemplate
    {
        public string Type { get; set; }
        public float Chance { get; set; }
        public int MinGroup { get; set; } = 1;
        public int MaxGroup { get; set; } = 1;
    }
}
