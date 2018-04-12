/*
 * Author: Zaphyk
 * Date: 27/03/2016
 * Time: 10:41 p.m.
 *
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of MarchingData.
	/// </summary>
	public static class IsoSurfaceCreator
	{
		public const float WaterQuadSize = 1.0f;
		public const float WaterQuadOffset = 1.5f;
		private static Vector3[] FaceUp = new Vector3[4], FaceDown = new Vector3[4], FaceLeft = new Vector3[4],
			FaceRight = new Vector3[4], FaceFront = new Vector3[4], FaceBack = new Vector3[4];
		
		private static Vector3[] FromFace(Vector3 Input, GridCell Cell, int LOD){
			float BlockSize = Chunk.BlockSize;
			if(Vector3.UnitY == Input){
				FaceUp[3] = new Vector3(BlockSize*LOD,0,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceUp[2] = new Vector3(BlockSize*LOD,0,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceUp[1] = new Vector3(0,0,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceUp[0] = new Vector3(0,0,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				return FaceUp;
			}
			
			if(-Vector3.UnitY == Input){
				FaceDown[3] = new Vector3(BlockSize*LOD,-BlockSize,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceDown[2] = new Vector3(BlockSize*LOD,-BlockSize,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceDown[1] = new Vector3(0,-BlockSize,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceDown[0] = new Vector3(0,-BlockSize,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				return FaceDown;
			}
			
			if(Vector3.UnitX == Input){
				FaceRight[3] = new Vector3(BlockSize*LOD, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceRight[2] = new Vector3(BlockSize*LOD, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceRight[1] = new Vector3(BlockSize*LOD, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceRight[0] = new Vector3(BlockSize*LOD, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				return FaceRight;
			}
			
			if(Vector3.UnitZ == Input){
				FaceFront[3] = new Vector3(BlockSize*LOD, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceFront[2] = new Vector3(BlockSize*LOD, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceFront[1] = new Vector3(0f, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceFront[0] = new Vector3(0f, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				return FaceFront;
			}
			
			if(-Vector3.UnitX == Input){
				FaceLeft[3] = new Vector3(0f, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceLeft[2] = new Vector3(0f, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceLeft[1] = new Vector3(0f, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceLeft[0] = new Vector3(0f, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				return FaceLeft;
			}
			
			if(-Vector3.UnitZ == Input){
				FaceBack[3] = new Vector3(BlockSize*LOD, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceBack[2] = new Vector3(BlockSize*LOD, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceBack[1] = new Vector3(0f, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				FaceBack[0] = new Vector3(0f, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
				return FaceBack;
			}
			
			return null;
		}
		
		public static void CreateSolidQuad(Vector3 Direction, GridCell Cell, bool Flipped, Vector3 Offset, Random Gen, int LOD, Vector4 TemplateColor, VertexData Data){
			List<Vector3> Verts = new List<Vector3>();
			List<Vector4> ColorsList = new List<Vector4>();
			List<Vector3> NormalsList = new List<Vector3>();
			List<uint> IndicesList = new List<uint>();
			
			IndicesList.Add((uint) IndicesList.Count);
			IndicesList.Add((uint) IndicesList.Count);
			IndicesList.Add((uint) IndicesList.Count);
			
			IndicesList.Add((uint) IndicesList.Count);
			IndicesList.Add((uint) IndicesList.Count);
			IndicesList.Add((uint) IndicesList.Count);
			
			Vector3[] FaceVertices = IsoSurfaceCreator.FromFace(Direction, Cell, LOD);
			Vector3 V3 = FaceVertices[3];
			Vector3 V1 = FaceVertices[2];
			Vector3 V2 = FaceVertices[1];
			Vector3 V0 = FaceVertices[0];
			
			if(Flipped){
				Verts.Add(V2);
				Verts.Add(V3);
				Verts.Add(V1);
				
				Verts.Add(V0);
				Verts.Add(V2);
				Verts.Add(V1);
			}else{
				Verts.Add(V2);
				Verts.Add(V3);
				Verts.Add(V0);
				
				Verts.Add(V3);
				Verts.Add(V1);
				Verts.Add(V0);
			}
			
			Vector3 Normal1 = Mathf.CrossProduct(Verts[Verts.Count-5] - Verts[Verts.Count-6], Verts[Verts.Count-4] - Verts[Verts.Count-6]).Normalized();
			NormalsList.Add(Normal1);
			NormalsList.Add(Normal1);
			NormalsList.Add(Normal1);
			
			Vector3 Normal2 = Mathf.CrossProduct(Verts[Verts.Count-2] - Verts[Verts.Count-3], Verts[Verts.Count-1] - Verts[Verts.Count-3]).Normalized();
			NormalsList.Add(Normal2);
			NormalsList.Add(Normal2);
			NormalsList.Add(Normal2);
			
			Vector4 Color1 = OffsetColor(TemplateColor, (float) (Gen.NextDouble() * 2f -1f) * 0.015f);
			Vector4 Color2 = OffsetColor(TemplateColor, 0f);
			ColorsList.Add(Color1);
			ColorsList.Add(Color1);
			ColorsList.Add(Color1);
			ColorsList.Add(Color2);
			ColorsList.Add(Color2);
			ColorsList.Add(Color2);
			
			for(int i = 0; i < IndicesList.Count; i++){
				IndicesList[i] += (uint) Data.Vertices.Count;
			}
			Data.Indices.AddRange(IndicesList.ToArray());
			Data.Normals.AddRange(NormalsList.ToArray());
			Data.Vertices.AddRange(Verts.ToArray());
			Data.Colors.AddRange(ColorsList.ToArray());
		}
		
		
		public static void CreateWaterQuad(float BlockSize, GridCell Cell, bool Flipped, Vector3 Offset, int LOD, Vector4 TemplateColor, VertexData Data){
			
			float[] Densities = new float[]{ (float) Math.Max(Cell.Density[0],0), (float) Math.Max(Cell.Density[1],0),
				(float) Math.Max(Cell.Density[2],0), (float) Math.Max(Cell.Density[3],0)};
			
			if( Cell.P[0].Y <= BiomePool.SeaLevel){
				Densities = new float[]{0,0,0,0}; 
			}
			
			float Size = WaterQuadSize;
			Vector3 V3 = new Vector3(BlockSize * LOD * Size - (Size-1) * (Cell.P[2].X / Chunk.BlockSize),-Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[2].Y + Densities[2])*4, BlockSize * LOD * Size - (Size-1) * (Cell.P[2].Z / Chunk.BlockSize)) + Cell.P[0].Xz.ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);
			Vector3 V1 = new Vector3(BlockSize * LOD * Size - (Size-1) * (Cell.P[1].X / Chunk.BlockSize),-Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[1].Y + Densities[1])*4, - (Size-1) * (Cell.P[1].Z / Chunk.BlockSize)) + Cell.P[0].Xz.ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);
			Vector3 V2 = new Vector3(- (Size-1) * (Cell.P[3].X / Chunk.BlockSize) ,-Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[3].Y + Densities[3])*4, BlockSize * LOD * Size - (Size-1) * (Cell.P[3].Z / Chunk.BlockSize)) + Cell.P[0].Xz.ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);
			Vector3 V0 = new Vector3(- (Size-1) * (Cell.P[0].X / Chunk.BlockSize), -Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[0].Y + Densities[0])*4, - (Size-1) * (Cell.P[0].Z / Chunk.BlockSize)) + Cell.P[0].Xz.ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);
			int VertCount = Data.Vertices.Count;
			
			Data.Indices.AddRange( new uint[]{
                      	(uint) Data.Vertices.Count+0,
                      	(uint) Data.Vertices.Count+1,
                      	(uint) Data.Vertices.Count+2,
                      	(uint) Data.Vertices.Count+3,
                      	(uint) Data.Vertices.Count+4,
                      	(uint) Data.Vertices.Count+5
                      } );
			
			if(Flipped){
				
				Data.Vertices.Add(V2);
				Data.Vertices.Add(V3);
				Data.Vertices.Add(V1);
				
				Data.Vertices.Add(V0);
				Data.Vertices.Add(V2);
				Data.Vertices.Add(V1);

			}else{
				
				Data.Vertices.Add(V2);
				Data.Vertices.Add(V3);
				Data.Vertices.Add(V0);
				
				Data.Vertices.Add(V3);
				Data.Vertices.Add(V1);
				Data.Vertices.Add(V0);
			}
			
			Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+0], Data.Vertices[VertCount+1],
			                            Data.Vertices[VertCount+2], ShouldMove(Data.Vertices[VertCount+0], V0, V1, V2, V3, Cell)  ));
			Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+1], Data.Vertices[VertCount+0],
			                            Data.Vertices[VertCount+2], ShouldMove(Data.Vertices[VertCount+1], V0, V1, V2, V3, Cell)  ));
			Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+2], Data.Vertices[VertCount+1],
			                            Data.Vertices[VertCount+0], ShouldMove(Data.Vertices[VertCount+2], V0, V1, V2, V3, Cell)  ));
			
			Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+3], Data.Vertices[VertCount+4],
			                            Data.Vertices[VertCount+5], ShouldMove(Data.Vertices[VertCount+3], V0, V1, V2, V3, Cell)  ));
			Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+4], Data.Vertices[VertCount+3],
			                            Data.Vertices[VertCount+5], ShouldMove(Data.Vertices[VertCount+4], V0, V1, V2, V3, Cell)  ));
			Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+5], Data.Vertices[VertCount+4],
			                            Data.Vertices[VertCount+3], ShouldMove(Data.Vertices[VertCount+5], V0, V1, V2, V3, Cell)  ));
			
			Data.Colors.Add(TemplateColor);
			Data.Colors.Add(TemplateColor);
			Data.Colors.Add(TemplateColor);
			Data.Colors.Add(TemplateColor);
			Data.Colors.Add(TemplateColor);
			Data.Colors.Add(TemplateColor);
		}
		private static Vector3 CodeNormal(Vector3 Original, Vector3 V1, Vector3 V2, bool ShouldMove){
			Vector3 Coded1 = V1 - Original;
			Vector3 Coded2 = V2 - Original;
			
			return new Vector3(Pack(Coded1.Xz, 4096), Pack(Coded2.Xz, 4096), (ShouldMove) ? 1 : 0);//The third value is if the vertex should move
		}
		
		private static float Pack(Vector2 input, int precision)
		{
		    Vector2 output = input;
		    output.X = (float) Math.Floor(output.X * (precision - 1));
		    output.Y = (float) Math.Floor(output.Y * (precision - 1));
		
		    return (output.X * precision) + output.Y;
		}
		
		private static bool ShouldMove(Vector3 Vertex, Vector3 V0, Vector3 V1, Vector3 V2, Vector3 V3, GridCell Cell){
			return true;
			
			if(Vertex == V3)
				return Cell.Type[1] == BlockType.Water && Cell.Type[3] == BlockType.Water && Cell.Type[2] == BlockType.Water;
			if(Vertex == V1)
				return Cell.Type[5] == BlockType.Water && Cell.Type[1] == BlockType.Water && Cell.Type[7] == BlockType.Water;
			if(Vertex == V2)
				return Cell.Type[3] == BlockType.Water && Cell.Type[4] == BlockType.Water && Cell.Type[0] == BlockType.Water;
			if(Vertex == V0)
				return Cell.Type[4] == BlockType.Water && Cell.Type[5] == BlockType.Water && Cell.Type[6] == BlockType.Water; 
			
			return true;
		}
		
		private static Vector4 CodeColor(Vector4 Color){
			return new Vector4(Color.X, Color.Y, Color.Z, .7f);
		}
		
		private static Vector4 OffsetColor(Vector4 Color, float Offset){
			return new Vector4(Color.X + Offset, Color.Y + Offset, Color.Z + Offset, Color.W);
		}
		
	}
}