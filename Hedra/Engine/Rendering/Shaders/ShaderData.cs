using System;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Shaders
{
    public class ShaderData
    {
        public ShaderData()
        {
            SourceFinder = () => AssetManager.ReadShader(Path);
        }

        public string Source { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public Func<string> SourceFinder { get; set; }
    }
}