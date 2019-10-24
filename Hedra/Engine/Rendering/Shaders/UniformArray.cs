using System;
using System.Linq;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Core;

namespace Hedra.Engine.Rendering.Shaders
{
    public class UniformArray
    {
        protected UniformMapping[] Mappings { get; set; }
        public string Key { get;  }
        public object[] Values => Mappings.Select(M => M.Value).ToArray();

        public UniformArray(Type Type, int ShaderId, string Key, int Size, MappingType MappingType)
        {
            this.Key = Key;
            this.Mappings = new UniformMapping[Size];
            Log.WriteLine($"Parsing uniform array '{Key}'", LogType.System);
            for (var i = 0; i < Size; i++)
            {
                Log.WriteLine($"Retrieving uniform array element '{Key}[{i}]' from ShaderId '{ShaderId}'", LogType.System);
                var location = Renderer.GetUniformLocation(ShaderId, $"{Key}[{i}]");
                this.Mappings[i] = new UniformMapping(location, Activator.CreateInstance(Type), MappingType);
            }
        }

        public void Load(object[] Array, int Length)
        {
            for (var i = 0; i < Length; i++)
            {
                Mappings[i].Value = Array[i];
                Shader.LoadMapping(Mappings[i]);
            }
        }
    }
}
