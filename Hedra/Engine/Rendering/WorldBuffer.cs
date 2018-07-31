/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/10/2017
 * Time: 04:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using System.Linq;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of WorldBuffer.
	/// </summary>
	public class WorldBuffer
	{
		public GeometryPool<uint> Indices;
		public GeometryPool<Vector3> Vertices;
		public GeometryPool<Vector3> Normals;
		public GeometryPool<Vector4> Colors;
		public VAO<Vector3, Vector4, Vector3> Data;
		public Dictionary<Vector2, ChunkRenderCommand> ChunkData;
		public int SizeInBytes { get; }
		
		public WorldBuffer(PoolSize Size)
		{
		    const int megabyte = 1048576;
		    float realPoolSize = (int) Size / 100f * (.5f + 0.025f * (GameSettings.MaxLoadingRadius - GameSettings.MinLoadingRadius));

			Indices = new GeometryPool<uint>( (int) (megabyte * 9f * realPoolSize), sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer, BufferUsageHint.DynamicDraw);
			Vertices = new GeometryPool<Vector3>( (int) (megabyte * 7f * realPoolSize), Vector3.SizeInBytes, VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
			Normals = new GeometryPool<Vector3>( (int) (megabyte * 7f * realPoolSize), Vector3.SizeInBytes, VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
			Colors = new GeometryPool<Vector4>( (int) (megabyte * 7f * realPoolSize), Vector4.SizeInBytes, VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
			Data = new VAO<Vector3, Vector4, Vector3>(Vertices.Buffer, Colors.Buffer, Normals.Buffer);
            			 
			ChunkData = new Dictionary<Vector2, ChunkRenderCommand>();
		}
		
		public void Discard(){
			Indices.ObjectMap.Clear();
			Vertices.ObjectMap.Clear();
			Normals.ObjectMap.Clear();
			Colors.ObjectMap.Clear();
		    lock (ChunkData)
                ChunkData.Clear();
		}

	    public void ForceDiscard()
	    {
	        Indices.Discard();
	        Vertices.Discard();
            Normals.Discard();
            Colors.Discard();
            lock (ChunkData)
	            ChunkData.Clear();  
	    }

        public void Remove(Vector2 Offset){
			
			lock(ChunkData){
				if(ChunkData.ContainsKey(Offset)){
					
					ChunkRenderCommand Command = ChunkData[Offset];
					
					Indices.ObjectMap.Remove(Command.Entries[0]);
					Vertices.ObjectMap.Remove(Command.Entries[1]);
					Normals.ObjectMap.Remove(Command.Entries[2]);
					Colors.ObjectMap.Remove(Command.Entries[3]);
					ChunkData.Remove(Offset);
				}
			}
		}

        public bool Add(Vector2 Offset, VertexData Data)
        {
            if (this.Data != null) {

                lock (ChunkData)
                {
                    if (!ChunkData.ContainsKey(Offset))
                    {

                        MemoryEntry[] Entries = new MemoryEntry[4];
                        Entries[1] = Vertices.Allocate(Data.Vertices.ToArray(), Data.Vertices.Count * Vector3.SizeInBytes);
                        Entries[2] = Normals.Allocate(Data.Normals.ToArray(), Data.Normals.Count * Vector3.SizeInBytes);
                        Entries[3] = Colors.Allocate(Data.Colors.ToArray(), Data.Colors.Count * Vector4.SizeInBytes);

                        Entries[0] = this.ReplaceIndices(Data.Indices.ToArray(), Data.Indices.Count * sizeof(uint), new MemoryEntry(), Entries);

                        for (int i = 0; i < Entries.Length; i++)
                        {
                            if (Entries[i] == null)
                                throw new OutOfMemoryException("Geometry pool ran out of space");

                        }
                        ChunkRenderCommand Command = new ChunkRenderCommand();
                        Command.VertexCount = Data.Vertices.Count;
                        Command.DrawCount = Data.Indices.Count;
                        Command.Entries = Entries;

                        ChunkData.Add(Offset, Command);
                    }
                    else
                    {

                        MemoryEntry[] PreviousEntries = ChunkData[Offset].Entries;
                        MemoryEntry[] Entries = new MemoryEntry[4];

                        //Indices are a whole different thing
                        Entries[1] = Vertices.Update(Data.Vertices.ToArray(), Data.Vertices.Count * Vector3.SizeInBytes, PreviousEntries[1]);
                        Entries[2] = Normals.Update(Data.Normals.ToArray(), Data.Normals.Count * Vector3.SizeInBytes, PreviousEntries[2]);
                        Entries[3] = Colors.Update(Data.Colors.ToArray(), Data.Colors.Count * Vector4.SizeInBytes, PreviousEntries[3]);

                        Entries[0] = this.ReplaceIndices(Data.Indices.ToArray(), Data.Indices.Count * sizeof(uint), PreviousEntries[0], Entries);

                        for (int i = 0; i < Entries.Length; i++)
                        {
                            if (Entries[i] == null)
                                throw new OutOfMemoryException("Geometry pool ran out of space.");
                        }

                        var Command = new ChunkRenderCommand
                        {
                            VertexCount = Data.Vertices.Count,
                            DrawCount = Data.Indices.Count,
                            Entries = Entries
                        };

                        ChunkData[Offset] = Command;
                    }
                }
                return true;
            }
            return false;
		}
		
		  public MemoryEntry ReplaceIndices(uint[] Data, int SizeInBytes, MemoryEntry Entry, MemoryEntry[] Entries){
			
			
			if(Entry.Length != SizeInBytes){
				int Offset = 0;
				//Find a gap were this data can be inserted
				Indices.ObjectMap.Sort( (x,y) => MemoryEntry.Compare(x,y)  );
				for(int i = 0; i < Indices.ObjectMap.Count; i++){
					
					if(i == 0 && Indices.ObjectMap[i].Offset > SizeInBytes){
						Offset = 0;
						break;
					}
					
					if(i == Indices.ObjectMap.Count-1){
						Offset = Indices.ObjectMap[Indices.ObjectMap.Count-1].Offset + Indices.ObjectMap[Indices.ObjectMap.Count-1].Length;
						break;
					}
					
					if(Indices.ObjectMap[i] != Entry && Indices.ObjectMap[i+1].Offset - (Indices.ObjectMap[i].Offset + Indices.ObjectMap[i].Length) >= SizeInBytes){
						Offset = Indices.ObjectMap[i].Offset + Indices.ObjectMap[i].Length;
						break;
					}
				}

			    if (Offset + SizeInBytes > Indices.TotalMemory)
			    {
                    this.Indices.DrawAndSave();
                    this.Vertices.DrawAndSave();
                    throw new OutOfMemoryException("GeometryPool<uint> is out of memory");
                }
			    Entry.Offset = Offset;
				Entry.Length = SizeInBytes;
				if(Indices.ObjectMap.Contains(Entry))
					Indices.ObjectMap.Remove(Entry);
				
				Indices.ObjectMap.Add(Entry);

			}
			
			uint NewCount = (uint) (Entries[1].Offset / Vertices.TypeSizeInBytes);
			for(int i = 0; i < Data.Length; i++){
				Data[i] += NewCount;
			}
			
			GL.BindBuffer(Indices.Buffer.BufferTarget, Indices.Buffer.ID);
			GL.BufferSubData(Indices.Buffer.BufferTarget, (IntPtr) (Entry.Offset), SizeInBytes, Data);
			
			return Entry;                                   
		}
		
		public int[] BuildCounts(Dictionary<Vector2, Chunk> ToDraw, out IntPtr[] Offsets, bool Shadows = false){
			int[] Counts;
			
			lock(ChunkData){
				Counts = new int[ChunkData.Count];
				Offsets = new IntPtr[ChunkData.Count];

                int Index = 0; 
				foreach(KeyValuePair<Vector2, ChunkRenderCommand> Pair in ChunkData){
					
					int Count = 0;
					int Offset = 0;
					
					if( ToDraw.ContainsKey(Pair.Key) || Shadows){	
						Count = Pair.Value.DrawCount;
						Offset = Pair.Value.ByteOffset;					
					}
						
					Counts[Index] = Count;
                    Offsets[Index] = (IntPtr) Offset;

                    Index++;
				}
			}
			return Counts;
		}
	}
}
