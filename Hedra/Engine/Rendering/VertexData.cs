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
using OpenTK;

namespace Hedra.Engine.Rendering
{
    /// <inheritdoc />
    /// <summary>
    /// Description of VertexData.
    /// </summary>
    internal sealed class VertexData : IDisposable
    {
        public List<Vector3> Vertices { get; set; }
        public List<Vector4> Colors { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<uint> Indices { get; set; }
        public List<float> ExtraData { get; set; }
        public VertexData Original { get; private set; }
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
            ExtraData = new List<float>();
            _points = new Dictionary<Vector3, int>();
        }

		public Vector3 SupportPoint(Vector3 Direction)
        {
			return this.SupportPoint(Direction, -Vector4.One);
		}
		
		public Vector3 SupportPoint(Vector3 Direction, Vector4 Color)
        {
			float highest = float.MinValue;
		    Vector3 support = Vector3.Zero;
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
		    lock(Vertices)
            {
			    for (int i = Vertices.Count-1; i > -1; i--)
                {
			    	if(Colors[i] != Color && Color != -Vector4.One) continue;
			    		
			        Vector3 v = Vertices[i];
			        float dot = Vector3.Dot(Direction, v);
			
			        if (dot > highest)
                    {
			            highest = dot;
			            support = v;
			            index = i;
			        }
			    }
		    }

            if (index == -1 || !UseCache) return support;

            if(IsClone)
                Original._points.Add(Direction + Color.Xyz, index);
            else
                _points.Add(Direction + Color.Xyz, index);

            return support;	
		}
		
		public float[] GenerateWindValues()
        {
			return this.GenerateWindValues(-Vector4.One, Utils.Rng.NextFloat());
		}
		
		public float[] GenerateWindValues( Vector4 Color, float Dir)
        {
			var values = new float[Vertices.Count];
			Vector3 highest = this.SupportPoint(Vector3.UnitY, Color);
			Vector3 lowest = this.SupportPoint(-Vector3.UnitY, Color);
			
			for(var i = 0; i < Vertices.Count; i++)
            {
				if(Colors[i] != Color && Color != -Vector4.One){
					values[i] = 0;
					continue;
				}
				
				float shade = Vector3.Dot(Vertices[i] - lowest, Vector3.UnitY) / Vector3.Dot(highest - lowest, Vector3.UnitY);
				values[i] = shade + (float) Math.Pow(shade, 1.3);
			}
			return values;
		}

	    public void FillExtraData(float Value)
	    {
	        ExtraData.Clear();
	        for (var i = 0; i < Vertices.Count; i++)
	        {
	            ExtraData.Add(Value);
	        }
	    }

	    public void AddExtraData(Vector4 Color, float[] Values){
			var k = 0;
			for(var i = 0; i < Vertices.Count; i++)
            {
                if (Colors[i] != Color) continue;
                ExtraData[i] = Values[i];
                k++;
            }
		}

		public void GraduateColor(Vector3 Direction)
        {
			this.GraduateColor(Direction, .3f);
		}

		public void GraduateColor(Vector3 Direction, float Amount)
        {
			Vector3 highest = this.SupportPoint(Direction);
			Vector3 lowest =  this.SupportPoint(-Direction);

		    float dot = Vector3.Dot(highest - lowest, Direction);
            for (var i = 0; i < Vertices.Count; i++)
            {
				float shade = Vector3.Dot(Vertices[i] - lowest, Direction) / dot;
				Colors[i] += new Vector4(Amount, Amount, Amount, 0) * shade;
			}
		}
		
		public void Translate(Vector3 Position)
        {
			for (var i = 0; i < Vertices.Count; i++)
            {
				Vertices[i] += Position;
			}
		}
		
		public void Transform(Matrix4 Mat)
        {
			for (var i = 0; i < Vertices.Count; i++)
            {
				Vertices[i] = Vector3.TransformPosition(Vertices[i], Mat);
			}
            Matrix4 normalMat = Mat.ClearScale().ClearTranslation().Inverted();
            for (var i = 0; i < Normals.Count; i++)
            {
				Normals[i] = Vector3.TransformNormalInverse(Normals[i], normalMat);
			}
		}
		
		public void Transform(Matrix3 Mat)
        {
			for (var i = 0; i < Vertices.Count; i++)
            {
				Vertices[i] = Vector3.TransformPosition(Vertices[i], new Matrix4(Mat));
			}
		}
		
		public void Scale(Vector3 Scalar)
        {
			for (var i = 0; i < Vertices.Count; i++)
            {
				Vertices[i] *= Scalar;
			}
		}
		
		public void Paint(Vector4 Color)
        {
			for(var i = 0; i < Colors.Count; i++)
            {
				Colors[i] = Color;
			}
		}
		
		public VertexData Clone()
        {
            return new VertexData
            {
                Indices = new List<uint>(this.Indices),
                Vertices = new List<Vector3>(this.Vertices),
                Colors = new List<Vector4>(this.Colors),
                Normals = new List<Vector3>(this.Normals),
                ExtraData = new List<float>(ExtraData),
                Original = this
            };
		}

        public VertexData ShallowClone()
        {
            return new VertexData
            {
                Indices = new List<uint>(this.Indices),
                Vertices = new List<Vector3>(this.Vertices),
                Colors = new List<Vector4>(this.Colors),
                Normals = new List<Vector3>(this.Normals),
                ExtraData = new List<float>(this.ExtraData),
                Original = this
            };
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
			V1.ExtraData.AddRange(v3.ExtraData);
			
			v3.Dispose();
			return V1;
		}
		
		public int SizeInBytes => Indices.Count * sizeof(uint) 
		                          + Vertices.Count * Vector3.SizeInBytes 
		                          + Normals.Count * Vector3.SizeInBytes 
		                          + Colors.Count * Vector4.SizeInBytes 
		                          + ExtraData.Count * sizeof(float);
        public bool IsClone => Original != null;

        public void Clear()
        {
            ExtraData.Clear();
            Indices.Clear();
            Normals.Clear();
            Vertices.Clear();
            Indices.Clear();
            ExtraData.Clear();
        }

        public void Dispose()
        {
			this.Clear();
		}
	}
}
