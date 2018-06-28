using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Shaders
{
    internal class ShaderData
    {
        public string Source { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public Func<string> SourceFinder { get; set; }

        public ShaderData()
        {
            SourceFinder = () => AssetManager.ReadShader(Path);
        }
    }
}
