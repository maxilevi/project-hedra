/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/05/2016
 * Time: 01:47 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Generation;
using System.Collections.Generic;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of MarchingTriangles.
	/// </summary>
	internal static class MarchingTriangles
	{
		public static VertexData Triangulate(Chunk Chunk){
			VertexData Data = new VertexData();
			int Size = 6;
			
			Vector3 Middle = new Vector3(8,10,8);
			Vertex V0 = new Vertex(Middle, new Vector4(1,0,0,1), Vector3.UnitY);
			Triangle T1 = new Triangle(V0, new Vector3(Middle.X - Size*0.25f, Middle.Y, Middle.Z + Size*0.5f), new Vector3(Middle.X + Size*0.25f, Middle.Y, Middle.Z + Size*0.5f));
			Data.AddTriangle(T1);
			
			for(int x = 0; x < Chunk.Width; x++){
				for(int z = 0; z < Chunk.Width; z++){
					T1.Transform(new Vector3(Size,0,Size));
					Data.AddTriangle(T1);
				}
			}
			return Data;
		}
		
		internal struct Hexagon{
			public Triangle[] Triangles;
			
			public Hexagon(Vector3 Middle, Vector4 Color, int Size){
				Triangles = new Triangle[6];
			}
		}
		
		internal struct Triangle{
			public Vertex V1, V2, V3;
			
			public Triangle(Vertex V1, Vertex V2, Vertex V3){
				this.V1 = V1;
				this.V2 = V2;
				this.V3 = V3;
			}
			
			public Triangle(Vertex V1, Vector3 V2, Vector3 V3){
				this.V1 = V1;
				this.V2 = new Vertex(V2, V1.Color, V1.Normal);
				this.V3 = new Vertex(V3, V1.Color, V1.Normal);	
			}
			
			public void Transform(Vector3 Position){
				V1.Point += Position;
				V2.Point += Position;
				V3.Point += Position;
			}
			
			public void Flip(){
				Vertex Tmp = V1;
				V1 = V3;
				V3 = Tmp;
			}
		}
	}
	
	internal struct Vertex{
		public Vector3 Point;
		public Vector4 Color;
		public Vector3 Normal;
		
		public Vertex(Vector3 Point, Vector4 Color, Vector3 Normal){
			this.Point = Point;
			this.Color = Color;
			this.Normal = Normal;
		}
	}
}
