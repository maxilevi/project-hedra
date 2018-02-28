/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 07:37 p.m.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;
using SimplexNoise;
using Region = Hedra.Engine.BiomeSystem.Region;

namespace Hedra.Engine.Generation
{
    public class Chunk : IDisposable
    {
        public static int BaseHeight = 24;
        public static float BlockSize = 4.0f;
        public static int ChunkHeight = 160;
        public static int ChunkWidth = 128;
        private GridCell _nearestVertexCell;

        private readonly VertexData _nearestVertexData = new VertexData();
        private Vector3[] _terrainData;
        public Region Biome;
        public bool Blocked = false;
        public int BoundsX, BoundsY, BoundsZ;

        public bool BuildedCompletely;
        public int BuildedLod = 1;
        public bool BuildedWithStructures;
        public bool Disposed, CanDispose = true;
        public bool HasAo;
        public bool HasWater;
        public bool IsGenerated;
        public BiomeGenerator Landscape;

        public int Lod = 1;
        public ChunkMesh Mesh;
        public bool NeedsRebuilding;
        public bool NeedsToUpdateBounds = true;
        public Chunk[] Neighbours = new Chunk[8]; //Auto manages
        public bool NeverBuilded = true;
        public int OffsetX, OffsetZ;
        public Vector3 Position;

        public Block[][][] Blocks { get; set; } = new Block[(int) (ChunkWidth / BlockSize)][][];

        public List<ICollidable> CollisionShapes
        {
            get
            {
                if (Disposed || !Initialized) return new List<ICollidable>();

                lock (Mesh.CollisionBoxes)
                {
                    return Mesh.CollisionBoxes;
                }
            }
        }

        public bool Initialized => Mesh != null;

        public Chunk[] NeighbourChunks
        {
            get
            {
                var neighbourChunks = new Chunk[8];

                neighbourChunks[0] = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ);
                neighbourChunks[1] = World.GetChunkByOffset(OffsetX, OffsetZ + ChunkWidth);
                neighbourChunks[2] = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ + ChunkWidth);
                neighbourChunks[3] = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ + ChunkWidth);
                neighbourChunks[4] = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ - ChunkWidth);
                neighbourChunks[5] = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ - ChunkWidth);
                neighbourChunks[6] = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ);
                neighbourChunks[7] = World.GetChunkByOffset(OffsetX, OffsetZ - ChunkWidth);

                return neighbourChunks;
            }
        }

        //Neighbours
        public bool NeighboursExist
        {
            get
            {
                Chunk n1 = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ);
                Chunk n2 = World.GetChunkByOffset(OffsetX, OffsetZ + ChunkWidth);
                Chunk n3 = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ + ChunkWidth);
                Chunk n4 = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ);
                Chunk n5 = World.GetChunkByOffset(OffsetX, OffsetZ - ChunkWidth);
                Chunk n6 = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ - ChunkWidth);
                Chunk n7 = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ - ChunkWidth);
                Chunk n8 = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ + ChunkWidth);

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

        public List<VertexData> StaticElements => Mesh.Elements;

        public Chunk()
        {
        }

        public Chunk(int OffsetX, int OffsetZ)
        {
            this.OffsetX = OffsetX;
            this.OffsetZ = OffsetZ;
            Position = new Vector3(OffsetX, 0, OffsetZ);
            Biome = World.BiomePool.GetPredominantBiome(this);
            Landscape = new LandscapeGenerator(this);
        }

        public void Initialize()
        {
            var meshBuffers = new ChunkMeshBuffer[0];

            Mesh = new ChunkMesh(Position, meshBuffers, 1);
        }

        public void Generate()
        {
            IsGenerated = true;
            CanDispose = false;
            Mesh.Position = new Vector3(OffsetX, 0, OffsetZ);

            Landscape.Generate();
        }

        public void BuildMesh()
        {
            Neighbours[0] = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ);
            Neighbours[1] = World.GetChunkByOffset(OffsetX, OffsetZ + ChunkWidth);
            Neighbours[2] = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ + ChunkWidth);
            Neighbours[3] = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ + ChunkWidth);
            Neighbours[4] = World.GetChunkByOffset(OffsetX + ChunkWidth, OffsetZ - ChunkWidth);
            Neighbours[5] = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ - ChunkWidth);
            Neighbours[6] = World.GetChunkByOffset(OffsetX - ChunkWidth, OffsetZ);
            Neighbours[7] = World.GetChunkByOffset(OffsetX, OffsetZ - ChunkWidth);

            this.BuildMesh(Neighbours);
            for (var i = 0; i < Neighbours.Length; i++)
                Neighbours[i] = null;
        }

        public void BuildMesh(Chunk[] Neighbours)
        {
            this.BuildMesh(Neighbours, true);
        }

        public void BuildMesh(Chunk[] Neighbours, bool UsesWorldBuffer)
        {
            try
            {
                CanDispose = false;
                if (Disposed) return;

                bool buildBuffers = UsesWorldBuffer;
                var failed = false;
                var next = false;
                var hasNoise3D = false;
                var hasWater = false;

                if (!IsGenerated) return;

                lock (Blocks)
                {
                    BuildedCompletely = false;
                    NeedsRebuilding = false;

                    bool buildWithAo = GameSettings.BakedAO;
                    HasAo = false;
                    Mesh.Clear();

                    if (!Landscape.BlocksSetted)
                        return;

                    int width = Blocks.Length;
                    int height = Blocks[0].Length;
                    int depth = Blocks[0][0].Length;
                    BoundsX = width;
                    BoundsY = height;
                    BoundsZ = depth;

                    Chunk rightChunk = Neighbours[0];

                    Chunk frontChunk = Neighbours[1];

                    Chunk rightFrontChunk = Neighbours[2];
                    Chunk leftFrontChunk = Neighbours[3];
                    Chunk rightBackChunk = Neighbours[4];
                    Chunk leftBackChunk = Neighbours[5];

                    Chunk leftChunk = Neighbours[6];

                    Chunk backChunk = Neighbours[7];

                    var addonColors = new List<Vector4>();
                    var blockData = new VertexData();
                    var waterData = new VertexData();
                    var cell = new GridCell
                    {
                        P = new Vector3[8],
                        Type = new BlockType[8],
                        Density = new double[8]
                    };

                    for (var y = 0; y < height; y += 1)
                    for (var x = 0; x < width; x += Lod)
                    {
                        next = !next;
                        for (var z = 0; z < depth; z += Lod)
                        {
                            next = !next;

                            if (Blocks[x] == null || Blocks[x][y] == null) continue;
                            if (y == height - 1 || y == 0)
                                continue;

                            var success = true;

                            this.CreateCell(ref cell, x, y, z, rightChunk, frontChunk, rightFrontChunk, leftBackChunk,
                                rightBackChunk, leftFrontChunk, backChunk, leftChunk,
                                width, height, depth, true,
                                Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air,
                                out success);


                            if (!(Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air))
                                if (!MarchingCubes.Usable(0f, cell)) continue;

                            if (Blocks[x][y][z].Noise3D)
                                hasNoise3D = true;

                            if (!success && y < height - 2)
                                failed = true;

                            Vector4 color;

                            if (Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air)
                            {
                                var regionPosition =
                                    new Vector3(cell.P[0].X * BlockSize + OffsetX, 0,
                                        cell.P[0].Z * BlockSize + OffsetZ);

                                RegionColor region = /*Lod == 1
			                            ?*/ World.BiomePool.GetAverageRegionColor(regionPosition);
                                //: World.BiomePool.GetRegion(regionPosition).Colors;

                                color = region.WaterColor;
                                IsoSurfaceCreator.CreateWaterQuad(BlockSize, cell, next,
                                    new Vector3(BlockSize, 1, BlockSize), Lod, color, waterData);
                                hasWater = true;
                            }

                            if (Blocks[x][y][z].Type == BlockType.Water)
                            {
                                if (Blocks[x][y][z].Type == BlockType.Water &&
                                    Blocks[x][y + 1][z].Type == BlockType.Air)
                                    this.CreateCell(ref cell, x, y, z, rightChunk, frontChunk, rightFrontChunk,
                                        leftBackChunk,
                                        rightBackChunk, leftFrontChunk, backChunk, leftChunk,
                                        width, height, depth, true, false, out success);

                                if (MarchingCubes.Usable(0f, cell))
                                {

                                        var regionPosition =
                                        new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);

                                    RegionColor region = /*Lod == 1
                                            ? */World.BiomePool.GetAverageRegionColor(regionPosition);
                                    //: World.BiomePool.GetRegion(regionPosition).Colors;

                                    color = this.GetColor(cell, Blocks[x][y][z].Type, rightChunk, frontChunk,
                                        rightFrontChunk,
                                        leftBackChunk, rightBackChunk, leftFrontChunk, backChunk,
                                        leftChunk, width, height, depth, addonColors, region);

                                    MarchingCubes.Process(0f, cell, color, next, blockData);
                                }
                            }
                            else
                            {
                                var regionPosition =
                                    new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);

                                RegionColor region = /*Lod == 1
			                            ?*/ World.BiomePool.GetAverageRegionColor(regionPosition);
                                //: World.BiomePool.GetRegion(regionPosition).Colors;

                                color = this.GetColor(cell, Blocks[x][y][z].Type, rightChunk, frontChunk,
                                    rightFrontChunk,
                                    leftBackChunk, rightBackChunk, leftFrontChunk, backChunk,
                                    leftChunk, width, height, depth, addonColors, region);

                                MarchingCubes.Process(0f, cell, color, next, blockData);
                            }
                        }
                    }

                    #region MakeMesh

                    BuildedCompletely = !failed;
                    Position = new Vector3(OffsetX, 0, OffsetZ);
                    Mesh.IsGenerated = true;
                    NeverBuilded = false;
                    blockData.Transform(new Vector3(OffsetX, 0, OffsetZ));
                    waterData.Transform(new Vector3(OffsetX, 0, OffsetZ));
                    if (BuildedLod == 1 && Lod != 1) _terrainData = null;
                    if (Lod == 1 && hasNoise3D)
                        if (_terrainData != null)
                            lock (_terrainData)
                            {
                                _terrainData = blockData.Vertices.ToArray();
                            }
                        else _terrainData = blockData.Vertices.ToArray();
                    for (var k = 0; k < blockData.Vertices.Count; k++) //Add empty space for the buffer
                        blockData.ExtraData.Add(0);


                    VertexData staticData = MeshSimplifier.Simplify(blockData, Lod);
                    waterData = MeshSimplifier.Simplify(waterData, Lod);
                    //if(BuildWithAO)
                    //	StaticBuffer.VData = VertexOcclusion.Bake(this.Position, StaticBuffer.VData);


                    for (var i = 0; i < StaticElements.Count; i++)
                    {
                        if (StaticElements[i].ExtraData.Count != StaticElements[i].Vertices.Count)
                        {
                            float extraDataCount = StaticElements[i].Vertices.Count - StaticElements[i].ExtraData.Count;
                            for (var k = 0; k < extraDataCount; k++) //Add empty space for the buffer
                                StaticElements[i].ExtraData.Add(0);
                        }

                        staticData += StaticElements[i];
                    }

                    for (var i = 0; i < Mesh.InstanceElements.Count; i++)
                    {
                        if (Lod != 1
                            && (Mesh.InstanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Grass)
                                || Mesh.InstanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Wheat)))
                            continue;

                        VertexData model = Mesh.InstanceElements[i].MeshCache.Clone();
                        if (Mesh.InstanceElements[i].ColorCache != -Vector4.One &&
                            CacheManager.CachedColors.ContainsKey(Mesh.InstanceElements[i].ColorCache))
                            model.Colors = CacheManager.CachedColors[Mesh.InstanceElements[i].ColorCache].Clone();
                        else
                            model.Colors = Mesh.InstanceElements[i].Colors;

                        float variateFactor = (new Random(OffsetX + OffsetZ + World.Seed + i).NextFloat() * 2f - 1f) *
                                              (24 / 256f);
                        for (var l = 0; l < model.Colors.Count; l++)
                            model.Colors[l] += new Vector4(variateFactor, variateFactor, variateFactor, 0);

                        if (Mesh.InstanceElements[i].ExtraDataCache != -1)
                            model.ExtraData = CacheManager.CachedExtradata[Mesh.InstanceElements[i].ExtraDataCache]
                                .Clone();
                        else
                            model.ExtraData = Mesh.InstanceElements[i].ExtraData;

                        if (Mesh.InstanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Grass) ||
                            Mesh.InstanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Wheat))
                        {
                            Vector3 instancePosition = Mesh.InstanceElements[i].TransMatrix.ExtractTranslation();
                            var grassRng = new Random((int) (instancePosition.X * instancePosition.Z));
                            instancePosition += Vector3.UnitY * grassRng.NextFloat() * .2f;
                            if (!DrawManager.DropShadows.Exists(instancePosition))
                            {
                                var shadow = new DropShadow
                                {
                                    Position = instancePosition,
                                    DepthTest = true,
                                    Rotation = new Matrix3(Mathf.RotationAlign(Vector3.UnitY,
                                        Physics.NormalAtPosition(instancePosition)))
                                };
                                shadow.Scale *= 1.4f;
                                shadow.Position += Vector3.Transform(Vector3.UnitY, shadow.Rotation) *
                                                   grassRng.NextFloat() * .4f;
                                shadow.DeleteWhen = () => BuildedLod != 1 || Disposed;
                            }
                        }

                        model.Transform(Mesh.InstanceElements[i].TransMatrix);
                        //Pack some randomness to the wind values
                        float rng = Utils.Rng.NextFloat();
                        for (var k = 0; k < model.ExtraData.Count; k++)
                        {
                            if (model.ExtraData[k] != 0 && model.ExtraData[k] != -10f)
                                model.ExtraData[k] = Mathf.Pack(new Vector2(model.ExtraData[k], rng), 2048);

                            if (model.ExtraData[k] == -10f)
                                model.ExtraData[k] = -1f;
                        }

                        //StaticBuffer.VData += Model;
                        //Manually add these vertex data's for maximum performance
                        for (var k = 0; k < model.Indices.Count; k++)
                            model.Indices[k] += (uint) staticData.Vertices.Count;
                        staticData.Vertices.AddRange(model.Vertices);
                        staticData.Colors.AddRange(model.Colors);
                        staticData.Normals.AddRange(model.Normals);
                        staticData.Indices.AddRange(model.Indices);
                        staticData.ExtraData.AddRange(model.ExtraData);

                        model.Dispose();
                    }
                    BuildedLod = Lod;
                    HasAo = buildWithAo;
                    BuildedWithStructures = true;
                    HasWater = hasWater;
                    if (buildBuffers)
                    {
                        if (Mesh == null) return;

                        Mesh.OccluMin = new Vector3(staticData.SupportPoint(-Vector3.UnitX).X,
                            staticData.SupportPoint(-Vector3.UnitY).Y, staticData.SupportPoint(-Vector3.UnitZ).Z);
                        Mesh.OccluSize = new Vector3(staticData.SupportPoint(Vector3.UnitX).X,
                                             Math.Max(0, staticData.SupportPoint(Vector3.UnitY).Y),
                                             staticData.SupportPoint(Vector3.UnitZ).Z) - Mesh.OccluMin;

                        for (var i = 0; i < staticData.Colors.Count; i++)
                        {
                            float edata = staticData.ExtraData[i];

                            staticData.Colors[i] = new Vector4(staticData.Colors[i].Xyz, edata);
                        }


                        ThreadManager.ExecuteOnMainThread(delegate
                        {
                            bool result = WorldRenderer.StaticBuffer.Add(new Vector2(OffsetX, OffsetZ), staticData);

                            if (BuildedCompletely)
                                BuildedCompletely = result;

                            if (Mesh != null)
                            {
                                Mesh.IsBuilded = true;
                                Mesh.Enabled = true;
                                Mesh.BuildedOnce = true;
                            }
                            staticData?.Dispose();

                            if (waterData.Vertices.Count == 0)
                                CanDispose = true;
                        });
                        if (waterData.Vertices.Count > 0)
                            ThreadManager.ExecuteOnMainThread(delegate
                            {
                                bool result = WorldRenderer.WaterBuffer.Add(new Vector2(OffsetX, OffsetZ), waterData);

                                if (BuildedCompletely)
                                    BuildedCompletely = result;
                                waterData?.Dispose();

                                CanDispose = true;
                            });
                    }
                    else
                    {
                        
                        ThreadManager.ExecuteOnMainThread(() =>
                            Mesh.BuildFrom(Mesh.MeshBuffers[(int) ChunkBufferTypes.WATER], waterData, false));

                        ThreadManager.ExecuteOnMainThread(() =>
                            Mesh.BuildFrom(Mesh.MeshBuffers[(int) ChunkBufferTypes.STATIC], staticData, true));
                    }

                    #endregion
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(
                    "---- Probably a sync error while building the chunk mesh ---- " + Environment.NewLine + e);
                //AnalyticsManager.SendCrashReport(e.ToString(), CrashState.RUN);
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
                    return Blocks[X][Y][Z];

                return new Block();
            }
            return new Block();
        }

        public Vector3 NearestVertex(Vector3 BlockPosition)
        {
            if (_terrainData == null)
            {
                if (_nearestVertexCell.P == null)
                {
                    _nearestVertexCell.P = new Vector3[8];
                    _nearestVertexCell.Type = new BlockType[8];
                    _nearestVertexCell.Density = new double[8];
                }

                int width = Blocks.Length;
                int height = Blocks[0].Length;
                int depth = Blocks[0][0].Length;

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
                this.CreateCell(ref _nearestVertexCell, x, y, z, rightChunk, frontChunk, rightFrontChunk, leftBackChunk,
                    rightBackChunk, leftFrontChunk, backChunk, leftChunk,
                    width, height, depth, true,
                    Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air, out success);

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
            lock (_terrainData)
            {
                Vector3 nearest0 = Vector3.Zero,
                    nearest1 = Vector3.Zero,
                    nearest2 = Vector3.Zero,
                    nearest3 = Vector3.Zero;
                float dist0 = float.MaxValue;

                for (var i = 0; i < _terrainData.Length; i++)
                {
                    float newDist = (_terrainData[i] - BlockPosition).LengthSquared;
                    if (newDist < dist0)
                    {
                        dist0 = newDist;

                        nearest3 = nearest2;
                        nearest2 = nearest1;
                        nearest1 = nearest0;
                        nearest0 = _terrainData[i];
                    }
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
                if (Landscape != null && Landscape.BlocksSetted)
                    for (y = BoundsY - 1; y > -1; y--)
                    {
                        Block block = Blocks[X][y][Z];
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

            //lock(_blocks){
            if (Landscape != null && Landscape.BlocksSetted)
                for (var y = 0; y < BoundsY; y++)
                {
                    Block B = Blocks[X][y][Z];
                    if (B.Type == BlockType.Air || B.Type == BlockType.Water)
                        return y - 1;
                }
            return 0;
            //}
        }

        public int GetNearestY(int X, int Y, int Z)
        {
            if (Disposed) return 0;
            Y = Math.Max(0, Y);

            if (Landscape != null && Landscape.BlocksSetted)
                for (int y = Y; y < BoundsY - 1; y++)
                {
                    Block block = Blocks[X][y][Z];
                    if (block.Type != BlockType.Air && block.Type != BlockType.Water &&
                        Blocks[X][y + 1][Z].Type == BlockType.Air)
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
                Block B = Blocks[X][y][Z];
                if (B.Type != BlockType.Air && B.Type != BlockType.Water)
                    return Blocks[X][y][Z];
            }
            return new Block();
        }

        public Block GetLowestBlockAt(int X, int Z)
        {
            if (Disposed || X > BoundsX - 1 || Z > BoundsZ - 1 || X < 0 || Z < 0) return new Block();
            if (Landscape == null || !Landscape.BlocksSetted) return new Block();

            for (var y = 0; y < ChunkHeight; y++)
            {
                Block block = Blocks[X][y][Z];
                if (block.Type == BlockType.Air && block.Type == BlockType.Water)
                    return Blocks[X][y - 1][Z];
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

        public void Reset()
        {
            Landscape.StructuresPlaced = false;
            StaticElements.Clear();
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
            if (Blocks != null)
                lock (Blocks)
                {
                    Blocks = null;
                }
        }

        public IEnumerator DisposeCoroutine()
        {
            while (!CanDispose) yield return null;
            this.ForceDispose();
        }

        public void Dispose()
        {
            CoroutineManager.StartCoroutine(this.DisposeCoroutine);
        }

        #region HELPERS

        public Vector4 GetColor(GridCell Cell, BlockType Type, Chunk RightChunk, Chunk FrontChunk,
            Chunk RightFrontChunk, Chunk LeftBackChunk, Chunk RightBackChunk, Chunk LeftFrontChunk, Chunk BackChunk,
            Chunk LeftChunk, int Width, int Height, int Depth, List<Vector4> AddonColors, RegionColor RegionColor)
        {
            Vector3 position = Cell.P[0] / BlockSize;
            Vector4 color = Vector4.Zero;
            float colorCount = 0;
            int x = (int) position.X, y = (int) position.Y, z = (int) position.Z;

            float noise = Noise.Generate((Cell.P[0].X + OffsetX) * .00075f, (Cell.P[0].Z + OffsetZ) * .00075f);
            RegionColor regionColor = RegionColor;

            for (int _x = -Lod * 1; _x < 1 * Lod + 1; _x += Lod)
            for (int _z = -Lod * 1; _z < 1 * Lod + 1; _z += Lod)
            for (int _y = -1; _y < 1 + 1; _y++)
            {
                Block y0 = this.GetNeighbourBlock(x + _x, (int) Mathf.Clamp(y + _y, 0, ChunkHeight - 1), z + _z,
                    RightChunk, FrontChunk, RightFrontChunk,
                    LeftBackChunk, RightBackChunk, LeftFrontChunk, BackChunk,
                    LeftChunk);

                if (y0.Type != BlockType.Water && y0.Type != BlockType.Air && y0.Type != BlockType.Temporal)
                {
                    Vector4 blockColor = Block.GetColor(y0.Type, regionColor);
                    if (Block.GetColor(BlockType.Grass, regionColor) == blockColor)
                    {
                        float clampNoise = (noise + 1) * .5f;
                        float levelSize = 1.0f / regionColor.GrassColors.Length;
                        var nextIndex = (int) Math.Ceiling(clampNoise / levelSize);
                        if (nextIndex == regionColor.GrassColors.Length) nextIndex = 0;

                        Vector4 A = regionColor.GrassColors[(int) Math.Floor(clampNoise / levelSize)],
                            B = regionColor.GrassColors[nextIndex];

                        float delta = clampNoise / levelSize - (float) Math.Floor(clampNoise / levelSize);

                        blockColor = Mathf.Lerp(A, B, delta);
                    }
                    color += new Vector4(blockColor.X, blockColor.Y, blockColor.Z, blockColor.W);
                    colorCount++;
                }
            }
            return new Vector4(color.Xyz / colorCount, 1.0f);
        }

        private readonly float _coefficient = 1 / BlockSize;

        public void CreateCell(ref GridCell Cell, int X, int Y, int Z, Chunk RightChunk, Chunk FrontChunk,
            Chunk RightFrontChunk, Chunk LeftBackChunk, Chunk RightBackChunk, Chunk LeftFrontChunk, Chunk BackChunk,
            Chunk LeftChunk, int Width, int Height, int Depth, bool ExtraData, bool WaterCell, out bool Success)
        {
            Success = true;

            this.BuildCell(ref Cell, X, Y, Z, WaterCell);

            if (!WaterCell)
                for (var i = 0; i < Cell.Type.Length; i++)
                {
                    var pos = new Vector3(Cell.P[i].X * _coefficient, Cell.P[i].Y * _coefficient,
                        Cell.P[i].Z * _coefficient); // LOD is 1
                    Block block = this.GetNeighbourBlock((int) pos.X, (int) pos.Y, (int) pos.Z,
                        RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                        BackChunk, LeftChunk);

                    Cell.Type[i] = block.Type;
                    Cell.Density[i] = block.Density;

                    if (block.Type == BlockType.Water && (int) pos.Y > BiomePool.SeaLevel)
                        Cell.Density[i] = BiomePool.DecodeWater(block.Density);
                }
            if (WaterCell && Y > BiomePool.SeaLevel)
            {
//Only rivers
                var cz = new GridCell();
                cz.P = new Vector3[4];
                cz.P[0] = new Vector3(X * BlockSize, Y * BlockSize, Z * BlockSize);
                cz.P[1] = new Vector3(BlockSize * Lod + cz.P[0].X, cz.P[0].Y, cz.P[0].Z);
                cz.P[2] = new Vector3(BlockSize * Lod + cz.P[0].X, cz.P[0].Y, BlockSize * Lod + cz.P[0].Z);
                cz.P[3] = new Vector3(cz.P[0].X, cz.P[0].Y, BlockSize * Lod + cz.P[0].Z);

                for (var i = 0; i < 4; i++)
                {
                    var pos = new Vector3(cz.P[i].X * _coefficient, cz.P[i].Y * _coefficient,
                        cz.P[i].Z * _coefficient); // LOD is 1
                    Block waterBlock = this.GetNeighbourBlock((int) pos.X, (int) pos.Y, (int) pos.Z,
                        RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                        BackChunk, LeftChunk);

                    if (waterBlock.Type != BlockType.Water)
                    {
                        for (int k = (int) pos.Y - 3; k < BoundsY; k++)
                        {
                            waterBlock = this.GetNeighbourBlock((int) pos.X, k, (int) pos.Z,
                                RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                                BackChunk, LeftChunk);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }

                        for (int k = (int) pos.Y - 3; k < BoundsY; k++)
                        for (int kx = -2; kx < 3; kx++)
                        for (int kz = -2; kz < 3; kz++)
                        {
                            waterBlock = this.GetNeighbourBlock((int) pos.X + kx, k, (int) pos.Z + kz,
                                RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                                BackChunk, LeftChunk);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }
                    }
                    WATER_BREAK:

                    var scaleFactor = 65530.0;
                    double cp = 256.0 * 256.0;

                    double dy = Math.Floor(waterBlock.Density / cp);
                    var yk = (float) (dy / scaleFactor);
                    yk *= 10.0f;


                    for (int k = Y; k > -1; k--)
                    {
                        Block block = this.GetNeighbourBlock((int) pos.X, k, (int) pos.Z,
                            RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                            BackChunk, LeftChunk);
                        if (block.Type == BlockType.Seafloor)
                        {
                            Cell.Density[i] = 0;
                            Cell.P[i] = new Vector3(Cell.P[i].X, yk, Cell.P[i].Z);
                            break;
                        }
                    }
                }
            }
            else if (WaterCell)
            {
                this.UpdateCell(ref Cell, X, Y, Z, RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk,
                    RightBackChunk, LeftFrontChunk, BackChunk, LeftChunk, Width, Height, Depth, ExtraData, WaterCell);
            }

            for (var i = 0; i < Cell.Type.Length; i++)
                if (Cell.Type[i] == BlockType.Temporal && Y < ChunkHeight - 2)
                {
                    Success = false;
                    Cell.Type[i] = BlockType.Air;
                }
        }

        private void UpdateCell(ref GridCell Cell, int X, int Y, int Z, Chunk RightChunk, Chunk FrontChunk,
            Chunk RightFrontChunk, Chunk LeftBackChunk, Chunk RightBackChunk, Chunk LeftFrontChunk, Chunk BackChunk,
            Chunk LeftChunk, int Width, int Height, int Depth, bool ExtraData, bool NormalCell)
        {
            int lod = Lod;

            #region NormalCell

            if (NormalCell)
            {
                if (ExtraData)
                {
                    if (X <= lod - 1)
                    {
                        if (LeftChunk != null && !LeftChunk.Disposed && LeftChunk.IsGenerated &&
                            LeftChunk.Mesh.IsBuilded)
                        {
                            Cell.Type[4] = LeftChunk.GetBlockAt(Width - lod + X, Y, Z).Type;
                            Cell.Density[4] = LeftChunk.GetBlockAt(Width - lod + X, Y, Z).Density;
                        }
                    }
                    else
                    {
                        Cell.Type[4] = Blocks[X - lod][Y][Z].Type;
                        Cell.Density[4] = Blocks[X - lod][Y][Z].Density;
                    }

                    //5

                    if (Z <= lod - 1)
                    {
                        if (BackChunk != null && !BackChunk.Disposed && BackChunk.IsGenerated &&
                            BackChunk.Mesh.IsBuilded)
                        {
                            Cell.Type[5] = BackChunk.GetBlockAt(X, Y, Depth - lod + Z).Type;
                            Cell.Density[5] = BackChunk.GetBlockAt(X, Y, Depth - lod + Z).Density;
                        }
                    }
                    else
                    {
                        Cell.Type[5] = Blocks[X][Y][Z - lod].Type;
                        Cell.Density[5] = Blocks[X][Y][Z - lod].Density;
                    }
                }

                //---Special Cases---
                var bX = false;
                var bZ = false;
                //2
                if (X >= Width - lod) bX = true;
                if (Z >= Depth - lod) bZ = true;
                if (bX && bZ && RightFrontChunk != null && !RightFrontChunk.Disposed && RightFrontChunk.IsGenerated &&
                    RightFrontChunk.Mesh.IsBuilded)
                {
                    Cell.Density[2] = RightFrontChunk.GetBlockAt(0, Y, 0).Density;
                    Cell.Type[2] = RightFrontChunk.GetBlockAt(0, Y, 0).Type;
                }
                if (bZ && !bX && FrontChunk != null && !FrontChunk.Disposed && FrontChunk.IsGenerated &&
                    FrontChunk.Mesh.IsBuilded)
                {
                    Cell.Density[2] = FrontChunk.GetBlockAt(X + lod, Y, 0).Density;
                    Cell.Type[2] = FrontChunk.GetBlockAt(X + lod, Y, 0).Type;
                }
                if (bX && !bZ && RightChunk != null && !RightChunk.Disposed && RightChunk.IsGenerated &&
                    RightChunk.Mesh.IsBuilded)
                {
                    Cell.Density[2] = RightChunk.GetBlockAt(0, Y, Z + lod).Density;
                    Cell.Type[2] = RightChunk.GetBlockAt(0, Y, Z + lod).Type;
                }
                if (!bX && !bZ)
                {
                    Cell.Density[2] = Blocks[X + lod][Y][Z + lod].Density;
                    Cell.Type[2] = Blocks[X + lod][Y][Z + lod].Type;
                }

                if (ExtraData)
                {
                    //x-1z-1
                    var nX = false;
                    var nZ = false;
                    //2
                    if (X <= lod - 1) nX = true;
                    if (Z <= lod - 1) nZ = true;

                    if (nX && nZ && LeftBackChunk != null && !LeftBackChunk.Disposed && LeftBackChunk.IsGenerated &&
                        LeftBackChunk.Mesh.IsBuilded)
                    {
                        Cell.Type[6] = LeftBackChunk.GetBlockAt(Width - lod + X, Y, Depth - lod + Z).Type;
                        Cell.Density[6] = LeftBackChunk.GetBlockAt(Width - lod + X, Y, Depth - lod + Z).Density;
                    }
                    if (nZ && !nX && BackChunk != null && !BackChunk.Disposed && BackChunk.IsGenerated &&
                        BackChunk.Mesh.IsBuilded)
                    {
                        Cell.Type[6] = BackChunk.GetBlockAt(X - lod, Y, Depth - lod + Z).Type;
                        Cell.Density[6] = BackChunk.GetBlockAt(X - lod, Y, Depth - lod + Z).Density;
                    }
                    if (nX && !nZ && LeftChunk != null && !LeftChunk.Disposed && LeftChunk.IsGenerated &&
                        LeftChunk.Mesh.IsBuilded)
                    {
                        Cell.Type[6] = LeftChunk.GetBlockAt(Width - lod + X, Y, Z - lod).Type;
                        Cell.Density[6] = LeftChunk.GetBlockAt(Width - lod + X, Y, Z - lod).Density;
                    }
                    if (!nX && !nZ)
                    {
                        Cell.Type[6] = Blocks[X - lod][Y][Z - lod].Type;
                        Cell.Density[6] = Blocks[X - lod][Y][Z - lod].Density;
                    }

                    //x-1 z+1

                    if (nX && bZ && LeftFrontChunk != null && !LeftFrontChunk.Disposed && LeftFrontChunk.IsGenerated &&
                        LeftFrontChunk.Mesh.IsBuilded)
                        Cell.Type[0] = LeftFrontChunk.GetBlockAt(Width - lod + X, Y, 0).Type;
                    if (!nX && bZ && FrontChunk != null && !FrontChunk.Disposed && FrontChunk.IsGenerated &&
                        FrontChunk.Mesh.IsBuilded) Cell.Type[0] = FrontChunk.GetBlockAt(X - lod, Y, 0).Type;
                    if (nX && !bZ && LeftChunk != null && !LeftChunk.Disposed && LeftChunk.IsGenerated &&
                        LeftChunk.Mesh.IsBuilded) Cell.Type[0] = LeftChunk.GetBlockAt(Width - lod + X, Y, Z + lod).Type;
                    if (!nX && !bZ) Cell.Type[0] = Blocks[X - lod][Y][Z + lod].Type;

                    //x+1 z-1

                    if (bX && nZ && RightBackChunk != null && !RightBackChunk.Disposed && RightBackChunk.IsGenerated &&
                        RightBackChunk.Mesh.IsBuilded)
                    {
                        Cell.Type[7] = RightBackChunk.GetBlockAt(0, Y, Depth - lod + Z).Type;
                        Cell.Density[7] = RightBackChunk.GetBlockAt(0, Y, Depth - lod + Z).Density;
                    }
                    if (!bX && nZ && BackChunk != null && !BackChunk.Disposed && BackChunk.IsGenerated &&
                        BackChunk.Mesh.IsBuilded)
                    {
                        Cell.Type[7] = BackChunk.GetBlockAt(X + lod, Y, Depth - lod + Z).Type;
                        Cell.Density[7] = BackChunk.GetBlockAt(X + lod, Y, Depth - lod + Z).Density;
                    }
                    if (bX && !nZ && RightChunk != null && !RightChunk.Disposed && RightChunk.IsGenerated &&
                        RightChunk.Mesh.IsBuilded)
                    {
                        Cell.Type[7] = RightChunk.GetBlockAt(0, Y, Z - lod).Type;
                        Cell.Density[7] = RightChunk.GetBlockAt(0, Y, Z - lod).Density;
                    }
                    if (!bX && !nZ)
                    {
                        Cell.Type[7] = Blocks[X + lod][Y][Z - lod].Type;
                        Cell.Density[7] = Blocks[X + lod][Y][Z - lod].Density;
                    }
                }
            }

            #endregion
        }

        private void BuildCell(ref GridCell Cell, int X, int Y, int Z, bool WaterCell)
        {
            int lod = Lod;
            float blockSizeLod = BlockSize * lod;
            if (WaterCell)
            {
                Cell.P[0] = new Vector3(X, Y, Z); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE);
                Cell.P[1] = new Vector3(X + blockSizeLod, Y, Z); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE); 
                Cell.P[2] = new Vector3(X + blockSizeLod, Y,
                    Z + blockSizeLod); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE); 
                Cell.P[3] = new Vector3(X, Y, Z + blockSizeLod); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE); 
                Cell.P[4] = new Vector3(X, Y + BlockSize, Z); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE);
                Cell.P[5] = new Vector3(X + blockSizeLod, Y + BlockSize, Z); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE);
                Cell.P[6] = new Vector3(X + blockSizeLod, Y + BlockSize,
                    Z + blockSizeLod); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE);
                Cell.P[7] = new Vector3(X, Y + BlockSize, Z + blockSizeLod); // * new Vector3(BLOCK_SIZE,1,BLOCK_SIZE);
            }
            else
            {
                Cell.P[0] = new Vector3(X * BlockSize, Y * BlockSize, Z * BlockSize);
                Cell.P[1] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[2] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[3] = new Vector3(Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[4] = new Vector3(Cell.P[0].X, BlockSize + Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[5] = new Vector3(blockSizeLod + Cell.P[0].X, BlockSize + Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[6] = new Vector3(blockSizeLod + Cell.P[0].X, BlockSize + Cell.P[0].Y,
                    blockSizeLod + Cell.P[0].Z);
                Cell.P[7] = new Vector3(Cell.P[0].X, BlockSize + Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
            }
        }

        public bool IsBlockUsable(int X, int Y, int Z, Chunk RightChunk, Chunk FrontChunk, Chunk RightFrontChunk,
            Chunk LeftBackChunk, Chunk RightBackChunk, Chunk LeftFrontChunk, Chunk BackChunk, Chunk LeftChunk,
            int Width, int Height, int Depth, Dictionary<Vector3, bool> Cache)
        {
            var isUsable = false;
            var result = false;
            Vector3 position;
            Block y;
            for (int _x = -1; _x < 2 && !isUsable; _x++)
            for (int _z = -1; _z < 2 && !isUsable; _z++)
            for (int _i = -1; _i < 2 && !isUsable; _i++)
            {
                position = new Vector3(X + _x, _i + Y, Z + _z);

                if (!Cache.ContainsKey(position))
                {
                    y = this.GetNeighbourBlock((int) position.X, (int) position.Y, (int) position.Z, RightChunk,
                        FrontChunk, RightFrontChunk,
                        LeftBackChunk, RightBackChunk, LeftFrontChunk, BackChunk,
                        LeftChunk);

                    result = y.Type != BlockType.Water && y.Type != BlockType.Air;

                    Cache.Add(position, result);
                }
                else
                {
                    result = Cache[position];
                }

                if (result)
                {
                    isUsable = true;
                    break;
                }
            }
            return isUsable;
        }

        public Block GetNeighbourBlock(int X, int Y, int Z, Chunk RightChunk, Chunk FrontChunk, Chunk RightFrontChunk,
            Chunk LeftBackChunk, Chunk RightBackChunk, Chunk LeftFrontChunk, Chunk BackChunk, Chunk LeftChunk)
        {
            bool bX = X >= BoundsX;
            bool bZ = Z >= BoundsZ;

            bool nX = X <= -1;
            bool nZ = Z <= -1;

            if (!bX && !bZ && !nX && !nZ)
                return Blocks[X][Y][Z];

            if (bZ && !bX && FrontChunk != null && !FrontChunk.Disposed && FrontChunk.IsGenerated &&
                FrontChunk.Landscape.StructuresPlaced)
                return FrontChunk.GetBlockAt(X, Y, Z - BoundsZ);

            if (bX && !bZ && RightChunk != null && !RightChunk.Disposed && RightChunk.IsGenerated &&
                RightChunk.Landscape.StructuresPlaced)
                return RightChunk.GetBlockAt(X - BoundsX, Y, Z);

            if (nZ && !nX && BackChunk != null && !BackChunk.Disposed && BackChunk.IsGenerated &&
                BackChunk.Landscape.StructuresPlaced)
                return BackChunk.GetBlockAt(X, Y, Z + BoundsZ);

            if (nX && !nZ && LeftChunk != null && !LeftChunk.Disposed && LeftChunk.IsGenerated &&
                LeftChunk.Landscape.StructuresPlaced)
                return LeftChunk.GetBlockAt(X + BoundsX, Y, Z);

            if (nX && nZ && LeftBackChunk != null && !LeftBackChunk.Disposed && LeftBackChunk.IsGenerated &&
                LeftBackChunk.Landscape.StructuresPlaced)
                return LeftBackChunk.GetBlockAt(X + BoundsX, Y, Z + BoundsZ);

            if (bX && bZ && RightFrontChunk != null && !RightFrontChunk.Disposed && RightFrontChunk.IsGenerated &&
                RightFrontChunk.Landscape.StructuresPlaced)
                return RightFrontChunk.GetBlockAt(X - BoundsX, Y, Z - BoundsZ);


            return new Block(BlockType.Temporal);
        }

        #endregion
    }
}