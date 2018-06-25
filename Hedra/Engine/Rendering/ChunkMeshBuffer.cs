/*
 * Author: Zaphyk
 * Date: 20/02/2016
 * Time: 08:34 p.m.
 *
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Rendering.Effects;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace Hedra.Engine.Rendering
{
	public class ChunkMeshBuffer
	{
	    public VBO<Vector3> Vertices;
		public VBO<Vector4> Colors;
		public VBO<ushort> UshortIndices;
		public VBO<uint> Indices;
		public VBO<Vector3> Normals;
		public VAO<Vector3, Vector4, Vector3> Data;
		public List<DataContainer> Blocks = new List<DataContainer>();
		public bool ShortBuffer = false;
		
		public void Dispose(){
			if(this is ObjectMeshBuffer) return;
			
			Blocks.Clear();
		    Vertices?.Dispose();
		    Colors?.Dispose();
		    UshortIndices?.Dispose();
		    Indices?.Dispose();
		    Data?.Dispose();
		}
		
		public virtual void Clear(){
			Blocks.Clear();			
		}
		
		public virtual void Draw(Vector3 Position, bool Shadows){
			
			Data.Bind();

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
			//GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

			Data.Unbind();
			
		}
		
		public void ClearBlocks(){
			Blocks.Clear();
		}
		
		public virtual void Bind(){
			WorldRenderer.StaticShader.Bind();
			//GL.Uniform3(WorldRenderer.StaticShader.PlayerPositionUniform, GameManager.Player.Position);
		}
		
		public virtual void UnBind(){
		    WorldRenderer.StaticShader.UnBind();
		}
		
		public VertexData ToVertexData(){
			lock(Blocks){
				
				var Data = new VertexData();
				var colorsArray = new List<Vector4>();
				var verticesArray = new List<Vector3>();
				var normalsArray = new List<Vector3>();
				var indicesArray = new List<uint>();
				
				Data.Indices = indicesArray;
				Data.Vertices = verticesArray;
				Data.Normals = normalsArray;
				Data.Colors = colorsArray;
				
				if(Blocks.Count <= 0)
					return Data;
				
				bool regular = !(this.Blocks[0] is MarchingData);
				uint indexB = 0;
				for(var i = 0; i<this.Blocks.Count; i++){				
					
					for(var j=0; j<this.Blocks[i].Indices.Count;j++){
						if(!regular)
							this.Blocks[i].Indices[j] += indexB;
						else
							this.Blocks[i].Indices[j] += (uint) (i * this.Blocks[i].VerticesArrays.Length);
					}
					indexB += (uint) this.Blocks[i].Indices.Count;
					
					colorsArray.AddRange(this.Blocks[i].Color);
					verticesArray.AddRange(this.Blocks[i].VerticesArrays);
					indicesArray.AddRange(this.Blocks[i].Indices.ToArray());
					if(this.Blocks[i].HasNormals) normalsArray.AddRange(this.Blocks[i].Normals);
					
				}
				
				return Data;
			}
		}
	}
}