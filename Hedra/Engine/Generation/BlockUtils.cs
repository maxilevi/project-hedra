/*
 * Author: Zaphyk
 * Date: 03/03/2016
 * Time: 10:34 p.m.
 *
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of BlockUtils.
	/// </summary>
	public static class BlockUtils
	{
		public static BlockType[] CubicBlocks = new BlockType[]{ BlockType.Wood, BlockType.Leaves, BlockType.Temporal};
		public static CubeData AmbientOcclusion(CubeData Data, Chunk Chunk, int x, int y, int z){
			return AmbientOcclusion(Data, Chunk, x, y, z, new Vector4(0.075f,0.075f,0.075f, 0));
		}
		public static CubeData AmbientOcclusion(CubeData Data, Chunk Chunk, int x, int y, int z, Vector4 OccludeVector){
			bool side1;
			for(int i = 0;i < Data.Color.Length; i++){
				//SIDES 
				if(i == 0){//bot corner
					side1 = Chunk.IsActiveBlock(x-1,y,z+1);

					if(side1){	
						Data.Color[1] -= OccludeVector;
						Data.Color[2] -= OccludeVector;
					}
				}else
				
				if(i == 1){//bot corner
					side1 = Chunk.IsActiveBlock(x+1,y,z+1);
					
					if(side1){	
						Data.Color[0] -= OccludeVector;
						Data.Color[3] -= OccludeVector;
					}
				}else
				
				if(i == 2){

					side1 = Chunk.IsActiveBlock(x+1,y+1,z);
					
					if(side1){
						Data.Color[4] -= OccludeVector;
						Data.Color[7] -= OccludeVector;
					}
				}else
				
				if(i == 3){//top corner
					side1 = Chunk.IsActiveBlock(x+1,y,z-1);
					
					if(side1){
						Data.Color[6] -= OccludeVector;
						Data.Color[7] -= OccludeVector;
					}
				}
				 if(i == 11){

					side1 = Chunk.IsActiveBlock(x-1,y,z+1);
					
					if(side1){
						Data.Color[12] -= OccludeVector;
						Data.Color[15] -= OccludeVector;
					}
				}
				if(i == 13){

					side1 = Chunk.IsActiveBlock(x-1,y,z-1);
					
					if(side1){
						Data.Color[21] -= OccludeVector;
						Data.Color[22] -= OccludeVector;
					}
				}
				 
				//BLOCK ON TOP
				
				if(i == 4){

					side1 = Chunk.IsActiveBlock(x+1,y+1,z);
					
					if(side1){
						Data.Color[4] -= OccludeVector;
						Data.Color[7] -= OccludeVector;
					}
				}
				
				if(i == 5){

					side1 = Chunk.IsActiveBlock(x,y+1,z+1);
					
					if(side1){
						Data.Color[0] -= OccludeVector;
						Data.Color[1] -= OccludeVector;
					}
				}
				if(i == 10){

					side1 = Chunk.IsActiveBlock(x-1,y+1,z);
					
					if(side1){
						Data.Color[12] -= OccludeVector;
						Data.Color[13] -= OccludeVector;
					}
				}
				if(i == 12){

					side1 = Chunk.IsActiveBlock(x,y+1,z-1);
					
					if(side1){
						Data.Color[22] -= OccludeVector;
						Data.Color[23] -= OccludeVector;
					}
				}
				
				//TOP///
				if(i == 6){

					side1 = Chunk.IsActiveBlock(x,y+1,z+1);
					
					if(side1){
						Data.Color[8] -= OccludeVector;
						Data.Color[11] -= OccludeVector;
					}
				}
				if(i == 7){

					side1 = Chunk.IsActiveBlock(x+1,y+1,z);
					
					if(side1){
						Data.Color[8] -= OccludeVector;
						Data.Color[9] -= OccludeVector;
					}
				}
				
				if(i == 8){

					side1 = Chunk.IsActiveBlock(x,y+1,z-1);
					
					if(side1){
						Data.Color[9] -= OccludeVector;
						Data.Color[10] -= OccludeVector;
					}
				}
				
				if(i == 9){

					side1 = Chunk.IsActiveBlock(x-1,y+1,z);
					
					if(side1){
						Data.Color[11] -= OccludeVector;
						Data.Color[10] -= OccludeVector;
					}
				}
			}
			return Data;
		}
		
		public static CubeData CalculateLighting(CubeData Data, Vector3 Direction){
			for(int i = 0; i < Data.FacesIndex.Count; i++){
				Data = BlockUtils.CalculateLightingPerFace(Data, (Face) Data.FacesIndex[i], Direction);
			}
			return Data;
		}
		
		private static CubeData CalculateLightingPerFace(CubeData Data, Face F, Vector3 Direction){
			Vector4 Factor = new Vector4(0.055f, 0.055f, 0.055f, 0);
			Vector4 DarkFactor = new Vector4(-0.025f, -0.025f, -0.025f, 0);
			Vector3 ToLightDirection = Direction.Normalized();
			Vector3 FaceDirection = BlockUtils.GetFaceDirection(F);
			float Dot = Mathf.Clamp(Mathf.DotProduct(ToLightDirection, FaceDirection), 0, 1);
			for(uint i = 0; i<4; i++){
				if(F == Face.FRONT)
					Data.Color[0  * 4 + i] += DarkFactor * (1-Dot);
				if(F == Face.BACK)
					Data.Color[5 * 4 + i] += DarkFactor * (1-Dot);
				if(F == Face.LEFT)
					Data.Color[3 * 4 + i] += DarkFactor * (1-Dot);
				if(F == Face.RIGHT)
					Data.Color[1 * 4 + i] += DarkFactor * (1-Dot);
				if(F == Face.UP)
					Data.Color[2  * 4 + i] += DarkFactor * (1-Dot);
				if(F == Face.DOWN)
					Data.Color[4 * 4 + i] += DarkFactor * (1-Dot);
			}
			for(uint i = 0; i<4; i++){
				if(F == Face.FRONT)
					Data.Color[0  * 4 + i] += Factor * Dot;
				if(F == Face.BACK)
					Data.Color[5 * 4 + i] += Factor * Dot;
				if(F == Face.LEFT)
					Data.Color[3 * 4 + i] += Factor * Dot;
				if(F == Face.RIGHT)
					Data.Color[1 * 4 + i] += Factor * Dot;
				if(F == Face.UP)
					Data.Color[2  * 4 + i] += Factor * Dot;
				if(F == Face.DOWN)
					Data.Color[4 * 4 + i] += Factor * Dot;
			}
			return Data;
		}
		
		private static Vector3 GetFaceDirection(Face F){
			if(F == Face.RIGHT)
				return Vector3.UnitX;
			if(F == Face.LEFT)
				return -Vector3.UnitX;
			if(F == Face.UP)
				return Vector3.UnitY;
			if(F == Face.DOWN)
				return -Vector3.UnitY;
			if(F == Face.FRONT)
				return Vector3.UnitZ;
			if(F == Face.BACK)
				return -Vector3.UnitZ;
			
			return Vector3.Zero.Normalized();
		}
	}
}
