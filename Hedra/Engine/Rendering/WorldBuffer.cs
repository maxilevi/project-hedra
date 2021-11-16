/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/10/2017
 * Time: 04:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Framework;
using Hedra.Game;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     Description of WorldBuffer.
    /// </summary>
    public class WorldBuffer
    {
        private readonly Dictionary<Vector2, ChunkRenderCommand> _chunkDict;
        private readonly object _lock = new object();
        private List<KeyValuePair<Vector2, ChunkRenderCommand>> _chunkPairs;
        private uint[] _counts;
        private IntPtr[] _offset;

        public WorldBuffer(PoolSize Size)
        {
            const int megabyte = 1048576;
            var realPoolSize = (int)Size / 100f * 3f;
            Indices = new GeometryPool<uint>((int)(megabyte * 1.25f * realPoolSize), sizeof(uint),
                VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer, BufferUsageHint.DynamicDraw);
            Vertices = new GeometryPool<Vector3>((int)(megabyte * 1f * realPoolSize), HedraSize.Vector3,
                VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            Normals = new GeometryPool<Vector3>((int)(megabyte * 1f * realPoolSize), HedraSize.Vector3,
                VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            Colors = new GeometryPool<Vector4>((int)(megabyte * 1f * realPoolSize), HedraSize.Vector4,
                VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            Data = new VAO<Vector3, Vector4, Vector3>(Vertices.Buffer, Colors.Buffer, Normals.Buffer);

            _offset = new IntPtr[GeneralSettings.MaxChunks];
            _counts = new uint[GeneralSettings.MaxChunks];
            _chunkDict = new Dictionary<Vector2, ChunkRenderCommand>();
        }

        public GeometryPool<uint> Indices { get; }
        public GeometryPool<Vector3> Vertices { get; }
        public GeometryPool<Vector3> Normals { get; }
        public GeometryPool<Vector4> Colors { get; }
        public VAO<Vector3, Vector4, Vector3> Data { get; }
        public IComparer<KeyValuePair<Vector2, ChunkRenderCommand>> Comparer { get; set; }

        public IntPtr[] Offsets => _offset;

        public uint[] Counts => _counts;

        public int AvailableMemory => Indices.AvailableMemory + Vertices.AvailableMemory + Colors.AvailableMemory +
                                      Normals.AvailableMemory;

        public int TotalMemory => Indices.TotalMemory + Vertices.TotalMemory + Colors.TotalMemory + Normals.TotalMemory;

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
            lock (_lock)
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

        public bool Update(Vector2 Offset, NativeVertexData Data)
        {
            if (Data.IsEmpty) return Remove(Offset);
            return Add(Offset, Data);
        }

        public bool Has(Vector2 Offset)
        {
            lock (_lock)
            {
                return _chunkDict.ContainsKey(Offset);
            }
        }

        private bool Add(Vector2 Offset, NativeVertexData Data)
        {
            if (this.Data == null) return false;
            Data.AssertTriangulated();
            lock (_lock)
            {
                if (!_chunkDict.ContainsKey(Offset))
                    AddNew(Offset, Data);
                else
                    AddExisting(Offset, Data);

                if (Comparer == null) throw new ArgumentException("No comparer has been set.");

                _chunkPairs = _chunkDict.ToList();
                _chunkPairs.Sort(Comparer);
            }

            return true;
        }

        private void AddNew(Vector2 Offset, NativeVertexData Data)
        {
            var entries = new MemoryEntry[4];
            entries[1] = Vertices.Allocate(Data.Vertices.InternalArray, Data.Vertices.Count * HedraSize.Vector3);
            entries[2] = Normals.Allocate(Data.Normals.InternalArray, Data.Normals.Count * HedraSize.Vector3);
            entries[3] = Colors.Allocate(Data.Colors.InternalArray, Data.Colors.Count * HedraSize.Vector4);

            entries[0] = ReplaceIndices(Data.Indices.InternalArray, Data.Indices.Count * sizeof(uint),
                new MemoryEntry(), entries);

            for (var i = 0; i < entries.Length; i++)
                if (entries[i] == null)
                    throw new OutOfMemoryException("Geometry pool ran out of space");

            var command = new ChunkRenderCommand();
            command.VertexCount = Data.Vertices.Count;
            command.DrawCount = Data.Indices.Count;
            command.Entries = entries;

            _chunkDict.Add(Offset, command);
        }

        private void AddExisting(Vector2 Offset, NativeVertexData Data)
        {
            var previousEntries = _chunkDict[Offset].Entries;
            var entries = new MemoryEntry[4];

            //Indices are a whole different thing
            entries[1] = Vertices.Update(Data.Vertices.InternalArray, Data.Vertices.Count * HedraSize.Vector3,
                previousEntries[1]);
            entries[2] = Normals.Update(Data.Normals.InternalArray, Data.Normals.Count * HedraSize.Vector3,
                previousEntries[2]);
            entries[3] = Colors.Update(Data.Colors.InternalArray, Data.Colors.Count * HedraSize.Vector4,
                previousEntries[3]);

            entries[0] = ReplaceIndices(Data.Indices.InternalArray, Data.Indices.Count * sizeof(uint),
                previousEntries[0], entries);

            for (var i = 0; i < entries.Length; i++)
                if (entries[i] == null)
                    throw new OutOfMemoryException("Geometry pool ran out of space.");

            var command = new ChunkRenderCommand
            {
                VertexCount = Data.Vertices.Count,
                DrawCount = Data.Indices.Count,
                Entries = entries
            };

            _chunkDict[Offset] = command;
        }

        private MemoryEntry ReplaceIndices(NativeArray<uint> Data, int SizeInBytes, MemoryEntry Entry,
            MemoryEntry[] Entries)
        {
            if (Entry.Length != SizeInBytes)
            {
                var Offset = 0;
                //Find a gap were this data can be inserted
                Indices.ObjectMap.Sort(MemoryEntry.Compare);
                for (var i = 0; i < Indices.ObjectMap.Count; i++)
                {
                    if (i == 0 && Indices.ObjectMap[i].Offset > SizeInBytes)
                    {
                        Offset = 0;
                        break;
                    }

                    if (i == Indices.ObjectMap.Count - 1)
                    {
                        Offset = Indices.ObjectMap[Indices.ObjectMap.Count - 1].Offset +
                                 Indices.ObjectMap[Indices.ObjectMap.Count - 1].Length;
                        break;
                    }

                    if (Indices.ObjectMap[i] != Entry &&
                        Indices.ObjectMap[i + 1].Offset - (Indices.ObjectMap[i].Offset + Indices.ObjectMap[i].Length) >=
                        SizeInBytes)
                    {
                        Offset = Indices.ObjectMap[i].Offset + Indices.ObjectMap[i].Length;
                        break;
                    }
                }

                if (Offset + SizeInBytes > Indices.TotalMemory)
                {
                    Indices.DrawAndSave();
                    Vertices.DrawAndSave();
                    throw new OutOfMemoryException("GeometryPool<uint> is out of memory");
                }

                Entry.Offset = Offset;
                Entry.Length = SizeInBytes;
                if (Indices.ObjectMap.Contains(Entry))
                    Indices.ObjectMap.Remove(Entry);

                Indices.ObjectMap.Add(Entry);
            }

            var newCount = (uint)(Entries[1].Offset / Vertices.TypeSizeInBytes);
            for (var i = 0; i < Data.Length; i++) Data[i] += newCount;

            Indices.UpdateBuffer(Data, Entry.Offset, SizeInBytes);
            return Entry;
        }

        public int BuildCounts(Dictionary<Vector2, Chunk> ToDraw)
        {
            return BuildCounts(ToDraw, ref _offset, ref _counts);
        }

        public int BuildCounts(Dictionary<Vector2, Chunk> ToDraw, ref IntPtr[] OffsetsArray, ref uint[] CountsArray)
        {
            if (_chunkPairs == null) return 0;
            var index = 0;
            lock (_lock)
            {
                foreach (var pair in _chunkPairs)
                {
                    var count = 0;
                    var offset = 0;
                    if (ToDraw.ContainsKey(pair.Key))
                    {
                        count = pair.Value.DrawCount;
                        offset = pair.Value.Entries[0].Offset;
                    }

                    CountsArray[index] = (uint)count;
                    OffsetsArray[index] = (IntPtr)offset;

                    index++;
                }
            }

            return _chunkPairs.Count;
        }
    }
}