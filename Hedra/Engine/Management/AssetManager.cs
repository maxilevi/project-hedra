/*
 * Author: Zaphyk
 * Date: 09/02/2016
 * Time: 03:24 a.m.
 *
 */

using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Rendering;
using OpenTK;


namespace Hedra.Engine.Management
{
    /// <summary>
    /// Description of AssetManager.
    /// </summary>
    public static class AssetManager
    {
        public static string ShaderResource => Provider.ShaderResource;
        public static string SoundResource => Provider.SoundResource;
        public static string AssetsResource => Provider.AssetsResource;
        
        public static Vector4 ColorCode0 { get; }
        public static Vector4 ColorCode1 { get; }
        public static Vector4 ColorCode2 { get; }
        public static Vector4 ColorCode3 { get; }
        public static Vector4[] ColorCodes { get; }
        public static IAssetProvider Provider { get; set; }

        static AssetManager()
        {
            ColorCode0 = new Vector4(.0f,.0f,.0f,1f);
            ColorCode1 = new Vector4(.2f,.2f,.2f,1f);
            ColorCode2 = new Vector4(.4f,.4f,.4f,1f);
            ColorCode3 = new Vector4(.6f,.6f,.6f,1f);
            ColorCodes = new[] {ColorCode0, ColorCode1, ColorCode2, ColorCode3};
            Provider = new CompressedAssetProvider();
        }

        public static FontFamily BoldFamily => Provider.BoldFamily;
        public static FontFamily NormalFamily => Provider.NormalFamily;
        public static string AppPath => Provider.AppPath;
        public static string AppData => Provider.AppData;
        public static string TemporalFolder => Provider.TemporalFolder;
        public static string ShaderCode => Provider.ShaderCode;

        public static void Load()
        {
            Provider.Load();
        }

        public static void ReloadShaderSources()
        {
            Provider.ReloadShaderSources();
        }

        public static void GrabShaders()
        {
            Provider.GrabShaders();
        }

        public static byte[] ReadPath(string Path)
        {
            return Provider.ReadPath(Path);
        }

        public static byte[] ReadBinary(string Name, string DataFile)
        {
            return Provider.ReadBinary(Name, DataFile);
        }

        public static string ReadShader(string Name)
        {
            return Provider.ReadShader(Name);
        }

        public static Icon LoadIcon(string Path)
        {
            return Provider.LoadIcon(Path);
        }

        public static List<CollisionShape> LoadCollisionShapes(string Filename, int Count, Vector3 Scale)
        {
            return Provider.LoadCollisionShapes(Filename, Count, Scale);
        }

        public static List<CollisionShape> LoadCollisionShapes(string Filename, Vector3 Scale)
        {
            return Provider.LoadCollisionShapes(Filename, Scale);
        }

        public static Box LoadHitbox(string ModelFile)
        {
            return Provider.LoadHitbox(ModelFile);
        }

        public static Box LoadDimensions(string ModelFile)
        {
            return Provider.LoadDimensions(ModelFile);
        }

        public static VertexData PLYLoader(string File, Vector3 Scale)
        {
            return Provider.PLYLoader(File, Scale, Vector3.Zero, Vector3.Zero);
        }

        public static VertexData PLYLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation,
            bool HasColors = true)
        {
            return Provider.PLYLoader(File, Scale, Position, Rotation, HasColors);
        }

        public static VertexData PLYLoader(byte[] Data, Vector3 Scale, Vector3 Position, Vector3 Rotation,
            bool HasColors = true)
        {
            return Provider.PLYLoader(Data, Scale, Position, Rotation, HasColors);
        }
        
        public static ModelData DAELoader(string File)
        {
            return Provider.DAELoader(File);
        }

        public static void Dispose()
        {
            Provider.Dispose();
        }
    }
}
