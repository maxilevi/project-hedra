using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Rendering;
using System.Numerics;
using Silk.NET.GLFW;

namespace Hedra.Engine.Management
{
    public interface IAssetProvider
    {
        string ShaderResource { get; }
        string SoundResource { get; }
        string AssetsResource { get; }
        string AppPath { get; }
        string AppData { get; }
        string TemporalFolder { get; }
        string ShaderCode { get; }
        void Load();
        void ReloadShaderSources();
        void GrabShaders();
        byte[] ReadPath(string Path, bool Text);
        byte[] ReadBinary(string Name, string DataFile);
        string ReadShader(string Name);
        byte[] LoadIcon(string Path, out int Width, out int Height);
        List<CollisionShape> LoadCollisionShapes(string Filename, int Count, Vector3 Scale);
        List<CollisionShape> LoadCollisionShapes(string Filename, Vector3 Scale);
        Box LoadHitbox(string ModelFile);
        Box LoadDimensions(string ModelFile);
        VertexData LoadPLYWithLODs(string Filename, Vector3 Scale);
        VertexData PLYLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true);
        ModelData DAELoader(string File);
        void Dispose();
    }
}