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
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;
using Region = Hedra.Engine.BiomeSystem.Region;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class Chunk : IDisposable
    {
        public static int BaseHeight { get; } = 24;
        public static float BlockSize { get; } = 4.0f;
        public static int Height { get; } = 160;
        public static int Width { get; } = 128;
        public Block[][][] Voxels { get; private set; }
        public Region Biome { get; }
        public bool Blocked { get; set; }
        public int BoundsX { get; private set; }
        public int BoundsY { get; private set; }
        public int BoundsZ { get; private set; }
        public bool BuildedCompletely { get; set; }
        public int BuildedLod { get; set; } = 1;
        public bool BuildedWithStructures { get; set; }
        public bool Disposed { get; set; }
        public bool HasWater { get; set; }
        public bool IsGenerated { get; set; }
        public BiomeGenerator Landscape { get; set; }
        public int Lod { get; set; } = 1;
        public ChunkMesh Mesh { get; set; }
        public bool NeedsRebuilding { get; set; }
        public bool NeedsToUpdateBounds { get; set; } = true;
        public bool NeverBuilded { get; set; } = true;
        public int OffsetX { get; }
        public int OffsetZ { get; }
        public Vector3 Position { get; private set; }

        private readonly VertexData _nearestVertexData;
        private readonly Chunk[] _neighbours;
        private readonly ChunkTerrainMeshBuilder _terrainBuilder;
        private readonly ChunkStructuresMeshBuilder _structuresBuilder;
        private readonly object _terrainVerticesLock;
        private bool _canDispose = true;
        private GridCell _nearestVertexCell;
        private Vector3[] _terrainVertices;

        public Chunk()
        {
            Voxels = new Block[(int)(Width / BlockSize)][][];
            _nearestVertexData = new VertexData();
            _terrainBuilder = new ChunkTerrainMeshBuilder(this);
            _structuresBuilder = new ChunkStructuresMeshBuilder(this);
            _terrainVertices = new Vector3[0];
            _neighbours = new Chunk[8];
            _terrainVerticesLock = new object();
        }

        public Chunk(int OffsetX, int OffsetZ) : this()
        {
            this.OffsetX = OffsetX;
            this.OffsetZ = OffsetZ;
            Position = new Vector3(OffsetX, 0, OffsetZ);
            Biome = World.BiomePool.GetPredominantBiome(this);
            Landscape = new LandscapeGenerator(this);
        }

        public void Initialize()
        {
            Mesh = new ChunkMesh(Position, new ChunkMeshBuffer[0]);
        }

        public void Generate()
        {
            IsGenerated = true;
            _canDispose = false;
            Mesh.Position = new Vector3(OffsetX, 0, OffsetZ);

            Landscape.Generate();
        }

        public void BuildMesh()
        {
            _neighbours[0] = World.GetChunkByOffset(OffsetX + Width, OffsetZ);
            _neighbours[1] = World.GetChunkByOffset(OffsetX, OffsetZ + Width);
            _neighbours[2] = World.GetChunkByOffset(OffsetX + Width, OffsetZ + Width);
            _neighbours[3] = World.GetChunkByOffset(OffsetX - Width, OffsetZ + Width);
            _neighbours[4] = World.GetChunkByOffset(OffsetX + Width, OffsetZ - Width);
            _neighbours[5] = World.GetChunkByOffset(OffsetX - Width, OffsetZ - Width);
            _neighbours[6] = World.GetChunkByOffset(OffsetX - Width, OffsetZ);
            _neighbours[7] = World.GetChunkByOffset(OffsetX, OffsetZ - Width);

            this.BuildMesh(_neighbours);
            for (var i = 0; i < _neighbours.Length; i++)
                _neighbours[i] = null;
        }

        public void BuildMesh(Chunk[] Neighbours)
        {
            if (Disposed || !IsGenerated || !Landscape.BlocksSetted) return;

            this.CalculateBounds();
            this.PrepareForBuilding();
            var output = this.CreateTerrainMesh(Neighbours);

            if (output == null) return;
            this.SetChunkStatus(output);
            output = this.AddStructuresMeshes(output);

            if (output == null) return;
            this.UploadMesh(output, true);
            this.FinishUpload(output);
        }

        public ChunkMeshBuildOutput CreateTerrainMesh(Chunk[] Neighbours)
        {
            return this.CreateTerrainMesh(Neighbours, this.Lod);
        }

        public ChunkMeshBuildOutput CreateTerrainMesh(Chunk[] Neighbours, int LevelOfDetail)
        {
            return _terrainBuilder.CreateTerrainMesh(Neighbours, LevelOfDetail);
        }

        public ChunkMeshBuildOutput AddStructuresMeshes(ChunkMeshBuildOutput Input)
        {
            return this.AddStructuresMeshes(Input, this.Lod);
        }

        public ChunkMeshBuildOutput AddStructuresMeshes(ChunkMeshBuildOutput Input, int LevelOfDetail)
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
                _terrainVertices = Input.StaticData.Vertices.ToArray();
            }
        }

        public void CalculateBounds()
        {
            BoundsX = Voxels.Length;
            BoundsY = Voxels[0].Length;
            BoundsZ = Voxels[0][0].Length;
        }

        private void SetChunkStatus(ChunkMeshBuildOutput Input)
        {
            this.BuildedCompletely = !Input.Failed;
            this.Position = new Vector3(OffsetX, 0, OffsetZ);
            this.Mesh.IsGenerated = true;
            this.NeverBuilded = false;
        }

        private void PrepareForBuilding()
        {
            this._canDispose = false;
            this.BuildedCompletely = false;
            this.NeedsRebuilding = false;
            this.Mesh.Clear();
        }

        private void FinishUpload(ChunkMeshBuildOutput Input)
        {
            this.BuildedLod = Lod;
            this.BuildedWithStructures = true;
        }

        private void UploadMesh(ChunkMeshBuildOutput Input, bool BuildBuffers)
        {
            if (BuildBuffers)
            {
                if (Mesh == null) return;

                try
                {
                    Mesh.OccluMin = new Vector3(Input.StaticData.SupportPoint(-Vector3.UnitX).X,
                        Input.StaticData.SupportPoint(-Vector3.UnitY).Y,
                        Input.StaticData.SupportPoint(-Vector3.UnitZ).Z);
                    Mesh.OccluSize = new Vector3(Input.StaticData.SupportPoint(Vector3.UnitX).X,
                                         Math.Max(0, Input.StaticData.SupportPoint(Vector3.UnitY).Y),
                                         Input.StaticData.SupportPoint(Vector3.UnitZ).Z) - Mesh.OccluMin;

                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log.WriteLine("Sync error.");
                    return;
                }

                for (var i = 0; i < Input.StaticData.ExtraData.Count; i++)
                {
                    float edata = Input.StaticData.ExtraData[i];

                    Input.StaticData.Colors[i] = new Vector4(Input.StaticData.Colors[i].Xyz, edata);
                }


                ThreadManager.ExecuteOnMainThread(delegate
                {
                    bool result = WorldRenderer.StaticBuffer.Add(new Vector2(OffsetX, OffsetZ), Input.StaticData);

                    if (BuildedCompletely)
                        BuildedCompletely = result;

                    if (Mesh != null)
                    {
                        Mesh.IsBuilded = true;
                        Mesh.Enabled = true;
                        Mesh.BuildedOnce = true;
                    }
                    Input.StaticData?.Dispose();

                    if (Input.WaterData.Vertices.Count == 0)
                        _canDispose = true;
                });
                if (Input.WaterData.Vertices.Count > 0)
                    ThreadManager.ExecuteOnMainThread(delegate
                    {
                        bool result = WorldRenderer.WaterBuffer.Add(new Vector2(OffsetX, OffsetZ), Input.WaterData);

                        if (BuildedCompletely)
                            BuildedCompletely = result;
                        Input.WaterData?.Dispose();

                        _canDispose = true;
                    });
            }
            else
            {

                ThreadManager.ExecuteOnMainThread(() =>
                    Mesh.BuildFrom(Mesh.MeshBuffers[(int)ChunkBufferTypes.WATER], Input.WaterData, false));

                ThreadManager.ExecuteOnMainThread(() =>
                    Mesh.BuildFrom(Mesh.MeshBuffers[(int)ChunkBufferTypes.STATIC], Input.StaticData, true));
            }
        }

        public Block GetBlockAt(Vector3 V)
        {
            return this.GetBlockAt((int) V.X, (int) V.Y, (int) V.Z);
        }

        public Block GetBlockAt(int X, int Y, int Z)
        {
            if (Disposed) return new Block();

            if (Landscape != null && Landscape.BlocksSetted)
            {
                if (X >= 0 && Y >= 0 && Z >= 0 && X <= BoundsX - 1 && Y <= BoundsY - 1 && Z <= BoundsZ - 1)
                    return Voxels[X][Y][Z];

                return new Block();
            }
            return new Block();
        }

        public Vector3 NearestVertex(Vector3 BlockPosition)
        {
            if (_terrainVertices == null)
            {
                if (_nearestVertexCell.P == null)
                {
                    _nearestVertexCell.P = new Vector3[8];
                    _nearestVertexCell.Type = new BlockType[8];
                    _nearestVertexCell.Density = new double[8];
                }

                int width = Voxels.Length;
                int height = Voxels[0].Length;
                int depth = Voxels[0][0].Length;

                Chunk[] neighbours = NeighbourChunks;
                Chunk rightChunk = neighbours[0];
                Chunk frontChunk = neighbours[1];
                Chunk rightFrontChunk = neighbours[2];
                Chunk leftFrontChunk = neighbours[3];
                Chunk rightBackChunk = neighbours[4];
                Chunk leftBackChunk = neighbours[5];
                Chunk leftChunk = neighbours[6];
                Chunk backChunk = neighbours[7];
                //Check for air blocks
                Vector3 blockSpace = World.ToBlockSpace(BlockPosition);
                int x = (int) blockSpace.X, z = (int) blockSpace.Z, y = (int) blockSpace.Y;

                var success = false;
                _terrainBuilder.Helper.CreateCell(ref _nearestVertexCell, x, y, z, rightChunk, frontChunk, rightFrontChunk, leftBackChunk,
                    rightBackChunk, leftFrontChunk, backChunk, leftChunk,
                    width, height, depth, true,
                    Voxels[x][y][z].Type == BlockType.Water && Voxels[x][y + 1][z].Type == BlockType.Air, Lod, out success);

                _nearestVertexData.Vertices.Clear();
                _nearestVertexData.Colors.Clear();
                _nearestVertexData.Normals.Clear();
                _nearestVertexData.Indices.Clear();
                MarchingCubes.Process(0f, _nearestVertexCell, Vector4.One, (x + z) % 2 == 0, _nearestVertexData);

                if (_nearestVertexData.Vertices.Count != 0)
                {
                    Vector3 average = Vector3.Zero;
                    for (var i = 0; i < _nearestVertexData.Vertices.Count; i++)
                        average += _nearestVertexData.Vertices[i];
                    average /= _nearestVertexData.Vertices.Count;

                    return average - Vector3.UnitY * .25f;
                }
                return Vector3.Zero;
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
                    float newDist = (_terrainVertices[i] - BlockPosition).LengthSquared;
                    if (!(newDist < dist0)) continue;
                    dist0 = newDist;

                    nearest3 = nearest2;
                    nearest2 = nearest1;
                    nearest1 = nearest0;
                    nearest0 = _terrainVertices[i];
                }
                return nearest0 + 1 * Vector3.UnitY;
            }
        }

        public int GetHighestY(int X, int Z)
        {
            if (Disposed) return 0;
            var y = 0;
            try
            {
                if (Landscape == null || !Landscape.BlocksSetted) return 0;
                for (y = BoundsY - 1; y > -1; y--)
                {
                    Block block = Voxels[X][y][Z];
                    if (block.Type != BlockType.Air && block.Type != BlockType.Water)
                        return y;
                }
                return 0;
            }
            catch (IndexOutOfRangeException e)
            {
                Log.WriteLine("{ " + y + "/" + BoundsY + " | " + X + "|" + Z + " }" + Environment.NewLine + e);
                return 0;
            }
        }

        public int GetLowestY(int X, int Z)
        {
            if (Disposed || X < 0 || Z < 0) return 0;
            if (Landscape == null || !Landscape.BlocksSetted) return 0;
            for (var y = 0; y < BoundsY; y++)
            {
                var block = Voxels[X][y][Z];
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
                for (int y = Y; y < BoundsY - 1; y++)
                {
                    Block block = Voxels[X][y][Z];
                    if (block.Type != BlockType.Air && block.Type != BlockType.Water &&
                        Voxels[X][y + 1][Z].Type == BlockType.Air)
                        return y;
                }
            return Y;
        }

        public Block GetNearestBlockAt(int X, int Y, int Z)
        {
            if (Disposed) return new Block();
            if (Landscape == null || !Landscape.BlocksSetted) return new Block();

            Block block = this.GetBlockAt(X, Y, Z);
            if (block.Type == BlockType.Water && Y > BiomePool.SeaLevel)
                return new Block();

            return block;
        }

        public Block GetHighestBlockAt(int X, int Z)
        {
            if (Disposed || X > BoundsX - 1 || Z > BoundsZ - 1 || X < 0 || Z < 0) return new Block();
            if (Landscape == null || !Landscape.BlocksSetted) return new Block();

            for (int y = BoundsY - 1; y > -1; y--)
            {
                Block B = Voxels[X][y][Z];
                if (B.Type != BlockType.Air && B.Type != BlockType.Water)
                    return Voxels[X][y][Z];
            }
            return new Block();
        }

        public Block GetLowestBlockAt(int X, int Z)
        {
            if (Disposed || X > BoundsX - 1 || Z > BoundsZ - 1 || X < 0 || Z < 0) return new Block();
            if (Landscape == null || !Landscape.BlocksSetted) return new Block();

            for (var y = 0; y < Height; y++)
            {
                Block block = Voxels[X][y][Z];
                if (block.Type == BlockType.Air && block.Type == BlockType.Water)
                    return Voxels[X][y - 1][Z];
            }
            return new Block();
        }

        public bool IsActiveBlock(int X, int Y, int Z)
        {
            Block b = this.GetBlockAt(X, Y, Z);
            return b.Type != BlockType.Air && (b.Density > 0 || b.Density == -1) ? true : false;
        }

        public void AddStaticElement(params VertexData[] Data)
        {
            if (Mesh == null) return;

            lock (Mesh.Elements)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.Elements.Add(Data[i]);
            }
        }

        public void RemoveStaticElement(params VertexData[] Data)
        {
            if (Mesh == null) return;

            lock (Mesh.Elements)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.Elements.Remove(Data[i]);
            }
        }


        public void AddCollisionShape(params ICollidable[] Data)
        {
            if (Mesh == null) return;

            lock (Mesh.CollisionBoxes)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.CollisionBoxes.Add(Data[i]);
            }
        }

        public void RemoveCollisionShape(params ICollidable[] Data)
        {
            if (Mesh == null) return;

            lock (Mesh.CollisionBoxes)
            {
                for (var i = 0; i < Data.Length; i++)
                    Mesh.CollisionBoxes.Remove(Data[i]);
            }
        }

        public bool Initialized => Mesh != null;

        public Chunk[] NeighbourChunks
        {
            get
            {
                var neighbourChunks = new Chunk[8];

                neighbourChunks[0] = World.GetChunkByOffset(OffsetX + Width, OffsetZ);
                neighbourChunks[1] = World.GetChunkByOffset(OffsetX, OffsetZ + Width);
                neighbourChunks[2] = World.GetChunkByOffset(OffsetX + Width, OffsetZ + Width);
                neighbourChunks[3] = World.GetChunkByOffset(OffsetX - Width, OffsetZ + Width);
                neighbourChunks[4] = World.GetChunkByOffset(OffsetX + Width, OffsetZ - Width);
                neighbourChunks[5] = World.GetChunkByOffset(OffsetX - Width, OffsetZ - Width);
                neighbourChunks[6] = World.GetChunkByOffset(OffsetX - Width, OffsetZ);
                neighbourChunks[7] = World.GetChunkByOffset(OffsetX, OffsetZ - Width);

                return neighbourChunks;
            }
        }

        public bool NeighboursExist
        {
            get
            {
                Chunk n1 = World.GetChunkByOffset(OffsetX + Width, OffsetZ);
                Chunk n2 = World.GetChunkByOffset(OffsetX, OffsetZ + Width);
                Chunk n3 = World.GetChunkByOffset(OffsetX + Width, OffsetZ + Width);
                Chunk n4 = World.GetChunkByOffset(OffsetX - Width, OffsetZ);
                Chunk n5 = World.GetChunkByOffset(OffsetX, OffsetZ - Width);
                Chunk n6 = World.GetChunkByOffset(OffsetX - Width, OffsetZ - Width);
                Chunk n7 = World.GetChunkByOffset(OffsetX + Width, OffsetZ - Width);
                Chunk n8 = World.GetChunkByOffset(OffsetX - Width, OffsetZ + Width);

                if (n1 == null || n2 == null || n3 == null || n4 == null || n5 == null || n6 == null || n7 == null ||
                    n8 == null
                    || !n1.IsGenerated || !n1.Landscape.StructuresPlaced || !n2.IsGenerated ||
                    !n2.Landscape.StructuresPlaced
                    || !n3.IsGenerated || !n3.Landscape.StructuresPlaced || !n4.IsGenerated ||
                    !n4.Landscape.StructuresPlaced
                    || !n5.IsGenerated || !n5.Landscape.StructuresPlaced || !n6.IsGenerated ||
                    !n6.Landscape.StructuresPlaced
                    || !n7.IsGenerated || !n7.Landscape.StructuresPlaced || !n8.IsGenerated ||
                    !n8.Landscape.StructuresPlaced)
                    return false;
                return true;
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

        public void Reset()
        {
            Landscape.StructuresPlaced = false;
            Mesh.Elements.Clear();
            Mesh.InstanceElements.Clear();
        }

        public void ForceDispose()
        {
            Disposed = true;
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
            if (Voxels == null) return;
            lock (Voxels)
            {
                Voxels = null;
            }
        }

        public IEnumerator DisposeCoroutine()
        {
            while (!_canDispose) yield return null;
            this.ForceDispose();
        }

        public void Dispose()
        {
            CoroutineManager.StartCoroutine(this.DisposeCoroutine);
        }
    }
}