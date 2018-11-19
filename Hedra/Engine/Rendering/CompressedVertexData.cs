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
            set => Compress(_compressedColors, value);
        }
        
        private List<CompressedValue<Vector3>> _compressedNormals = new List<CompressedValue<Vector3>>();
        public List<Vector3> Normals
        {
            get => Decompress(_compressedNormals);
            set => Compress(_compressedNormals, value);
        }

        private List<CompressedValue<float>> _compressedExtradata = new List<CompressedValue<float>>();
        public List<float> Extradata
        {
            get => Decompress(_compressedExtradata);
            set => Compress(_compressedExtradata, value);
        }

        private static void Compress<T>(List<CompressedValue<T>> Compressed, List<T> Uncompressed) where T : struct
        {
            var count = 0;
            var type = default(T);
            var compressing = false;
            Compressed.Clear();
            for (var i = 0; i < Uncompressed.Count; i++)
            {
                if (!compressing)
                {
                    type = Uncompressed[i];
                    count = 1;
                    compressing = true;
                    continue;
                }

                if (type.Equals(Uncompressed[i]))
                {
                    count++;
                }
                else
                {
                    Compressed.Add(new CompressedValue<T>
                    {
                        Type = type,
                        Count = (ushort)count
                    });
                    type = Uncompressed[i];
                    count = 1;
                }
            }
            Compressed.Add(new CompressedValue<T>
            {
                Type = type,
                Count = (ushort)count
            });
        }

        private static List<T> Decompress<T>(List<CompressedValue<T>> Values) where T : struct
        {
            var list = new List<T>();
            for (var i = 0; i < Values.Count; i++)
            {
                list.AddRange(Enumerable.Repeat(Values[i].Type, Values[i].Count));   
            }
            return list;
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

    struct CompressedValue<T> where T : struct
    {
        public ushort Count { get; set; }
        public T Type { get; set; }
    }
}