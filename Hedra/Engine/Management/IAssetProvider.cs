using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Management
{
    public interface IAssetProvider
    {
        string ShaderResource { get; }
        string SoundResource { get; }
        string AssetsResource { get; }
        FontFamily BoldFamily { get; }
        string AppPath { get; }
        string AppData { get; }
        string TemporalFolder { get; }
        string ShaderCode { get; }
        void Load();
        void ReloadShaderSources();
        void GrabShaders();
        byte[] ReadPath(string Path);
        byte[] ReadBinary(string Name, string DataFile);
        string ReadShader(string Name);
        Icon LoadIcon(string path);
        List<CollisionShape> LoadCollisionShapes(string Filename, int Count, Vector3 Scale);
        List<CollisionShape> LoadCollisionShapes(string Filename, Vector3 Scale);
        Box LoadHitbox(string ModelFile);
        Box LoadDimensions(string ModelFile);
        VertexData PLYLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true);
        VertexData PLYLoader(byte[] Data, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true);
        void Dispose();
    }
}