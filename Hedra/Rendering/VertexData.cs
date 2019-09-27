/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/05/2016
 * Time: 05:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using OpenTK;
using InstanceData = Hedra.Engine.Rendering.InstanceData;

namespace Hedra.Rendering
{
    /// <inheritdoc />
    /// <summary>
    /// Description of VertexData.
    /// </summary>
    public sealed class VertexData : LodableObject<VertexData>, IDisposable, IModelData
    {
        public string Name { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<Vector4> Colors { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<uint> Indices { get; set; }
        public List<float> Extradata { get; set; }
        public VertexData Original { get; set; }
        public bool UseCache { get; set; }
        public bool HasColors => Colors.Count != 0;
        public bool HasExtradata => Extradata.Count == Vertices.Count;
        public static VertexData Empty { get; }
        private readonly Dictionary<Vector3, int> _points;

        static VertexData()
        {
            Empty = new VertexData();
        }

        public VertexData()
        {
            Vertices = new List<Vector3>();
            Colors = new List<Vector4>();
            Normals = new List<Vector3>();
            Indices = new List<uint>();
            Extradata = new List<float>();
            _points = new Dictionary<Vector3, int>();
        }

        public static void TrimExcess<T>(List<T> List)
        {
            var excess = (List.Count % 3);
            if (excess == 0) return;
            for (var i = 0; i < (3-excess); ++i)
            {
                List.Add(List[List.Count - 1]);
            }
        }
        
        public void Trim()
        {
            TrimExcess(Vertices);
            TrimExcess(Colors);
            TrimExcess(Normals);
            TrimExcess(Indices);
            TrimExcess(Extradata);
        }
        
        public Vector3 SupportPoint(Vector3 Direction)
        {
            return this.SupportPoint(Direction, -Vector4.One);
        }

        private Vector3 SupportPoint(Vector3 Direction, Vector4 Color)
        {
            var highest = float.MinValue;
            var support = Vector3.Zero;
            if(UseCache)
            {
                if(IsClone)
                {
                    if(Original._points.ContainsKey(Direction + Color.Xyz))
                    {
                        return Original.Vertices[Original._points[Direction + Color.Xyz]];
                    }
                }
                else
                {
                    if(_points.ContainsKey(Direction + Color.Xyz))
                        return Vertices[_points[Direction + Color.Xyz]];
                }
            }
            var index = -1;
            var all = Color == -Vector4.One;
            lock(Vertices)
            {
                for (var i = Vertices.Count-1; i > -1; i--)
                {
                    if (HasColors)
                    {
                        if (Colors[i] != Color && !all) continue;
                    }

                    var v = Vertices[i];
                    var dot = Vector3.Dot(Direction, v);

                    if (!(dot > highest)) continue;
                    highest = dot;
                    support = v;
                    index = i;
                }
            }

            if (index == -1 || !UseCache) return support;

            if(IsClone)
                Original._points.Add(Direction + Color.Xyz, index);
            else
                _points.Add(Direction + Color.Xyz, index);

            return support;    
        }

        public VertexData AddWindValues(float Scalar = 1)
        {
            return AddWindValues(-Vector4.One, Scalar);
        }
        
        public VertexData AddWindValues(Vector4 ColorFilter, float Scalar = 1)
        {
            return AddWindValues(
                ColorFilter,
                SupportPoint(-Vector3.UnitY, ColorFilter),
                SupportPoint(Vector3.UnitY, ColorFilter),
                Scalar
            );
        }
        
        public VertexData AddWindValues(Vector3 Lowest, Vector3 Highest, float Scalar)
        {
            return AddWindValues(
                -Vector4.One,
                Lowest,
                Highest,
                Scalar
            );
        }
        
        private VertexData AddWindValues(Vector4 ColorFilter, Vector3 Lowest, Vector3 Highest, float Scalar)
        {
            var values = new float[Vertices.Count];
            var all = ColorFilter == -Vector4.One;
            if(Extradata.Count == 0) Extradata = Enumerable.Repeat(0.01f, Vertices.Count).ToList();
            for(var i = 0; i < Extradata.Count; i++)
            {
                if(Colors[i] != ColorFilter && !all)
                {
                    values[i] = 0;
                    continue;
                }             
                var shade = Vector3.Dot(Vertices[i] - Lowest, Vector3.UnitY) / Vector3.Dot(Highest - Lowest, Vector3.UnitY);
                Extradata[i] = (shade + (float) Math.Pow(shade, 1.3)) * Scalar;
            }
            ApplyRecursively(V => V.AddWindValues(ColorFilter, Scalar));
            return this;
        }
        
        public VertexData FillExtraData(float Value)
        {
            Extradata.Clear();
            for (var i = 0; i < Vertices.Count; i++)
            {
                Extradata.Add(Value);
            }
            ApplyRecursively(V => V.FillExtraData(Value));
            return this;
        }
        
        public VertexData Translate(Vector3 Position)
        {
            Transform(Matrix4.CreateTranslation(Position));
            return this;
        }

        public VertexData AverageCenter()
        {
            var avg = Vertices.Aggregate((V1, V2) => V1 + V2) / Vertices.Count;
            Vertices = Vertices.Select(V => V - avg).ToList();
            return this;
        }

        public VertexData Center()
        {
            var min = new Vector3(
                SupportPoint(-Vector3.UnitX).X,
                SupportPoint(-Vector3.UnitY).Y,
                SupportPoint(-Vector3.UnitZ).Z
            );
            var renderCenter = new Vector3(
                SupportPoint(Vector3.UnitX).X - min.X,
                SupportPoint(Vector3.UnitY).Y - min.Y,
                SupportPoint(Vector3.UnitZ).Z - min.Z
            ) * .5f;
            Vertices = Vertices.Select(V => V - (min + renderCenter)).ToList();
            ApplyRecursively(V => V.Center());
            return this;
        }

        public VertexData GraduateColor(Vector3 Direction)
        {
            return GraduateColor(Direction, .3f);
        }

        private VertexData GraduateColor(Vector3 Direction, float Amount)
        {
            var highest = this.SupportPoint(Direction);
            var lowest =  this.SupportPoint(-Direction);

            var dot = Vector3.Dot(highest - lowest, Direction);
            for (var i = 0; i < Vertices.Count; i++)
            {
                var shade = Vector3.Dot(Vertices[i] - lowest, Direction) / dot;
                Colors[i] += new Vector4(Amount, Amount, Amount, 0) * shade;
            }
            ApplyRecursively(V => V.GraduateColor(Direction, Amount));
            return this;
        }
        
        public VertexData Transform(Matrix4 Mat)
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vector3.TransformPosition(Vertices[i], Mat);
            }
            var normalMat = Mat.ClearScale().ClearTranslation().Inverted();
            for (var i = 0; i < Normals.Count; i++)
            {
                Normals[i] = Vector3.TransformNormalInverse(Normals[i], normalMat);
            }
            ApplyRecursively(V => V.Transform(Mat));
            return this;
        }

        public void Optimize()
        {
            if (!HasColors || !CompatibilityManager.SupportsMeshOptimizer) return;
            /* var originalVertices = Vertices.Count; */
            var vertices = new MeshOptimizerVertex[Vertices.Count];
            for (var i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = new MeshOptimizerVertex
                {
                    Position = Vertices[i],
                    Normal = Normals[i],
                    Color = Colors[i]
                };
                if(HasExtradata) vertices[i].Extradata = Extradata[i];
            }
            var result = MeshOptimizer.Optimize(vertices, Indices.ToArray(), MeshOptimizerVertex.SizeInBytes);
            Indices = new List<uint>(result.Item2);
            Normals = new List<Vector3>(result.Item1.Select(V => V.Normal));
            Colors = new List<Vector4>(result.Item1.Select(V => V.Color));
            if(HasExtradata) Extradata = new List<float>(result.Item1.Select(V => V.Extradata));
            Vertices = new List<Vector3>(result.Item1.Select(V => V.Position));
            /* Log.WriteLine($"Vertex Change % = {(1f - Vertices.Count / (float)originalVertices) * 100}, {Vertices.Count}/{originalVertices}"); */
        }

        public void UniqueVertices()
        {
            if (!HasColors) return;
            
            var newIndices = new List<uint>();
            var vertexMap = new Dictionary<Vector3, int>();
            var newVertices = new List<Vector3>();
            var newNormals = new List<Vector3>();
            var newColors = new List<Vector4>();
            var newExtradata = new List<float>();
            for (var i = 0; i < Indices.Count; i++)
            {
                var curr = (int) Indices[i];
                var vertex = Vertices[curr];
                var index = 0;
                if (vertexMap.ContainsKey(vertex))
                {
                    index = vertexMap[vertex];
                }
                else
                {
                    index = newVertices.Count;
                    newVertices.Add(vertex);
                    vertexMap.Add(vertex, index);
                    
                    newColors.Add(Colors[curr]);
                    newNormals.Add(Normals[curr]);
                    if(HasExtradata)
                        newExtradata.Add(Extradata[curr]);
                }
                newIndices.Add((uint)index);
            }
            Indices = newIndices;
            Vertices = newVertices;
            Colors = newColors;
            Extradata = newExtradata;
            Normals = newNormals;
        }
        
        public void Flat()
        {
            if (!HasColors) return;
            var newVertices = new List<Vector3>();
            var newNormals = new List<Vector3>();
            var newColors = new List<Vector4>();
            var newExtradata = new List<float>();
            var newIndices = new List<uint>();
            for (var i = 0; i < Indices.Count; i+=3)
            {
                var i0 = (int)Indices[i];
                var i1 = (int)Indices[i+1];
                var i2 = (int)Indices[i+2];

                var triangleColor = (Colors[i0] + Colors[i1] + Colors[i2]) * .33f;
                newColors.Add(triangleColor);
                newColors.Add(triangleColor);
                newColors.Add(triangleColor);
                
                newVertices.Add(Vertices[i0]);
                newVertices.Add(Vertices[i1]);
                newVertices.Add(Vertices[i2]);
                
                var normal = Vector3.Cross(Vertices[i1] - Vertices[i0], Vertices[i2] - Vertices[i0]).Normalized();
                newNormals.Add(normal);
                newNormals.Add(normal);
                newNormals.Add(normal);
                
                newIndices.Add((uint)newIndices.Count);
                newIndices.Add((uint)newIndices.Count);
                newIndices.Add((uint)newIndices.Count);

                if (HasExtradata)
                {
                    newExtradata.Add(Extradata[i0]);
                    newExtradata.Add(Extradata[i1]);
                    newExtradata.Add(Extradata[i2]);
                }
            }
            Vertices = newVertices;
            Colors = newColors;
            Extradata = newExtradata;
            Normals = newNormals;
            Indices = newIndices;
        }

        public VertexData[] Ungroup()
        {
            return MeshAnalyzer.GetConnectedComponents(this);
        }

        public VertexData RotateX(float EulerAngle)
        {
            return Transform(Matrix4.CreateRotationX(EulerAngle * Mathf.Radian));
        }
        
        public VertexData RotateY(float EulerAngle)
        {
            return Transform(Matrix4.CreateRotationY(EulerAngle * Mathf.Radian));
        }
        
        public VertexData RotateZ(float EulerAngle)
        {
            return Transform(Matrix4.CreateRotationZ(EulerAngle * Mathf.Radian));
        }
        
        public VertexData Scale(Vector3 Scalar)
        {
            return Transform(Matrix4.CreateScale(Scalar));
        }
        
        public VertexData Paint(Vector4 Color)
        {
            for(var i = 0; i < Colors.Count; i++)
            {
                Colors[i] = Color;
            }
            ApplyRecursively(V => V.Paint(Color));
            return this;
        }

        public CompressedVertexData AsCompressed()
        {
            return CompressedVertexData.FromVertexData(this);
        }

        public InstanceData ToInstanceData(Matrix4 Transformation)
        {
            if (!IsClone)
                throw new ArgumentOutOfRangeException("VertexData needs to be a clone.");
            var data = new InstanceData
            {
                ExtraData = Extradata,
                OriginalMesh = Original,
                Colors = Colors,
                TransMatrix = Transformation
            };
            if (HasLod)
            {
                for (var i = 1; i < 4; ++i)
                {
                    var iterator = (int) Math.Pow(2, i);
                    data.AddLOD(Get(iterator, false)?.ToInstanceData(Transformation), iterator);
                }
            }
            CacheManager.Check(data);
            return data;
        }
        
        public VertexData Clone()
        {
            var data = new VertexData
            {
                Indices = new List<uint>(this.Indices),
                Vertices = new List<Vector3>(this.Vertices),
                Colors = new List<Vector4>(this.Colors),
                Normals = new List<Vector3>(this.Normals),
                Extradata = new List<float>(Extradata),
                Original = Original ?? this
            };
            if (data.Original.HasLod)
            {
                for (var i = 1; i < 4; ++i)
                {
                    var iterator = (int)Math.Pow(2, i);
                    data.AddLOD(Get(iterator, false)?.Clone(), iterator);
                }
            }
            return data;
        }

        public VertexData ShallowClone()
        {
            return Clone();
        }

        public void Color(Vector4 OriginalColor, Vector4 ReplacementColor)
        {
            for(var i = 0; i < Colors.Count; i++)
            {
                if( (Colors[i] - OriginalColor).Length < .01f)
                {
                    Colors[i] = ReplacementColor;
                }
            }
            ApplyRecursively(V => V.Color(OriginalColor, ReplacementColor));
        }
        
        public static VertexData operator +(VertexData V1, VertexData V2)
        {
            if(V1?.Indices == null) return V2.Clone();
            var v3 = V2.Clone();
            for(var i = 0; i < v3.Indices.Count; i++)
            {
                v3.Indices[i] += (uint) V1.Vertices.Count;
            }
            V1.Vertices.AddRange(v3.Vertices);
            V1.Colors.AddRange(v3.Colors);
            V1.Normals.AddRange(v3.Normals);
            V1.Indices.AddRange(v3.Indices);
            V1.Extradata.AddRange(v3.Extradata);
            
            v3.Dispose();
            return V1;
        }

        public bool IsEmpty => Vertices.Count == 0
            && Indices.Count == 0
            && Normals.Count == 0
            && Colors.Count == 0
            && Extradata.Count == 0;

        public int SizeInBytes => Indices.Count * sizeof(uint) 
                                  + Vertices.Count * Vector3.SizeInBytes 
                                  + Normals.Count * Vector3.SizeInBytes 
                                  + Colors.Count * Vector4.SizeInBytes 
                                  + Extradata.Count * sizeof(float);
        public bool IsClone => Original != null;
        uint[] IModelData.Indices => Indices.ToArray();
        
        public void Dispose()
        {
            Extradata.Clear();
            Indices.Clear();
            Normals.Clear();
            Vertices.Clear();
            Indices.Clear();
            Extradata.Clear();
        }

        private struct MeshOptimizerVertex
        {
            public static uint SizeInBytes => sizeof(float) * 11;
            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Color;
            public float Extradata;
        }

        public static VertexData Load(string Path, Vector3 Scale)
        {
            return AssetManager.LoadModel(Path, Scale);
        }
    }
}
