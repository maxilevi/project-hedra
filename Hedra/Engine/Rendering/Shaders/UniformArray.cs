using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Shaders
{
    internal class UniformArray
    {
        protected UniformMapping[] Mappings { get; set; }
        public string Key { get; set; }
        public object[] Values => Mappings.Select(M => M.Value).ToArray();

        public UniformArray(Type Type, int ShaderId, string Key, int Size)
        {
            this.Key = Key;
            this.Mappings = new UniformMapping[Size];
            for (var i = 0; i < Size; i++)
            {
                var location = GL.GetUniformLocation(ShaderId, Key + "[" + i + "]");
                this.Mappings[i] = new UniformMapping(location, Activator.CreateInstance(Type));
            }
        }

        public void Load(object[] Array)
        {
            for (var i = 0; i < Array.Length; i++)
            {
                Mappings[i].Value = Array[i];
                Shader.LoadMapping(Mappings[i]);
            }
        }
    }
}
