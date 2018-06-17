/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/05/2016
 * Time: 05:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.IO.Compression;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of VertexData.
	/// </summary>
	public class VertexData : IDisposable
    {
		public List<Vector3> Vertices = new List<Vector3>();
		public List<Vector4> Colors = new List<Vector4>();
		public List<Vector3> Normals = new List<Vector3>();
		public List<uint> Indices = new List<uint>();
		public List<float> ExtraData = new List<float>();
		public VertexData Original = null;
		public bool UseCache = false;
		public static VertexData Empty = new VertexData();
		
		public bool IsClone => Original != null;

	    public void Clear(){
			ExtraData.Clear();
			Indices.Clear();
			Normals.Clear();
			Vertices.Clear();
			Indices.Clear();
			ExtraData.Clear();
		}
		
		public void AddVertex( Vector3 Point, Vector4 Color, Vector3 Normal){
			Indices.Add( (uint) Indices.Count);
			Vertices.Add(Point);
			Colors.Add(Color);
			Normals.Add(Normal);
		}

		public Vector3 SupportPoint(Vector3 Direction){
			return SupportPoint(Direction, -Vector4.One);
		}
		
		public Dictionary<Vector3, int> Points = new Dictionary<Vector3, int>();
		public Vector3 SupportPoint(Vector3 Direction, Vector4 Color){
			float highest = float.MinValue;
		    Vector3 support = Vector3.Zero;
		    if(UseCache){
			    if(IsClone){
			    	if(Original.Points.ContainsKey(Direction + Color.Xyz)){
			    		return Original.Vertices[Original.Points[Direction + Color.Xyz]];
			    	}
			    }else{
			    	if(Points.ContainsKey(Direction + Color.Xyz))
			    		return Vertices[Points[Direction + Color.Xyz]];
			    }
		    }
		    int Index = -1;
		    lock(Vertices){
			    for (int i = Vertices.Count-1; i > -1; i--) {
			    	if(Colors[i] != Color && Color != -Vector4.One) continue;
			    		
			        Vector3 v = Vertices[i];
			        float dot = Vector3.Dot(Direction, v);
			
			        if (dot > highest) {
			            highest = dot;
			            support = v;
			            Index = i;
			        }
			    }
		    }

		    if(Index != -1 && UseCache){
			    if(IsClone)
			    	Original.Points.Add(Direction + Color.Xyz, Index);
			 	else
			   		Points.Add(Direction + Color.Xyz, Index);
		    }
		    return support;	
		}
		
		public List<Vector3> SelectVertices(Vector4 Color){
			List<Vector3> ColorList = new List<Vector3>();
			for(int i = 0; i < Vertices.Count; i++){
				if(Color == Colors[i])
					ColorList.Add(Vertices[i]);
			}
			return ColorList;
		}
		
		public float[] GenerateWindValues(){
			return GenerateWindValues(-Vector4.One, Utils.Rng.NextFloat());
		}
		
		public float[] GenerateWindValues( Vector4 Color, float Dir){
			float[] Values = new float[Vertices.Count];
			Vector3 Highest = SupportPoint(Vector3.UnitY, Color);
			Vector3 Lowest =  SupportPoint(-Vector3.UnitY, Color);
			
			for(int i = 0; i < Vertices.Count; i++){
				if(Colors[i] != Color && Color != -Vector4.One){
					Values[i] = 0;
					continue;
				}
				
				float Shade = Vector3.Dot(Vertices[i] - Lowest, Vector3.UnitY) / Vector3.Dot(Highest - Lowest, Vector3.UnitY);
				Values[i] = Shade + (float) Math.Pow(Shade, 1.3);
			}
			return Values;
		}

	    public void FillExtraData(float Value)
	    {
	        ExtraData.Clear();
	        for (int i = 0; i < Vertices.Count; i++)
	        {
	            ExtraData.Add(Value);
	        }
	    }

	    public void AddExtraData(Vector4 Color, float[] Values){
			int k = 0;
			for(int i = 0; i < Vertices.Count; i++){
				if(Colors[i] == Color){
					ExtraData[i] = Values[i];
					k++;
				}
			}
		}
		public void GraduateColor(Vector3 Direction){
			this.GraduateColor(Direction, .3f);
		}
		public void GraduateColor(Vector3 Direction, float Amount){
			Vector3 Highest = SupportPoint(Direction);
			Vector3 Lowest =  SupportPoint(-Direction);

		    float dot = Vector3.Dot(Highest - Lowest, Direction);
            for (int i = 0; i < Vertices.Count; i++){
				float Shade = Vector3.Dot(Vertices[i] - Lowest, Direction) / dot;
				Colors[i] += new Vector4(Amount, Amount, Amount, 0) * Shade;
			}
		}
		
		public void Transform(Vector3 Position){
			for (int i = 0; i < Vertices.Count; i++){
				Vertices[i] += Position;
			}
		}
		
		public void Transform(Matrix4 Mat){
			Matrix4 Mat2 = Mat.ClearScale().ClearTranslation().Inverted();
			for (int i = 0; i < Vertices.Count; i++){
				Vertices[i] = Vector3.TransformPosition(Vertices[i], Mat);
			}
			for (int i = 0; i < Normals.Count; i++){
				Normals[i] = Vector3.TransformNormalInverse(Normals[i], Mat2);
			}
		}
		
		public void Transform(Matrix3 Mat){
			for (int i = 0; i < Vertices.Count; i++){
				Vertices[i] = Vector3.TransformPosition(Vertices[i], new Matrix4(Mat));
			}
		}
		
		public void Scale(Vector3 Scalar){
			for (int i = 0; i < Vertices.Count; i++){
				Vertices[i] *= Scalar;
			}
		}
		
		public void VariateColors(float Amount, Random Rng){
			for(int i = 0; i < Colors.Count; i++){
				float Val = Rng.NextFloat() * Amount * 2 - Amount;
				Colors[i] += new Vector4(Val,Val,Val,1);
			}
		}
		
		public void MultiplyColor( Vector4 Color){
			for(int i = 0; i < Colors.Count; i++){
				Colors[i] *= Color;
			}
		}
		
		public void ColorAll(Vector4 Color){
			for(int i = 0; i < Vertices.Count; i++){ 
				Colors.Add(Color);
			}
		}
		
		public void Mix(Vector4 Color){
			for(int i = 0; i < Colors.Count; i++){
				Colors[i] += Color;
				Colors[i] *= .5f;
			}
		}
		
		public void ReColor(Vector4 Color){
			for(int i = 0; i < Colors.Count; i++){
				Colors[i] = Color;
			}
		}
		
		public VertexData Clone(){
			VertexData Data = new VertexData();
			
			try{
				Data.Indices = new List<uint>(this.Indices);
				Data.Vertices = new List<Vector3>(this.Vertices);
				Data.Colors = new List<Vector4>(this.Colors);
				Data.Normals = new List<Vector3>(this.Normals);
				Data.ExtraData = new List<float>(ExtraData);
				Data.Original = this;
			}catch(Exception e){
				Log.WriteLine(e.ToString());
			}
			return Data;
		}
		
		public void Color(Vector4 Original, Vector4 Replacement){
			for(int i = 0; i < Colors.Count; i++){
				if(Colors[i] == Original){
					Colors[i] = Replacement;
				}
			}
		}
		
		public void ColorWithRandomness(Vector4 Original, Vector4 Replacement, int Range){
			for(int i = 0; i < Colors.Count; i++){
				if(Colors[i] == Original){
					Colors[i] = Utils.VariateColor(Replacement, Range);
				}
			}
		}
		
		public static VertexData operator +(VertexData V1, VertexData V2){
			if(V1 == null || V1.Indices == null)
				return V2.Clone();
			VertexData V3 = V2.Clone();
			for(int i = 0; i < V3.Indices.Count; i++){
				V3.Indices[i] += (uint) V1.Vertices.Count;
			}
			V1.Vertices.AddRange(V3.Vertices);
			V1.Colors.AddRange(V3.Colors);
			V1.Normals.AddRange(V3.Normals);
			V1.Indices.AddRange(V3.Indices);
			V1.ExtraData.AddRange(V3.ExtraData);
			
			V3.Dispose();
			return V1;
		}
		
		public void AddVertex(Vertex V1){
			this.AddVertex(V1.Point, V1.Color, V1.Normal);
		}
		public void AddTriangle( MarchingTriangles.Triangle Tri){
			this.AddVertex(Tri.V1); 
			this.AddVertex(Tri.V2);
			this.AddVertex(Tri.V3);
		}
		
		public DataContainer ToDataContainer(){
			MarchingData Data = new MarchingData(Vector4.One);
			Data.Indices = this.Indices.Clone();
			Data.VerticesArrays = this.Vertices.ToArray();
			Data.Normals = this.Normals.ToArray();
			Data.Color = this.Colors.ToArray();
			return Data;
		}
		
		public int SizeInBytes{
			get{ return Indices.Count * sizeof(uint) 
					+ Vertices.Count * Vector3.SizeInBytes 
					+ Normals.Count * Vector3.SizeInBytes 
					+ Colors.Count * Vector4.SizeInBytes 
					+ ExtraData.Count * sizeof(float); 
			}
		}
		
		public byte[] ToByteArray(){
			using ( MemoryStream Ms = new MemoryStream() ){
				using ( BinaryWriter Bw = new BinaryWriter(Ms) ){
					
					Bw.Write(Vertices.Count);
					Bw.Write(ExtraData.Count);
					Bw.Write(Indices.Count);
					
					for(int i = 0; i < Vertices.Count; i++){
						Bw.Write(Vertices[i]);
						Bw.Write(Normals[i]);
						Bw.Write(Colors[i]);
					}
					
					for(int i = 0; i < ExtraData.Count; i++)
						Bw.Write(ExtraData[i]);
					
					for(int i = 0; i < Indices.Count; i++)
						Bw.Write(Indices[i]);
				}
				return Ms.ToArray();
			}
		}
		
		public void FromByteArray(byte[] Data, bool Compressed = false){
			this.Clear();
			
			MemoryStream DataStream = new MemoryStream(Data);
			Stream Ms = null;
			
			if(Compressed)
		       	Ms = new GZipStream(DataStream, CompressionMode.Decompress);
			else
				Ms = DataStream;
			
			using ( BinaryReader Br = new BinaryReader(Ms) ){

				int VerticesCount = Br.ReadInt32();
				int ExtraDataCount = Br.ReadInt32();
				int IndicesCount = Br.ReadInt32();
				
				for(int i = 0; i < VerticesCount; i++){
					Vertices.Add(new Vector3(Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle()));
					
					Normals.Add(new Vector3(Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle()));
					
					Colors.Add(new Vector4(Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle()));
				}
				
				for(int i = 0; i < ExtraDataCount; i++)
					ExtraData.Add(Br.ReadSingle());
				
				
				for(int i = 0; i < IndicesCount; i++)
					Indices.Add( Br.ReadUInt32() );
			}
			Ms.Dispose();
			DataStream.Dispose();
		}

		public void Dispose(){
			Indices.Clear();
			Colors.Clear();
			Vertices.Clear();
			Normals.Clear();
			ExtraData.Clear();
		}
	}
}
