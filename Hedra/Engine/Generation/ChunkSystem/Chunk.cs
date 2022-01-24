/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 07:37 p.m.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Bullet;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Framework;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class Chunk : IDisposable, IPositionable
    {
        public const float BlockSize = 4.0f;
        public const int Height = 256;
        public const int Width = 128;
        public const int BoundsX = (int)(Width / BlockSize);
        public const int BoundsY = Height;
        public const int BoundsZ = (int)(Width / BlockSize);
        private static readonly Block _defaultBlock;
        private readonly object _blocksLock;
        private readonly object _heightCacheLock = new object();
        private readonly RegionCache _regionCache;
        private readonly ChunkStructuresMeshBuilder _structuresBuilder;
        private readonly ChunkTerrainMeshBuilder _terrainBuilder;
        private readonly object _waterLock = new object();
        private readonly Block[] _blocks;
        private int _currentNeedsRebuildCount;
        private byte[] _heightCache;
        private int _needsRebuildCount;

        private List<CoordinateHash3D> _waterPositions;

        public Chunk(int OffsetX, int OffsetZ)
        {
            this.OffsetX = OffsetX;
            this.OffsetZ = OffsetZ;
            Position = new Vector3(OffsetX, 0, OffsetZ);
            Mesh = new ChunkMesh(Position, null);
            if (World.GetChunkByOffset(this.OffsetX, this.OffsetZ) != null)
                throw new ArgumentNullException($"A chunk with the coodinates ({OffsetX}, {OffsetZ}) already exists.");
            _terrainBuilder = new ChunkTerrainMeshBuilder(this);
            _structuresBuilder = new ChunkStructuresMeshBuilder(this);
            _blocksLock = new object();
            _blocks = new Block[BoundsX * BoundsY * BoundsZ];
            _regionCache = new RegionCache(Position, Position + new Vector3(Width, 0, Width));
            Biome = World.BiomePool.GetPredominantBiome(this);
            Landscape = World.BiomePool.GetGenerator(this);
            Automatons = new ChunkAutomatons(this);
        }

        public Region Biome { get; }
        public bool BuildedCompletely { get; set; }
        public int BuildedLod { get; private set; }
        public bool BuildedWithStructures { get; private set; }
        public bool Disposed { get; private set; }
        public bool IsRiverConstant { get; private set; }
        public bool IsGenerated { get; private set; }
        public BiomeGenerator Landscape { get; }
        public int Lod { get; set; } = 1;
        public ChunkMesh Mesh { get; }
        public bool NeedsRebuilding => _needsRebuildCount > 0;
        public bool NeverBuilded { get; private set; } = true;
        public int OffsetX { get; }
        public int OffsetZ { get; }
        private bool IsBuilding { get; set; }
        private bool IsGenerating { get; set; }
        public ChunkAutomatons Automatons { get; }

        public bool NeighboursExist
        {
            get
            {
                var n1 = World.GetChunkByOffset(OffsetX + Width, OffsetZ);
                var n2 = World.GetChunkByOffset(OffsetX, OffsetZ + Width);
                var n3 = World.GetChunkByOffset(OffsetX + Width, OffsetZ + Width);
                var n4 = World.GetChunkByOffset(OffsetX - Width, OffsetZ);
                var n5 = World.GetChunkByOffset(OffsetX, OffsetZ - Width);
                var n6 = World.GetChunkByOffset(OffsetX - Width, OffsetZ - Width);
                var n7 = World.GetChunkByOffset(OffsetX + Width, OffsetZ - Width);
                var n8 = World.GetChunkByOffset(OffsetX - Width, OffsetZ + Width);

                return n1 != null && n2 != null && n3 != null && n4 != null
                       && n5 != null && n6 != null && n7 != null && n8 != null
                       && n1.IsGenerated && n1.Landscape.StructuresPlaced
                       && n2.IsGenerated && n2.Landscape.StructuresPlaced
                       && n3.IsGenerated && n3.Landscape.StructuresPlaced
                       && n4.IsGenerated && n4.Landscape.StructuresPlaced
                       && n5.IsGenerated && n5.Landscape.StructuresPlaced
                       && n6.IsGenerated && n6.Landscape.StructuresPlaced
                       && n7.IsGenerated && n7.Landscape.StructuresPlaced
                       && n8.IsGenerated && n8.Landscape.StructuresPlaced;
            }
        }

        public ChunkMesh StaticBuffer => Mesh;

        public ReadOnlyCollection<VertexData> StaticElements => Mesh.Elements.AsReadOnly();

        public int MaximumHeight => _terrainBuilder.Sparsity?.MaximumHeight ?? BoundsY - 1;
        public int MinimumHeight => _terrainBuilder.Sparsity?.MinimumHeight ?? 0;

        public Block this[int Index] => _blocks[Index];


        public int MemoryUsed => IsCompressed ? 0 : (_blocks?.Length ?? BoundsX * BoundsZ * BoundsY) * sizeof(ushort);

        public bool IsCompressed => _blocks == null;

        public bool HasWater
        {
            get
            {
                lock (_waterLock)
                {
                    return _waterPositions != null;
                }
            }
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            RoutineManager.StartRoutine(DisposeCoroutine);
        }

        public Vector3 Position { get; private set; }

        public void GenerateBlocks()
        {
            if (Disposed) throw new ArgumentException("Cannot build a disposed chunk.");
            lock (_blocksLock)
            {
                IsGenerating = true;
                var details = Landscape.GenerateBlocks(_blocks);
                IsRiverConstant = details.IsRiverConstant;
                FillHeightCache(_blocks);
                IsGenerating = false;
            }
        }

        private void FillHeightCache(Block[] Blocks)
        {
            int GetHighestY(int X, int Z)
            {
                for (var y = MaximumHeight; y > MinimumHeight; y--)
                {
                    var type = Blocks[X * BoundsZ * BoundsY + y * BoundsZ + Z].Type;
                    if (type != BlockType.Air && type != BlockType.Water)
                        return y;
                }

                return 0;
            }

            lock (_heightCacheLock)
            {
                _heightCache = new byte[BoundsX * BoundsZ];
                for (var x = 0; x < BoundsX; x++)
                for (var z = 0; z < BoundsZ; z++)
                    _heightCache[x * BoundsZ + z] = (byte)GetHighestY(x, z);
            }
        }

        public void GenerateStructures()
        {
            if (Disposed) throw new ArgumentException("Cannot build a disposed chunk.");
            lock (_blocksLock)
            {
                IsGenerating = true;
                Landscape.GenerateEnvironment(_regionCache);
                IsGenerating = false;
            }

            IsGenerated = true;
        }

        public void BuildMesh()
        {
            if (Disposed || !IsGenerated || !Landscape.BlocksSetted || !Landscape.StructuresPlaced) return;
            if (_terrainBuilder.Sparsity == null) BuildSparsity();
            var buildingLod = Lod;
            PrepareForBuilding();
            var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 128, Allocator.Megabyte * 16);
            SetupCollider(allocator, buildingLod);
            if (!allocator.IsEmpty) throw new ArgumentOutOfRangeException("Detected memory leak");
            
            var output = CreateTerrainMesh(allocator, buildingLod);
            if (output == null) return;
            SetChunkStatus(output);
            output = AddStructuresMeshes(allocator, output, buildingLod);

            if (output == null) return;
            /* The allocator will be destroyed when the mesh is uploaded */
            UploadMesh(allocator, output);
            FinishUpload(output, buildingLod);
        }

        private void SetupCollider(IAllocator Allocator, int BuildingLod)
        {
            if (BuildingLod == 1 || BuildingLod == 2)
            {
                var mesh = CreateCollisionTerrainMesh(Allocator);
                BulletPhysics.AddChunk(Position.Xz(), mesh, Mesh.CollisionShapes, Mesh.Offsets);
                mesh.Dispose();
            }
            else
            {
                BulletPhysics.RemoveChunk(Position.Xz());
            }
        }

        private void BuildSparsity()
        {
            /* We should build the sparsity data when all the neighbours exist */
            _terrainBuilder.Sparsity = ChunkSparsity.From(this);
        }

        private NativeVertexData CreateCollisionTerrainMesh(IAllocator Allocator)
        {
            lock (_blocksLock)
            {
                return _terrainBuilder.CreateTerrainCollisionMesh(_regionCache, Allocator);
            }
        }

        private ChunkMeshBuildOutput CreateTerrainMesh(IAllocator Allocator, int LevelOfDetail)
        {
            lock (_blocksLock)
            {
                return _terrainBuilder.CreateTerrainMesh(Allocator, LevelOfDetail, _regionCache);
            }
        }

        private ChunkMeshBuildOutput AddStructuresMeshes(IAllocator Allocator, ChunkMeshBuildOutput Input,
            int LevelOfDetail)
        {
            return _structuresBuilder.AddStructuresMeshes(Allocator, Input, LevelOfDetail);
        }

        private void SetChunkStatus(ChunkMeshBuildOutput Input)
        {
            BuildedCompletely = !Input.Failed;
            Position = new Vector3(OffsetX, 0, OffsetZ);
            Mesh.IsGenerated = true;
            NeverBuilded = false;
        }

        private void PrepareForBuilding()
        {
            IsBuilding = true;
            BuildedCompletely = false;
            _currentNeedsRebuildCount = _needsRebuildCount;
        }

        private void FinishUpload(ChunkMeshBuildOutput Input, int BuildedLod)
        {
            this.BuildedLod = BuildedLod;
            BuildedWithStructures = true;
            _needsRebuildCount -= _currentNeedsRebuildCount;
            IsBuilding = false;
        }

        private void UploadMesh(IAllocator InputAllocator, ChunkMeshBuildOutput Input)
        {
            var meshInvalid = Input.StaticData.Colors.Count != Input.StaticData.Vertices.Count ||
                              Input.InstanceData.Colors.Count != Input.InstanceData.Vertices.Count ||
                              Input.InstanceData.Extradata.Count != Input.InstanceData.Vertices.Count ||
                              Input.StaticData.Extradata.Count != Input.StaticData.Vertices.Count;
            if (meshInvalid)
            {
                Log.WriteLine("Chunk index mismatch, skipping...");
                InputAllocator.Dispose();
                return;
            }

            var staticMin = new Vector3(
                Input.StaticData.SupportPoint(-Vector3.UnitX).X - OffsetX,
                Input.StaticData.SupportPoint(-Vector3.UnitY).Y,
                Input.StaticData.SupportPoint(-Vector3.UnitZ).Z - OffsetZ
            );

            var staticMax = new Vector3(
                Input.StaticData.SupportPoint(Vector3.UnitX).X - OffsetX,
                Input.StaticData.SupportPoint(Vector3.UnitY).Y,
                Input.StaticData.SupportPoint(Vector3.UnitZ).Z - OffsetZ
            );

            Mesh.SetBounds(
                new Vector3(staticMin.X, staticMin.Y, staticMin.Z),
                new Vector3(staticMax.X, Math.Max(staticMax.Y, Input.WaterData.SupportPoint(Vector3.UnitY).Y),
                    staticMax.Z)
            );
            using (var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 64, Allocator.Megabyte * 8))
            {
                Input.StaticData.Optimize(allocator);
                Input.InstanceData.Optimize(allocator);
                Input.WaterData.Optimize(allocator);
            }

            DistributedExecuter.Execute(delegate
            {
                var staticResult = WorldRenderer.UpdateStatic(new Vector2(OffsetX, OffsetZ), Input.StaticData);
                var instanceResult = WorldRenderer.UpdateInstance(new Vector2(OffsetX, OffsetZ), Input.InstanceData);
                var waterResult = WorldRenderer.UpdateWater(new Vector2(OffsetX, OffsetZ), Input.WaterData);

                BuildedCompletely = BuildedCompletely && instanceResult && staticResult && waterResult;

                if (Mesh != null)
                {
                    Mesh.IsBuilded = true;
                    Mesh.Enabled = true;
                    Mesh.BuildedOnce = true;
                }

                Input.StaticData.Dispose();
                Input.InstanceData.Dispose();
                Input.WaterData.Dispose();
                InputAllocator.Dispose();
            });
        }

        public Block GetBlockAt(int X, int Y, int Z)
        {
            if (Disposed || !Landscape.BlocksSetted || Y >= BoundsY || Y < 0) return _defaultBlock;
            return this[X * BoundsZ * BoundsY + Y * BoundsZ + Z];
        }

        public void SetBlockAt(int X, int Y, int Z, BlockType Type)
        {
            _blocks[X * BoundsZ * BoundsY + Y * BoundsZ + Z] =
                new Block(Type, _blocks[X * BoundsZ * BoundsY + Y * BoundsZ + Z].Density);
        }

        public int GetHighestY(int X, int Z)
        {
            lock (_heightCacheLock)
            {
                return _heightCache?[X * BoundsX + Z] ?? 0;
            }
        }

        public float GetHighest(int X, int Z)
        {
            if (Disposed || !Landscape.BlocksSetted) return 0;
            var y = GetHighestY(X, Z);
            var b1 = GetBlockAt(X, y, Z);
            return y + Mathf.Clamp(b1.Density, -0.0f, 0.65f);
        }

        public Block GetHighestBlockAt(int X, int Z)
        {
            if (Disposed || !Landscape.BlocksSetted) return new Block();
            return GetBlockAt(X, GetHighestY(X, Z), Z);
        }

        public void AddStaticElement(params VertexData[] Data)
        {
            if (Mesh == null) throw new ArgumentException("Failed to add static element");

            lock (Mesh.Elements)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.Elements.Add(Data[i]);
            }

            _needsRebuildCount++;
        }

        public void RemoveStaticElement(params VertexData[] Data)
        {
            if (Mesh == null) throw new ArgumentException("Failed to remove static element");

            lock (Mesh.Elements)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.Elements.Remove(Data[i]);
            }

            _needsRebuildCount++;
        }

        public void AddInstance(InstanceData Data, bool AffectedByLod = false)
        {
            if (Mesh == null) throw new ArgumentException("Failed to add instance data ");

            StaticBuffer.AddInstance(Data, AffectedByLod);
            _needsRebuildCount++;
        }

        public void RemoveInstance(InstanceData Data)
        {
            if (Mesh == null) throw new ArgumentException("Failed to remove instance data ");

            StaticBuffer.RemoveInstance(Data);
            _needsRebuildCount++;
        }

        public void AddCollisionShape(params CollisionShape[] Data)
        {
            if (Mesh == null) throw new ArgumentException("Failed to add collision shape");

            for (var i = 0; i < Data.Length; i++)
                Mesh.Add(Data[i]);
        }

        public void AnalyzeCompression()
        {
            /*
            if (_activityTimer.Tick() && !IsCompressed && BuildedCompletely)
            {
                _rleBlocks = CompressRLE(_blocks);
                _blocks = null;
            }*/
        }

        public void ForceDispose()
        {
            BulletPhysics.RemoveChunk(Position.Xz());
            Mesh?.Dispose();
            Landscape?.Dispose();
            ChunkTerrainMeshBuilder.ClearMapping(Position);
        }

        private static byte[] CompressRLE(Block[] Blocks)
        {
            ushort lastBits = 0;
            ushort count = 0;

            void Add(BinaryWriter Writer)
            {
                Writer.Write(lastBits);
                Writer.Write(count > 1);
                if (count > 1)
                    Writer.Write(count);
            }

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    for (var x = 0; x < BoundsX; ++x)
                    for (var y = 0; y < BoundsY; ++y)
                    for (var z = 0; z < BoundsZ; ++z)
                    {
                        var block = Blocks[x * BoundsY * BoundsZ + y * BoundsZ + z];
                        if (lastBits == block._bits)
                        {
                            count++;
                        }
                        else if (count != 0)
                        {
                            Add(bw);
                            lastBits = block._bits;
                            count = 1;
                        }
                        else
                        {
                            lastBits = block._bits;
                            count = 1;
                        }
                    }

                    Add(bw);
                }

                return ms.ToArray();
            }
        }

        private static Block[] DecompressRLE(byte[] RLE)
        {
            var blocks = new Block[BoundsX * BoundsZ * BoundsY];
            var index = 0;
            using (var ms = new MemoryStream(RLE))
            {
                using (var br = new BinaryReader(ms))
                {
                    while (ms.Position < ms.Length)
                    {
                        var bits = br.ReadUInt16();
                        var isMoreThanOne = br.ReadBoolean();
                        var count = 1;
                        if (isMoreThanOne)
                            count = br.ReadUInt16();

                        for (var i = 0; i < count; ++i) blocks[index++]._bits = bits;
                    }
                }
            }

            return blocks;
        }

        public void AddWaterDensity(int X, int Y, int Z)
        {
            lock (_waterLock)
            {
                if (_waterPositions == null) _waterPositions = new List<CoordinateHash3D>();
                var hash = new CoordinateHash3D(X, Y, Z);
                _waterPositions.Add(hash);
            }
        }

        public NativeArray<CoordinateHash3D> GetWaterPositions(IAllocator Allocator)
        {
            lock (_waterLock)
            {
                return _waterPositions.ToNativeArray(Allocator);
            }
        }

        public void Test()
        {
            if (_blocks == null) return;
            var sw = new Stopwatch();
            sw.Start();
            var bytes = CompressRLE(_blocks);
            sw.Stop();
            Chat.Log($"RLE took '{sw.ElapsedMilliseconds}' MS and compressed 512KB into '{bytes.Length / 1024}'KB");
        }

        public int GetHighestWaterY(int X, int Z)
        {
            var nearestWaterBlockY = 0;
            for (var y = MaximumHeight; y > MinimumHeight; y--)
            {
                var block = GetBlockAt(X, y, Z);
                if (block.Type == BlockType.Water)
                {
                    nearestWaterBlockY = y;
                    break;
                }
            }

            return nearestWaterBlockY;
        }

        private IEnumerator DisposeCoroutine()
        {
            var time = 0f;
            while ((IsBuilding || IsGenerating) && time < 2.5f)
            {
                time += Time.DeltaTime;
                yield return null;
            }

            ForceDispose();
        }
    }
}