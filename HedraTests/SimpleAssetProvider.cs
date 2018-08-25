﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace HedraTests
{
    public class SimpleAssetProvider : IAssetProvider
    {
        public string ShaderResource { get; }
        public string SoundResource { get; }
        public string AssetsResource { get; }
        public FontFamily BoldFamily => new FontFamily(GenericFontFamilies.SansSerif);
        public FontFamily NormalFamily => new FontFamily(GenericFontFamilies.SansSerif);
        public string AppPath { get; }
        public string AppData { get; }
        public string TemporalFolder { get; }
        public string ShaderCode { get; }
        
        public void Load()
        {
        }

        public void ReloadShaderSources()
        {
        }

        public void GrabShaders()
        {
        }

        public byte[] ReadPath(string Path)
        {
            return SampleImage;
        }

        public byte[] ReadBinary(string Name, string DataFile)
        {
            return SampleImage;
        }

        public string ReadShader(string Name)
        {
            return string.Empty;
        }

        public Icon LoadIcon(string path)
        {
            return null;
        }

        public List<CollisionShape> LoadCollisionShapes(string Filename, int Count, Vector3 Scale)
        {
            return new List<CollisionShape>();
        }

        public List<CollisionShape> LoadCollisionShapes(string Filename, Vector3 Scale)
        {
            return new List<CollisionShape>();
        }

        public Box LoadHitbox(string ModelFile)
        {
            return new Box();
        }

        public Box LoadDimensions(string ModelFile)
        {
            return new Box();
        }

        public VertexData PLYLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true)
        {
            return new VertexData();
        }

        public VertexData PLYLoader(byte[] Data, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true)
        {
            return new VertexData();
        }

        public void Dispose()
        {
        }

        private static readonly byte[] SampleImage =
        {
            137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68,
            82, 0, 0, 0, 1, 0, 0, 0, 1, 8, 6, 0, 0, 0, 31, 21, 196, 137,
            0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 4, 103, 
            65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 9, 112,
            72, 89, 115, 0, 0, 14, 195, 0, 0, 14, 195, 1,
            199, 111, 168, 100, 0, 0, 0, 13, 73, 68, 65, 84, 24, 87,
            99, 96, 96, 96, 96, 0, 0, 0, 5, 0, 1, 138, 51, 227, 0, 0, 0,
            0, 0, 73, 69, 78, 68, 174, 66, 96, 130
        };
    }
}