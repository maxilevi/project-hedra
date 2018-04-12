/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/09/2016
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.Collections.Generic;
using System.Collections;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.Generation
{
	
	public class ClosestChunk: IComparer<Chunk> {
		public Vector3 PlayerPos;
		public ClosestChunk(){ 
		}
		public ClosestChunk(Vector3 pos){
			PlayerPos = pos;
		}
	
		public int Compare(Chunk V1, Chunk V2){
			try{
				if(V1 == V2) return 0;
				
				if(V1 == null)
					return -1;
				
				if(V2 == null)
					return 1;
				
				float V1f = (V1.Position - PlayerPos).LengthSquared;
				float V2f = (V2.Position - PlayerPos).LengthSquared;
		
				if(V1f < V2f){
					return -1;
				}else if(V1f == V2f){
					return 0;
				}else{
					return 1;
				}
			}catch(ArgumentException e){
				Log.WriteLine("Unable to sort the chunks properly. " +  e);
				return 0;
			}
		}
	}
}
