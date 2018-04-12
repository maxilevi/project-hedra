using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkTerrainMeshBuilderHelper
    {
        private readonly Chunk _parent;
        private readonly float _coefficient = 1 / BlockSize;

        public ChunkTerrainMeshBuilderHelper(Chunk Parent)
        {
            _parent = Parent;
        }

        private int Lod => _parent.Lod;
        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private int BoundsX => _parent.BoundsX;
        private int BoundsY => _parent.BoundsY;
        private int BoundsZ => _parent.BoundsZ;
        private int Width => Chunk.Width;
        private int Height => Chunk.Height;
        private static float BlockSize => Chunk.BlockSize;
        private Block[][][] Blocks => _parent.Voxels;

        public Vector4 GetColor(GridCell Cell, BlockType Type, Chunk RightChunk, Chunk FrontChunk,
            Chunk RightFrontChunk, Chunk LeftBackChunk, Chunk RightBackChunk, Chunk LeftFrontChunk, Chunk BackChunk,
            Chunk LeftChunk, int Width, int Height, int Depth, List<Vector4> AddonColors, RegionColor RegionColor)
        {
            Vector3 position = Cell.P[0] / BlockSize;
            Vector4 color = Vector4.Zero;
            float colorCount = 0;
            int x = (int)position.X, y = (int)position.Y, z = (int)position.Z;

            float noise = (float)OpenSimplexNoise.Evaluate((Cell.P[0].X + OffsetX) * .00075f, (Cell.P[0].Z + OffsetZ) * .00075f);
            RegionColor regionColor = RegionColor;

            for (int _x = -Lod * 1; _x < 1 * Lod + 1; _x += Lod)
                for (int _z = -Lod * 1; _z < 1 * Lod + 1; _z += Lod)
                    for (int _y = -1; _y < 1 + 1; _y++)
                    {
                        Block y0 = this.GetNeighbourBlock(x + _x, (int)Mathf.Clamp(y + _y, 0, this.Height - 1), z + _z,
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
                                var nextIndex = (int)Math.Ceiling(clampNoise / levelSize);
                                if (nextIndex == regionColor.GrassColors.Length) nextIndex = 0;

                                Vector4 A = regionColor.GrassColors[(int)Math.Floor(clampNoise / levelSize)],
                                    B = regionColor.GrassColors[nextIndex];

                                float delta = clampNoise / levelSize - (float)Math.Floor(clampNoise / levelSize);

                                blockColor = Mathf.Lerp(A, B, delta);
                            }
                            color += new Vector4(blockColor.X, blockColor.Y, blockColor.Z, blockColor.W);
                            colorCount++;
                        }
                    }
            return new Vector4(color.Xyz / colorCount, 1.0f);
        }

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
                    Block block = this.GetNeighbourBlock((int)pos.X, (int)pos.Y, (int)pos.Z,
                        RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                        BackChunk, LeftChunk);

                    Cell.Type[i] = block.Type;
                    Cell.Density[i] = block.Density;

                    if (block.Type == BlockType.Water && (int)pos.Y > BiomePool.SeaLevel)
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
                    Block waterBlock = this.GetNeighbourBlock((int)pos.X, (int)pos.Y, (int)pos.Z,
                        RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                        BackChunk, LeftChunk);

                    if (waterBlock.Type != BlockType.Water)
                    {
                        for (int k = (int)pos.Y - 3; k < BoundsY; k++)
                        {
                            waterBlock = this.GetNeighbourBlock((int)pos.X, k, (int)pos.Z,
                                RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                                BackChunk, LeftChunk);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }

                        for (int k = (int)pos.Y - 3; k < BoundsY; k++)
                            for (int kx = -2; kx < 3; kx++)
                                for (int kz = -2; kz < 3; kz++)
                                {
                                    waterBlock = this.GetNeighbourBlock((int)pos.X + kx, k, (int)pos.Z + kz,
                                        RightChunk, FrontChunk, RightFrontChunk, LeftBackChunk, RightBackChunk, LeftFrontChunk,
                                        BackChunk, LeftChunk);


                                    if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                                }
                    }
                    WATER_BREAK:

                    var scaleFactor = 65530.0;
                    double cp = 256.0 * 256.0;

                    double dy = Math.Floor(waterBlock.Density / cp);
                    var yk = (float)(dy / scaleFactor);
                    yk *= 10.0f;


                    for (int k = Y; k > -1; k--)
                    {
                        Block block = this.GetNeighbourBlock((int)pos.X, k, (int)pos.Z,
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
                if (Cell.Type[i] == BlockType.Temporal && Y < this.Height - 2)
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
                            y = this.GetNeighbourBlock((int)position.X, (int)position.Y, (int)position.Z, RightChunk,
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
    }
}
