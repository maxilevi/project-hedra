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
		
		//public BO Indices{
		//	get{ return (ShortBuffer) ? UshortIndices : UintIndices; }
		//}
		
		
		public void Dispose(){
			if(this is EntityMeshBuffer) return;
			
			Blocks.Clear();
			if(Vertices != null)
				Vertices.Dispose();
			if(Colors != null)
				Colors.Dispose();
			if(UshortIndices != null)
				UshortIndices.Dispose();
			if(Indices != null)
				Indices.Dispose();
			if(Data != null)
				Data.Dispose();
		}
		
		public virtual void Clear(){
			Blocks.Clear();
			
		}
		
		public virtual void Draw(Vector3 Position, bool Shadows){
			
			Data.Bind();
			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
			GL.EnableVertexAttribArray(2);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
			GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
			
			GL.DisableVertexAttribArray(0);
			GL.DisableVertexAttribArray(1);
			GL.DisableVertexAttribArray(2);
			Data.UnBind();
			
		}
		
		public void ClearBlocks(){
			Blocks.Clear();
		}
		
		public virtual void Bind(){
			BlockShaders.StaticShader.Bind();
			GL.Uniform3(BlockShaders.StaticShader.PlayerPositionUniform, Scenes.SceneManager.Game.Player.Position);
		}
		
		public virtual void UnBind(){
			BlockShaders.StaticShader.UnBind();
		}
		
		public VertexData ToVertexData(){
			lock(Blocks){
				
				VertexData Data = new VertexData();
				List<Vector4> ColorsArray = new List<Vector4>();
				List<Vector3> VerticesArray = new List<Vector3>();
				List<Vector3> NormalsArray = new List<Vector3>();
				List<uint> IndicesArray = new List<uint>();
				
				Data.Indices = IndicesArray;
				Data.Vertices = VerticesArray;
				Data.Normals = NormalsArray;
				Data.Colors = ColorsArray;
				
				if(Blocks.Count <= 0)
					return Data;
				
				bool Regular = !(this.Blocks[0] is MarchingData);
				uint IndexB = 0;
				for(int i = 0; i<this.Blocks.Count; i++){				
					
					for(int j=0; j<this.Blocks[i].Indices.Count;j++){
						if(!Regular)
							this.Blocks[i].Indices[j] += IndexB;
						else
							this.Blocks[i].Indices[j] += (uint) (i * this.Blocks[i].VerticesArrays.Length);
					}
					IndexB += (uint) this.Blocks[i].Indices.Count;
					
					ColorsArray.AddRange(this.Blocks[i].Color);
					VerticesArray.AddRange(this.Blocks[i].VerticesArrays);
					IndicesArray.AddRange(this.Blocks[i].Indices.ToArray());
					if(this.Blocks[i].HasNormals)
						NormalsArray.AddRange(this.Blocks[i].Normals);
					
				}
				
				return Data;
			}
		}
	}
}