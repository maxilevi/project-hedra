/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/10/2017
 * Time: 04:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
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
		private readonly object _lock = new object();
		private readonly Dictionary<Vector2, ChunkRenderCommand> _chunkDict;
		private List<KeyValuePair<Vector2, ChunkRenderCommand>> _chunkPairs;
		public GeometryPool<uint> Indices { get; }
		public GeometryPool<Vector3> Vertices { get; }
		public GeometryPool<Vector3> Normals { get; }
		public GeometryPool<Vector4> Colors { get; }
		public VAO<Vector3, Vector4, Vector3> Data { get; }
		public IComparer<KeyValuePair<Vector2, ChunkRenderCommand>> Comparer { get; set; } = Comparer<KeyValuePair<Vector2, ChunkRenderCommand>>.Default;
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
            			 
			_chunkDict = new Dictionary<Vector2, ChunkRenderCommand>();
		}
		
		public void Discard()
		{
			Indices.ObjectMap.Clear();
			Vertices.ObjectMap.Clear();
			Normals.ObjectMap.Clear();
			Colors.ObjectMap.Clear();
			lock (_lock)
			{
				_chunkDict.Clear();
			}
		}

	    public void ForceDiscard()
	    {
	        Indices.Discard();
	        Vertices.Discard();
            Normals.Discard();
            Colors.Discard();
		    lock (_lock)
		    {
			    _chunkDict.Clear();
		    }
	    }

        public void Remove(Vector2 Offset){
			
			lock(_lock)
			{
				if(_chunkDict.ContainsKey(Offset))
				{					
					var command = _chunkDict[Offset];					
					Indices.ObjectMap.Remove(command.Entries[0]);
					Vertices.ObjectMap.Remove(command.Entries[1]);
					Normals.ObjectMap.Remove(command.Entries[2]);
					Colors.ObjectMap.Remove(command.Entries[3]);
					_chunkDict.Remove(Offset);
				}
			}
		}

        public bool Add(Vector2 Offset, VertexData Data)
        {
            if (this.Data != null)
            {

                lock (_lock)
                {
                    if (!_chunkDict.ContainsKey(Offset))
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

                        _chunkDict.Add(Offset, Command);
                    }
                    else
                    {

                        MemoryEntry[] PreviousEntries = _chunkDict[Offset].Entries;
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

                        _chunkDict[Offset] = Command;
                    }

	                _chunkPairs = _chunkDict.ToList();
	                _chunkPairs.Sort(Comparer);
                }
                return true;
            }
            return false;
		}
		
		  public MemoryEntry ReplaceIndices(uint[] Data, int SizeInBytes, MemoryEntry Entry, MemoryEntry[] Entries)
		  {			
			if(Entry.Length != SizeInBytes)
			{
				int Offset = 0;
				//Find a gap were this data can be inserted
				Indices.ObjectMap.Sort( MemoryEntry.Compare  );
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
			
			Renderer.BindBuffer(Indices.Buffer.BufferTarget, Indices.Buffer.ID);
			Renderer.BufferSubData(Indices.Buffer.BufferTarget, (IntPtr) Entry.Offset, (IntPtr) SizeInBytes, Data);
			
			return Entry;                                   
		}
		
		public int[] BuildCounts(Dictionary<Vector2, Chunk> ToDraw, out IntPtr[] Offsets, bool Shadows = false)
		{
			if (_chunkPairs == null)
			{
				Offsets = new IntPtr[0];
				return new int[0];
			}
			
			int[] counts;			
			lock(_lock)
			{
				counts = new int[_chunkDict.Count];
				Offsets = new IntPtr[_chunkDict.Count];

                var index = 0; 
				foreach(var pair in _chunkPairs)
				{		
					var count = 0;
					var offset = 0;				
					if(ToDraw.ContainsKey(pair.Key) || Shadows)
					{	
						count = pair.Value.DrawCount;
						offset = pair.Value.ByteOffset;					
					}				
					counts[index] = count;
                    Offsets[index] = (IntPtr) offset;

                    index++;
				}
			}
			return counts;
		}
	}
}
