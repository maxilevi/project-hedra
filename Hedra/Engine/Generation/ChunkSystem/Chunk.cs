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
using System.Linq;
using System.Runtime.CompilerServices;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Core;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using OpenTK;
using Region = Hedra.BiomeSystem.Region;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class Chunk : IDisposable, IPositionable
    {
        public const float BlockSize = 4.0f;
        public const int Height = 256;
        public const int Width = 128;
        public Region Biome { get; }
        public int BoundsX { get; private set; }
        public int BoundsY { get; private set; }
        public int BoundsZ { get; private set; }
        public bool BuildedCompletely { get; set; }
        public int BuildedLod { get; private set; }
        public bool BuildedWithStructures { get; private set; }
        public bool Disposed { get; private set; }
        public bool HasWater { get; private set; }
        public bool IsGenerated { get; private set; }
        public BiomeGenerator Landscape { get; private set; }
        public int Lod { get; set; } = 1;
        public ChunkMesh Mesh { get; private set; }
        public bool NeedsRebuilding { get; private set; }
        public bool NeverBuilded { get; private set; } = true;
        public int OffsetX { get; }
        public int OffsetZ { get; }
        private bool IsBuilding { get; set; }
        private bool IsGenerating { get; set; }
        public Vector3 Position { get; private set; }

        private Block[][][] _blocks;
        private static readonly Block[][] _dummyBlocks;
        private Dictionary<CoordinateHash3D, Half> _waterDensity;
        private readonly object _waterLock = new object();
        private readonly VertexData _nearestVertexData;
        private readonly ChunkTerrainMeshBuilder _terrainBuilder;
        private readonly ChunkStructuresMeshBuilder _structuresBuilder;
        private readonly object _terrainVerticesLock;
        private readonly object _blocksLock;
        private readonly RegionCache _regionCache;
        private GridCell _nearestVertexCell;
        private Vector3[] _terrainVertices;

        static Chunk()
        {
            _dummyBlocks = new Block[Height][];
            for (var i = 0; i < _dummyBlocks.Length; i++)
            {
                _dummyBlocks[i] = new Block[(int) (Chunk.Width / Chunk.BlockSize)];
            }
        }
        
        public Chunk(int OffsetX, int OffsetZ)
        {
            this.OffsetX = OffsetX;
            this.OffsetZ = OffsetZ;
            this.Position = new Vector3(OffsetX, 0, OffsetZ);
            if (World.GetChunkByOffset(this.OffsetX, this.OffsetZ) != null)
                throw new ArgumentNullException($"A chunk with the coodinates ({OffsetX}, {OffsetZ}) already exists.");
            _blocks = new Block[(int) (Width / BlockSize)][][];
            _nearestVertexData = new VertexData();
            _terrainBuilder = new ChunkTerrainMeshBuilder(this);
            _structuresBuilder = new ChunkStructuresMeshBuilder(this);
            _terrainVertices = new Vector3[0];
            _terrainVerticesLock = new object();
            _blocksLock = new object();
            _regionCache = new RegionCache(Position, Position + new Vector3(Chunk.Width, 0, Chunk.Width));

            Biome = World.BiomePool.GetPredominantBiome(this);
            Landscape = new LandscapeGenerator(this);
        }

        public void Initialize()
        {
            Mesh = new ChunkMesh(Position, null);
        }

        public void Generate()
        {
            if (Disposed) throw new ArgumentException($"Cannot build a disposed chunk.");
            if (!Initialized) throw new ArgumentException($"Chunk hasnt been initialized yet.");
            lock (_blocksLock)
            {
                IsGenerating = true;
                Landscape.Generate(_blocks, _regionCache);
                IsGenerating = false;
            }
            CalculateBounds();
            IsGenerated = true;
        }

        public void BuildMesh()
        {
            if (Disposed || !IsGenerated || !Landscape.BlocksSetted || !Landscape.StructuresPlaced) return;
            if (_terrainBuilder.Sparsity == null) BuildSparsity();
            var buildingLod = this.Lod;
            this.PrepareForBuilding();
            var output = this.CreateTerrainMesh(buildingLod);

            if (output == null) return;
            this.SetChunkStatus(output);
            output = this.AddStructuresMeshes(output, buildingLod);

            if (output == null) return;
            this.UploadMesh(output);
            this.FinishUpload(output, buildingLod);
        }

        private void BuildSparsity()
        {
            /* We should build the sparsity data when all the neighbours exist */
            _terrainBuilder.Sparsity = ChunkSparsity.From(this);
            /* Landscape.Cull(_blocks, _terrainBuilder.Sparsity); */
        }
        
        private ChunkMeshBuildOutput CreateTerrainMesh(int LevelOfDetail)
        {
            lock (_blocks)
            {
                return _terrainBuilder.CreateTerrainMesh(_blocks, LevelOfDetail, _regionCache);
            }
        }

        private ChunkMeshBuildOutput AddStructuresMeshes(ChunkMeshBuildOutput Input, int LevelOfDetail)
        {
            return _structuresBuilder.AddStructuresMeshes(Input, LevelOfDetail);
        }

        public void SetTerrainVertices(ChunkMeshBuildOutput Input)
        {
            lock (_terrainVerticesLock)
            {
                if (BuildedLod == 1 && Lod != 1)
                {
                    _terrainVertices = null;
                }

                if (Lod != 1 || !Input.HasNoise3D) return;
                //_terrainVertices = Input.StaticData.Vertices.Select( V => V - new Vector3(OffsetX, 0, OffsetZ)).ToArray();
            }
        }

        public void CalculateBounds()
        {
            BoundsX = _blocks.Length;
            BoundsY = _blocks[0].Length;
            BoundsZ = _blocks[0][0].Length;
        }

        private void SetChunkStatus(ChunkMeshBuildOutput Input)
        {
            if (!Initialized) throw new ArgumentException($"Chunk hasnt been initialized yet.");
            this.BuildedCompletely = !Input.Failed;
            this.Position = new Vector3(OffsetX, 0, OffsetZ);
            this.Mesh.IsGenerated = true;
            this.NeverBuilded = false;
        }

        private void PrepareForBuilding()
        {
            this.IsBuilding = true;
            this.BuildedCompletely = false;
        }

        private void FinishUpload(ChunkMeshBuildOutput Input, int BuildedLod)
        {
            this.BuildedLod = BuildedLod;
            this.BuildedWithStructures = true;
            this.NeedsRebuilding = false;
            this.IsBuilding = false;
            this.HasWater = Input.HasWater;
        }

        private void UploadMesh(ChunkMeshBuildOutput Input)
        {
            if (Mesh == null ||
                Input.StaticData.Colors.Count != Input.StaticData.Vertices.Count ||
                Input.InstanceData.Colors.Count != Input.InstanceData.Vertices.Count)
                throw new ArgumentException("Chunk index mismatch.");

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
                new Vector3(staticMax.X, Math.Max(staticMax.Y, Input.WaterData.SupportPoint(Vector3.UnitY).Y), staticMax.Z)
            );
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

                Input.StaticData?.Dispose();
                Input.InstanceData?.Dispose();
                Input.WaterData?.Dispose();
            });
        }

        public Block GetBlockAt(int X, int Y, int Z)
        {
            if (Disposed) return new Block();

            if (Landscape != null && Landscape.BlocksSetted)
            {
                if (X >= 0 && Y >= 0 && Z >= 0 && X <= BoundsX - 1 && Y <= BoundsY - 1 && Z <= BoundsZ - 1)
                    return _blocks[X][Y][Z];

                return new Block();
            }

            return new Block();
        }

        public Vector3 NearestVertex(Vector3 Position)
        {
            return Vector3.Zero;
            /*
            //if (_terrainVertices == null)
            {
                if (_nearestVertexCell.P == null)
                {
                    _nearestVertexCell.P = new Vector3[8];
                    _nearestVertexCell.Type = new BlockType[8];
                    _nearestVertexCell.Density = new double[8];
                }

                int width = _blocks.Length;
                int height = _blocks[0].Length;
                int depth = _blocks[0][0].Length;

                //Check for air blocks
                Vector3 blockSpace = World.ToBlockSpace(Position);
                int x = (int) blockSpace.X, z = (int) blockSpace.Z, y = (int) blockSpace.Y;

                var success = false;
                _terrainBuilder.Helper.CreateCell(ref _nearestVertexCell, x, y, z, true,
                    _blocks[x][y][z].Type == BlockType.Water && _blocks[x][y + 1][z].Type == BlockType.Air, Lod, out success);

                _nearestVertexData.Vertices.Clear();
                _nearestVertexData.Colors.Clear();
                _nearestVertexData.Normals.Clear();
                _nearestVertexData.Indices.Clear();
                MarchingCubes.Process(0f, _nearestVertexCell, Vector4.One, (x + z) % 2 == 0, _nearestVertexData);
                var dist = float.MaxValue;
                var point = Vector3.Zero;
                for (var i = 0; i < _nearestVertexData.Vertices.Count; i++)
                {
                    var newDist = (Position - _nearestVertexData.Vertices[i]).LengthSquared;
                    if (newDist < dist)
                    {
                        dist = newDist;
                        point = _nearestVertexData.Vertices[i];
                    }
                }
                return point;
            }
            
            lock (_terrainVerticesLock)
            {
                Vector3 nearest0 = Vector3.Zero,
                    nearest1 = Vector3.Zero,
                    nearest2 = Vector3.Zero,
                    nearest3 = Vector3.Zero;
                float dist0 = float.MaxValue;

                for (var i = 0; i < _terrainVertices.Length; i++)
                {
                    float newDist = (_terrainVertices[i] - Position).LengthSquared;
                    if (!(newDist < dist0)) continue;
                    dist0 = newDist;

                    nearest3 = nearest2;
                    nearest2 = nearest1;
                    nearest1 = nearest0;
                    nearest0 = _terrainVertices[i];
                }

                return nearest0;// + 1 * Vector3.UnitY;
            }*/
        }

        public void AddWaterDensity(Vector3 WaterPosition, Half Density)
        {
            lock (_waterLock)
            {
                if (_waterDensity == null) _waterDensity = new Dictionary<CoordinateHash3D, Half>();
                var hash = new CoordinateHash3D(WaterPosition);
                if (!_waterDensity.ContainsKey(hash)) _waterDensity.Add(hash, Density);
            }
        }

        public CoordinateHash3D[] GetWaterPositions()
        {
            lock (_waterLock)
            {
                return _waterDensity.Keys.ToArray();
            }
        }

        public Half GetWaterDensity(Vector3 WaterPosition)
        {
            lock (_waterLock)
            {
                var hash = new CoordinateHash3D(WaterPosition);
                return _waterDensity?.ContainsKey(hash) ?? false ? _waterDensity[hash] : default(Half);
            }
        }

        public int GetHighestY(int X, int Z)
        {
            if (Disposed) return 0;
            if (Landscape == null || !Landscape.BlocksSetted) return 0;
            for (var y = MaximumHeight; y > MinimumHeight; y--)
            {
                var type = _blocks[X][y][Z].Type;
                if (type != BlockType.Air && type != BlockType.Water)
                    return y;
            }

            return 0;
        }

        public float GetHighest(int X, int Z)
        {
            if (Disposed) return 0;
            if (Landscape == null || !Landscape.BlocksSetted) return 0;
            for (var y = MaximumHeight; y > MinimumHeight; y--)
            {
                var type = _blocks[X][y][Z].Type;
                if (type != BlockType.Air && type != BlockType.Water)
                    return _blocks[X][y][Z].Density + y;
            }

            return 0;
        }

        public int GetLowestY(int X, int Z)
        {
            if (Disposed || X < 0 || Z < 0 || Landscape == null || !Landscape.BlocksSetted) return 0;
            for (var y = MinimumHeight; y < MaximumHeight; y++)
            {
                var block = _blocks[X][y][Z];
                if (block.Type == BlockType.Air || block.Type == BlockType.Water)
                    return y - 1;
            }

            return 0;
        }

        public int GetNearestY(int X, int Y, int Z)
        {
            if (Disposed) return 0;
            Y = Math.Max(0, Y);

            if (Landscape != null && Landscape.BlocksSetted)
            {
                for (int y = Y; y < MaximumHeight; y++)
                {
                    var block = _blocks[X][y][Z];
                    if (block.Type != BlockType.Air && block.Type != BlockType.Water &&
                        _blocks[X][y + 1][Z].Type == BlockType.Air)
                        return y;
                }
            }

            return Y;
        }

        public Block GetNearestBlockAt(int X, int Y, int Z)
        {
            if (Disposed || Landscape == null || !Landscape.BlocksSetted) return new Block();

            var block = this.GetBlockAt(X, Y, Z);
            if (block.Type == BlockType.Water)
                return new Block();

            return block;
        }

        public Block GetHighestBlockAt(int X, int Z)
        {
            if (Disposed || X > BoundsX - 1 || Z > BoundsZ - 1 || X < 0 || Z < 0) return new Block();
            if (Landscape == null || !Landscape.BlocksSetted) return new Block();
            for (int y = MaximumHeight; y > MinimumHeight; y--)
            {
                var B = _blocks[X][y][Z];
                if (B.Type != BlockType.Air && B.Type != BlockType.Water)
                    return B;
            }

            return new Block();
        }

        public Block GetLowestBlockAt(int X, int Z)
        {
            if (Disposed || X > BoundsX - 1 || Z > BoundsZ - 1 || X < 0 || Z < 0) return new Block();
            if (Landscape == null || !Landscape.BlocksSetted) return new Block();

            for (var y = MinimumHeight; y < MaximumHeight; y++)
            {
                var block = _blocks[X][y][Z];
                if (block.Type == BlockType.Air && block.Type == BlockType.Water)
                    return _blocks[X][y - 1][Z];
            }

            return new Block();
        }

        public void AddStaticElement(params VertexData[] Data)
        {
            if (Mesh == null) throw new ArgumentException($"Failed to add static element");

            lock (Mesh.Elements)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.Elements.Add(Data[i]);
            }

            this.NeedsRebuilding = true;
        }

        public void RemoveStaticElement(params VertexData[] Data)
        {
            if (Mesh == null) throw new ArgumentException($"Failed to remove static element");

            lock (Mesh.Elements)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.Elements.Remove(Data[i]);
            }

            this.NeedsRebuilding = true;
        }

        public void AddInstance(InstanceData Data, bool AffectedByLod = false)
        {
            if (Mesh == null) throw new ArgumentException($"Failed to add instance data ");

            StaticBuffer.AddInstance(Data, AffectedByLod);
            this.NeedsRebuilding = true;
        }
        
        public void RemoveInstance(InstanceData Data)
        {
            if (Mesh == null) throw new ArgumentException($"Failed to remove instance data ");

            StaticBuffer.RemoveInstance(Data);
            this.NeedsRebuilding = true;
        }

        public void AddCollisionShape(params ICollidable[] Data)
        {
            if (Mesh == null) throw new ArgumentException($"Failed to add collision shape");

            lock (Mesh.CollisionBoxes)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.CollisionBoxes.Add(Data[i]);
            }
        }

        public void RemoveCollisionShape(params ICollidable[] Data)
        {
            if (Mesh == null) throw new ArgumentException($"Failed to remove collision shape");

            lock (Mesh.CollisionBoxes)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.CollisionBoxes.Remove(Data[i]);
            }
        }

        public bool Initialized => Mesh != null;

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

        public ReadOnlyCollection<ICollidable> CollisionShapes
        {
            get
            {
                if (Disposed || !Initialized) return new List<ICollidable>().AsReadOnly();

                lock (Mesh.CollisionBoxes)
                {
                    return Mesh.CollisionBoxes.AsReadOnly();
                }
            }
        }

        public int MaximumHeight => _terrainBuilder.Sparsity?.MaximumHeight ?? BoundsY - 1;
        public int MinimumHeight => _terrainBuilder.Sparsity?.MinimumHeight ?? 0;

        public Block[][] this[int Index] => Disposed || !Landscape.StructuresPlaced || !Landscape.BlocksSetted
            ? _dummyBlocks
            : _blocks[Index];

        public Vector3[] TerrainVertices => _terrainVertices;

        private void ForceDispose()
        {
            Disposed = true;
            _waterDensity?.Clear();
            if (Mesh != null)
            {
                Mesh.Dispose();
                Mesh = null;
            }
            if (Landscape != null)
            {
                Landscape.Dispose();
                Landscape = null;
            }
            _blocks = null;
        }

        private IEnumerator DisposeCoroutine()
        {
            var time = 0f;
            while ( (IsBuilding || IsGenerating) && time < 2.5f)
            {
                time += Time.DeltaTime;
                yield return null;
            }
            this.ForceDispose();
        }

        public void Dispose()
        {
            RoutineManager.StartRoutine(this.DisposeCoroutine);
        }
    }
}