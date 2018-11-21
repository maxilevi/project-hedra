using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class CompressedVertexData  : IVertexData, IDisposable
    {
        /* It's not worth compression vertices and indices since they will be mostly different. */
        
        public List<Vector3> Vertices { get; set; }
        public List<uint> Indices { get; set; }

        private List<CompressedValue<Vector4>> _compressedColors = new List<CompressedValue<Vector4>>();
        public List<Vector4> Colors
        {
            get => Decompress(_compressedColors);
            set => Compress(value, _compressedColors);
        }
        
        private List<CompressedValue<Vector3>> _compressedNormals = new List<CompressedValue<Vector3>>();
        public List<Vector3> Normals
        {
            get => Decompress(_compressedNormals);
            set => Compress(value, _compressedNormals);
        }

        private List<CompressedValue<float>> _compressedExtradata = new List<CompressedValue<float>>();
        public List<float> Extradata
        {
            get => Decompress(_compressedExtradata);
            set => Compress(value, _compressedExtradata);
        }

        private static List<T> Decompress<T>(List<CompressedValue<T>> Values) where T : struct
        {
            return Values.Decompress();
        }
        
        private static void Compress<T>(List<T> Uncompressed, List<CompressedValue<T>> Compressed) where T : struct
        {
            Uncompressed.Compress(Compressed);
        }
        
        public VertexData ToVertexData()
        {
            return new VertexData
            {
                Vertices = Vertices,
                Colors = Colors,
                Normals = Normals,
                Extradata = Extradata,
                Indices = Indices
            };
        }
        
        public static CompressedVertexData FromVertexData(VertexData Data)
        {
            return new CompressedVertexData
            {
                Vertices = Data.Vertices,
                Colors = Data.Colors,
                Normals = Data.Normals,
                Extradata = Data.Extradata,
                Indices = Data.Indices
            };
        }
        
        public void Dispose()
        {
            
        }
    }
}