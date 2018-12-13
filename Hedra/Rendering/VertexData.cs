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
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Rendering
{
    /// <inheritdoc />
    /// <summary>
    /// Description of VertexData.
    /// </summary>
    public sealed class VertexData : LodableObject<VertexData>, IDisposable, IVertexData
    {
        public List<Vector3> Vertices { get; set; }
        public List<Vector4> Colors { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<uint> Indices { get; set; }
        public List<float> Extradata { get; set; }
        public VertexData Original { get; set; }
        public bool UseCache { get; set; }
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
                    if(Colors[i] != Color && !all) continue;
                        
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
        
        public void AddWindValues()
        {
            AddWindValues(-Vector4.One);
        }
        
        public void AddWindValues(float Scalar)
        {
            AddWindValues(-Vector4.One, Scalar);
        }
        
        public void AddWindValues(Vector4 Color, float Scalar = 1f)
        {
            var values = new float[Vertices.Count];
            var highest = this.SupportPoint(Vector3.UnitY, Color);
            var lowest = this.SupportPoint(-Vector3.UnitY, Color);
            var all = Color == -Vector4.One;
            if(Extradata.Count == 0) Extradata = Enumerable.Repeat(0f, Vertices.Count).ToList();
            for(var i = 0; i < Extradata.Count; i++)
            {
                if(Colors[i] != Color && !all)
                {
                    values[i] = 0;
                    continue;
                }             
                var shade = Vector3.Dot(Vertices[i] - lowest, Vector3.UnitY) / Vector3.Dot(highest - lowest, Vector3.UnitY);
                Extradata[i] = (shade + (float) Math.Pow(shade, 1.3)) * Scalar;
            }
            ApplyRecursively(V => V.AddWindValues(Color, Scalar));
        }
        
        public void FillExtraData(float Value)
        {
            Extradata.Clear();
            for (var i = 0; i < Vertices.Count; i++)
            {
                Extradata.Add(Value);
            }
            ApplyRecursively(V => V.FillExtraData(Value));
        }
        
        public void Translate(Vector3 Position)
        {
            Transform(Matrix4.CreateTranslation(Position));
        }

        public void Center()
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
        }

        public void GraduateColor(Vector3 Direction)
        {
            this.GraduateColor(Direction, .3f);
        }

        private void GraduateColor(Vector3 Direction, float Amount)
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
        }
        
        public void Transform(Matrix4 Mat)
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
        }

        public VertexData Optimize()
        {
            if (Colors.Count == 0) return this;

            var decoupledVertices = Indices.Select(I => Vertices[(int)I]).ToList();
            var newIndices = new List<uint>();
            var newVertices = decoupledVertices.Distinct().ToList();
            for (var i = 0; i < decoupledVertices.Count; i++)
            {
                newIndices.Add((uint) newVertices.IndexOf(decoupledVertices[i]));
            }
            var newNormals = new List<Vector3>();
            var newColors = new List<Vector4>();
            var newExtradata = new List<float>();
            for (var i = 0; i < newVertices.Count; i++)
            {
                var index = Vertices.IndexOf(newVertices[i]);
                newColors.Add(Colors[index]);
                newNormals.Add(Normals[index]);
                if(Extradata != null && newExtradata.Count > 0)
                    newExtradata.Add(Extradata[index]);
            }
            Indices = newIndices;
            Vertices = newVertices;
            Colors = newColors;
            Extradata = newExtradata;
            Normals = newNormals;
            return this;
        }
        
        public void Scale(Vector3 Scalar)
        {
            Transform(Matrix4.CreateScale(Scalar));
        }
        
        public void Paint(Vector4 Color)
        {
            for(var i = 0; i < Colors.Count; i++)
            {
                Colors[i] = Color;
            }
            ApplyRecursively(V => V.Paint(Color));
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
                if(Colors[i] == OriginalColor)
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
        
        public void Dispose()
        {
            Extradata.Clear();
            Indices.Clear();
            Normals.Clear();
            Vertices.Clear();
            Indices.Clear();
            Extradata.Clear();
        }
    }
}
