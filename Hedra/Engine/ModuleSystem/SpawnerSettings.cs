using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ModuleSystem
{
    public class SpawnerSettings
    {        
        public SpawnTemplate[] Shore { get; set; }     
        public SpawnTemplate[] Plains { get; set; }
        public SpawnTemplate[] Mountain { get; set; }
        public SpawnTemplate[] Forest { get; set; }
    }
}
