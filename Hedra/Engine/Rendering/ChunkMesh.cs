/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 08:12 p.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Rendering
{
	public class ChunkMesh : ICullable, IDisposable
	{
		public List<ChunkMeshBuffer> MeshBuffers = new List<ChunkMeshBuffer>();
		public List<InstanceBatch> InstanceBatches = new List<InstanceBatch>();
		public List<InstanceData> InstanceElements = new List<InstanceData>();
		public List<ICollidable> CollisionBoxes = new List<ICollidable>();
		public List<VertexData> Elements = new List<VertexData>();
		public VertexData ModelData {get; set;}
		
		public bool IsBuilded;
		public bool IsGenerated;
		public bool Enabled { get; set; }
		public bool BuildedOnce { get; set; }
		
        public Vector3 Position { get; set; }
		public Box CullingBox { get; set; }
		

		public ChunkMesh(Vector3 Position, ChunkMeshBuffer[] BuffersAttachments)
        {
			this.Position = Position;
			this.CullingBox = new Box(Vector3.Zero, new Vector3(Chunk.Width, 768, Chunk.Width));			
			this.MeshBuffers.AddRange(BuffersAttachments);
		}

		#region BUILD METHODS
		public void Build(){
			foreach(ChunkMeshBuffer Buffer in MeshBuffers){
				if(Buffer.Blocks.Count > 0 && Buffer.Blocks[0] is MarchingData)
					BuildBuffer(Buffer, false);
				else
					BuildBuffer(Buffer, true);
			}
		}
		
		public void BuildFrom(ChunkMeshBuffer MeshBuffer, VertexData Data, bool ExtraData){
			try{
			if(Data?.Colors == null)
				return;

			Vector4[] ColorBuffer;
			if(ExtraData){
				ColorBuffer = new Vector4[Data.Colors.Count];
				for(int i = 0; i < ColorBuffer.Length; i++){
					ColorBuffer[i] = new Vector4(Data.Colors[i].Xyz, Data.ExtraData[i]);
				}
			}else{
				ColorBuffer = Data.Colors.ToArray();
			}
			
			int ColorBufferSize = (ColorBuffer.Length * Vector4.SizeInBytes);
			int VertexBufferSize = (Data.Vertices.Count * Vector3.SizeInBytes);
			int IndexBufferSize = (Data.Indices.Count * sizeof(int) );
			int NormalBufferSize = (Data.Normals.Count * Vector3.SizeInBytes);
			
			if(MeshBuffer.Vertices == null)
				MeshBuffer.Vertices = new VBO<Vector3>(Data.Vertices.ToArray(), VertexBufferSize, VertexAttribPointerType.Float);
			else
				MeshBuffer.Vertices.Update(Data.Vertices.ToArray(), VertexBufferSize);

			if(MeshBuffer.Indices == null)
				MeshBuffer.Indices = new VBO<uint>(Data.Indices.ToArray(), IndexBufferSize, VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
			else
				MeshBuffer.Indices.Update(Data.Indices.ToArray(), IndexBufferSize);
			
			
			if(MeshBuffer.Colors == null)
				MeshBuffer.Colors = new VBO<Vector4>(ColorBuffer, ColorBufferSize, VertexAttribPointerType.Float);
			else
				MeshBuffer.Colors.Update(ColorBuffer, ColorBufferSize);
			
			if(MeshBuffer.Normals == null)
				MeshBuffer.Normals = new VBO<Vector3>(Data.Normals.ToArray(), NormalBufferSize, VertexAttribPointerType.Float);
			else
				MeshBuffer.Normals.Update(Data.Normals.ToArray(), NormalBufferSize);
			
			if(MeshBuffer.Data == null)
				MeshBuffer.Data = new VAO<Vector3, Vector4, Vector3>(MeshBuffer.Vertices, MeshBuffer.Colors, MeshBuffer.Normals);
			
			if(MeshBuffer is StaticMeshBuffer)
				(MeshBuffer as StaticMeshBuffer).DrawType = DrawElementsType.UnsignedInt;
			MeshBuffer.ClearBlocks();
			MeshBuffer.Clear();
			//Data.Dispose();
			//Data = null;
			IsBuilded = true;
			Enabled = true;
			BuildedOnce = true;
			}
            catch (Exception e)
            {
				Log.WriteLine(e.ToString());
			}
		}
		
		public void BuildBuffer(ChunkMeshBuffer MeshBuffer, bool HasIndices){
			
			IsBuilded = false;
			try{
				List<Vector4> Colors = new List<Vector4>();
				List<uint> Indices = new List<uint>();
				List<Vector3> Vertices = new List<Vector3>();
				List<Vector3> Normals = new List<Vector3>();
 
				uint IndexB = 0;
				for(int i = 0; i<MeshBuffer.Blocks.Count; i++){
					if(HasIndices){
						for(int j=0; j<MeshBuffer.Blocks[i].Indices.Count;j++){
							MeshBuffer.Blocks[i].Indices[j] += IndexB;
						}
						IndexB += (uint) MeshBuffer.Blocks[i].Indices.Count;
					}
					Colors.AddRange(MeshBuffer.Blocks[i].Color);
					Vertices.AddRange(MeshBuffer.Blocks[i].VerticesArrays);
					Indices.AddRange(MeshBuffer.Blocks[i].Indices.ToArray());
					Normals.AddRange(MeshBuffer.Blocks[i].Normals);
					
				}
				
				int ColorBufferSize = (Colors.Count * Vector4.SizeInBytes);		
				int VertexBufferSize = (Vertices.Count * Vector3.SizeInBytes);
				int IndexBufferSize = (Indices.Count * sizeof(uint));
				int NormalBufferSize = (Normals.Count * Vector3.SizeInBytes);
				
				if(MeshBuffer.Vertices == null)
					MeshBuffer.Vertices = new VBO<Vector3>(Vertices.ToArray(), VertexBufferSize, VertexAttribPointerType.Float);
				else
					MeshBuffer.Vertices.Update(Vertices.ToArray(), VertexBufferSize);
				
				if(HasIndices){
					throw new NotSupportedException("Chunk Mesh, Water cant have indices");
				}
				
				if(MeshBuffer.Colors == null)
					MeshBuffer.Colors = new VBO<Vector4>(Colors.ToArray(), ColorBufferSize, VertexAttribPointerType.Float);
				else
					MeshBuffer.Colors.Update(Colors.ToArray(), ColorBufferSize);
				
				if(MeshBuffer.Normals == null)
					MeshBuffer.Normals = new VBO<Vector3>(Normals.ToArray(), NormalBufferSize, VertexAttribPointerType.Float);
				else
					MeshBuffer.Normals.Update(Normals.ToArray(), NormalBufferSize);
				
				if(MeshBuffer.Data == null)
					MeshBuffer.Data = new VAO<Vector3, Vector4, Vector3>(MeshBuffer.Vertices, MeshBuffer.Colors, MeshBuffer.Normals);
				
				MeshBuffer.ClearBlocks();
				IsBuilded = true;
				Enabled = true;
				BuildedOnce = true;
			}
            catch (Exception e)
            {
			    Log.WriteLine(e.ToString());
			}
		}
		#endregion
		
		#region DRAW METHODS
		public void Draw(ChunkBufferTypes Type){
			Draw(Type, this.Position, false);
		}
		
		public void Draw(ChunkBufferTypes Type, Vector3 Position, bool Shadows){
			if(MeshBuffers == null) return;

			if(GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			DrawMesh(MeshBuffers[(int) Type], Position, Shadows);
			if(GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

		}
		
		public void DrawMesh(ChunkMeshBuffer MeshBuffer, Vector3 Position, bool Shadows){
			if(IsBuilded && IsGenerated && Enabled && MeshBuffer.Data != null){
				MeshBuffer.Draw(Position, Shadows);
			}
		}
		#endregion
		
		public void AddInstance(InstanceData Data){
			InstanceElements.Add(Data);
		}
		
		public new void Dispose(){
			//Already disposed
			if(MeshBuffers == null) return;
			if(MeshBuffers.Count > 0 && MeshBuffers[0] is ObjectMeshBuffer)
				return;
			
			foreach(ChunkMeshBuffer Buffer in MeshBuffers){
				Buffer.Dispose();
			}
			for(int i = 0; i < InstanceBatches.Count; i++){
				InstanceBatches[i].Dispose();
			}

		    ModelData?.Dispose();
		    InstanceElements.Clear();
			Elements.Clear();
            lock(CollisionBoxes)
			    CollisionBoxes.Clear();
			InstanceBatches.Clear();
			MeshBuffers = null;
		}
		
		public void Clear(){
			foreach(ChunkMeshBuffer Buffer in MeshBuffers){
				Buffer.Clear();
			}
		}
		
		public void AddBlock(DataContainer Data, ChunkMeshBuffer Buffer){
			Buffer.Blocks.Add(Data);
		}
		
		public void AddBlock(DataContainer Data, ChunkBufferTypes Type){
			MeshBuffers[ (int) Type].Blocks.Add(Data);
		}
		public void AddBlocks(DataContainer[] Data, ChunkBufferTypes Type){
			for(int i = 0; i < Data.Length; i++){
				MeshBuffers[ (int) Type].Blocks.Add(Data[i]);
			}
		}
	}
}

public enum ChunkBufferTypes{
	STATIC,
	WATER,
	MAX_COUNT
}