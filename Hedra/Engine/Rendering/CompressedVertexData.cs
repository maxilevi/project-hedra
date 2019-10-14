using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.Rendering
{
    public class CompressedVertexData : IDisposable
    {
        /* It's not worth compression vertices and indices since they will be mostly different. */
        private VertexData _original;
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

        public void Transform(Matrix4x4 Transformation)
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], Transformation);
            }
            var normalMat = Transformation.ClearScale().ClearTranslation().Inverted();
            for (var i = 0; i < _compressedNormals.Count; i++)
            {
                _compressedNormals[i] = new CompressedValue<Vector3>()
                {
                    Type = Vector3.TransformNormal(_compressedNormals[i].Type, normalMat),
                    Count = _compressedNormals[i].Count
                };
            }
        }
        
        public VertexData ToVertexData()
        {
            return new VertexData
            {
                Vertices = Vertices,
                Colors = Colors,
                Normals = Normals,
                Extradata = Extradata,
                Indices = Indices,
                Original = _original
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
                Indices = Data.Indices,
                _original = Data
            };
        }
        
        public void Dispose()
        {
            
        }
    }
}