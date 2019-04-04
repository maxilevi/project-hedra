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
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Rendering;

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
        public IComparer<KeyValuePair<Vector2, ChunkRenderCommand>> Comparer { get; set; }
        private IntPtr[] _offset;
        private int[] _counts;
        
        public WorldBuffer(PoolSize Size)
        {
            const int megabyte = 1048576;
            var realPoolSize = ((int) Size / 100f) * 8f;
            Indices = new GeometryPool<uint>( (int) (megabyte * 1.25f * realPoolSize), sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer, BufferUsageHint.DynamicDraw);
            Vertices = new GeometryPool<Vector3>( (int) (megabyte * 1f * realPoolSize), Vector3.SizeInBytes, VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            Normals = new GeometryPool<Vector3>( (int) (megabyte * 1f * realPoolSize), Vector3.SizeInBytes, VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            Colors = new GeometryPool<Vector4>( (int) (megabyte * 1f * realPoolSize), Vector4.SizeInBytes, VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            Data = new VAO<Vector3, Vector4, Vector3>(Vertices.Buffer, Colors.Buffer, Normals.Buffer);
                         
            _offset = new IntPtr[GeneralSettings.MaxChunks];
            _counts = new int[GeneralSettings.MaxChunks];
            _chunkDict = new Dictionary<Vector2, ChunkRenderCommand>();
        }

        public void Bind(bool EnableVertexAttributes = true)
        {
            Data.Bind(EnableVertexAttributes);
        }

        public void BindIndices()
        {
            Indices.Bind();
        }
        
        public void UnbindIndices()
        {
            Indices.Unbind();
        }

        public void Unbind()
        {
            Data.Unbind();
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

        public bool Remove(Vector2 Offset)
        {
            
            lock(_lock)
            {
                if (_chunkDict.ContainsKey(Offset))
                {                    
                    var command = _chunkDict[Offset];                    
                    Indices.ObjectMap.Remove(command.Entries[0]);
                    Vertices.ObjectMap.Remove(command.Entries[1]);
                    Normals.ObjectMap.Remove(command.Entries[2]);
                    Colors.ObjectMap.Remove(command.Entries[3]);
                    _chunkDict.Remove(Offset);
                }
            }
            return true;
        }

        public bool Update(Vector2 Offset, VertexData Data)
        {
            if (Data.IsEmpty)
            {
                return Remove(Offset);
            }
            return Add(Offset, Data);      
        }
        
        public bool Has(Vector2 Offset)
        {
            lock (_lock)
                return _chunkDict.ContainsKey(Offset);
        }

        private bool Add(Vector2 Offset, VertexData Data)
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

                    if (Comparer == null) throw new ArgumentException("No comparer has been set.");

                    _chunkPairs = _chunkDict.ToList();
                    _chunkPairs.Sort(Comparer);
                }
                return true;
            }
            return false;
        }

        private MemoryEntry ReplaceIndices(uint[] Data, int SizeInBytes, MemoryEntry Entry, MemoryEntry[] Entries)
          {            
            if(Entry.Length != SizeInBytes)
            {
                int Offset = 0;
                //Find a gap were this data can be inserted
                Indices.ObjectMap.Sort( MemoryEntry.Compare  );
                for(int i = 0; i < Indices.ObjectMap.Count; i++){
                    
                    if(i == 0 && Indices.ObjectMap[i].Offset > SizeInBytes)
                    {
                        Offset = 0;
                        break;
                    }
                    
                    if(i == Indices.ObjectMap.Count-1)
                    {
                        Offset = Indices.ObjectMap[Indices.ObjectMap.Count-1].Offset + Indices.ObjectMap[Indices.ObjectMap.Count-1].Length;
                        break;
                    }
                    
                    if(Indices.ObjectMap[i] != Entry && Indices.ObjectMap[i+1].Offset - (Indices.ObjectMap[i].Offset + Indices.ObjectMap[i].Length) >= SizeInBytes)
                    {
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
            
            var newCount = (uint) (Entries[1].Offset / Vertices.TypeSizeInBytes);
            for(var i = 0; i < Data.Length; i++)
            {
                Data[i] += newCount;
            }
            
            Indices.UpdateBuffer(Data, Entry.Offset, SizeInBytes);
            return Entry;                                   
        }

        public int BuildCounts(Dictionary<Vector2, Chunk> ToDraw)
        {
            return BuildCounts(ToDraw, ref _offset, ref _counts);
        }

        public int BuildCounts(Dictionary<Vector2, Chunk> ToDraw, ref IntPtr[] OffsetsArray, ref int[] CountsArray)
        {
            if (_chunkPairs == null) return 0;
            var index = 0;
            lock (_lock)
            {
                foreach(var pair in _chunkPairs)
                {        
                    var count = 0;
                    var offset = 0;                
                    if(ToDraw.ContainsKey(pair.Key))
                    {    
                        count = pair.Value.DrawCount;
                        offset = pair.Value.Entries[0].Offset;                    
                    }                
                    CountsArray[index] = count;
                    OffsetsArray[index] = (IntPtr) offset;

                    index++;
                }
            }
            return _chunkPairs.Count;
        }

        public IntPtr[] Offsets => _offset;
        
        public int[] Counts => _counts;
        
        public int AvailableMemory => Indices.AvailableMemory + Vertices.AvailableMemory + Colors.AvailableMemory + Normals.AvailableMemory;
        
        public int TotalMemory => Indices.TotalMemory + Vertices.TotalMemory + Colors.TotalMemory + Normals.TotalMemory;
    }
}
