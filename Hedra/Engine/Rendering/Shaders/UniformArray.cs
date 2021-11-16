using System;
using System.Linq;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;

namespace Hedra.Engine.Rendering.Shaders
{
    public class UniformArray
    {
        public UniformArray(Type Type, int ShaderId, string Key, int Size, MappingType MappingType)
        {
            this.Key = Key;
            Mappings = new UniformMapping[Size];
            Log.WriteLine($"Parsing uniform array '{Key}'", LogType.System);
            for (var i = 0; i < Size; i++)
            {
                Log.WriteLine($"Retrieving uniform array element '{Key}[{i}]' from ShaderId '{ShaderId}'",
                    LogType.System);
                var location = Renderer.GetUniformLocation(ShaderId, $"{Key}[{i}]");
                Mappings[i] = new UniformMapping(location, Activator.CreateInstance(Type), MappingType);
            }
        }

        protected UniformMapping[] Mappings { get; set; }
        public string Key { get; }
        public object[] Values => Mappings.Select(M => M.Value).ToArray();

        public void Load<T>(T[] Array, int Length)
        {
            for (var i = 0; i < Length; i++)
            {
                Mappings[i].Value = Array[i];
                Shader.LoadMapping(Mappings[i]);
            }
        }
    }
}