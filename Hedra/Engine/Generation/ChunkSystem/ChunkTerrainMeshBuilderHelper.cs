using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    internal class ChunkTerrainMeshBuilderHelper
    {
        private readonly Chunk _parent;
        private readonly float _coefficient = 1 / BlockSize;

        public ChunkTerrainMeshBuilderHelper(Chunk Parent)
        {
            _parent = Parent;
        }

        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private int BoundsX => _parent.BoundsX;
        private int BoundsY => _parent.BoundsY;
        private int BoundsZ => _parent.BoundsZ;
        private int Width => Chunk.Width;
        private int Height => Chunk.Height;
        private static float BlockSize => Chunk.BlockSize;

        public Vector4 GetColor(GridCell Cell, BlockType Type, int Width, int Height, int Depth,
            List<Vector4> AddonColors, RegionColor RegionColor, int Lod)
        {
            Vector3 position = Cell.P[0] / BlockSize;
            Vector4 color = Vector4.Zero;
            float colorCount = 0;
            int x = (int) position.X, y = (int) position.Y, z = (int) position.Z;

            float noise =
                (float) OpenSimplexNoise.Evaluate((Cell.P[0].X + OffsetX) * .00075f, (Cell.P[0].Z + OffsetZ) * .00075f);
            RegionColor regionColor = RegionColor;

            for (int _x = -Lod * 1; _x < 1 * Lod + 1; _x += Lod)
            for (int _z = -Lod * 1; _z < 1 * Lod + 1; _z += Lod)
            for (int _y = -1; _y < 1 + 1; _y++)
            {
                Block y0 = this.GetNeighbourBlock(x + _x, (int) Mathf.Clamp(y + _y, 0, this.Height - 1), z + _z);

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

        public void CreateCell(ref GridCell Cell, int X, int Y, int Z, int Width, int Height, int Depth, bool ExtraData,
            bool WaterCell, int Lod, out bool Success)
        {
            Success = true;
            this.BuildCell(ref Cell, X, Y, Z, WaterCell, Lod);

            if (!WaterCell)
            {
                for (var i = 0; i < Cell.Type.Length; i++)
                {
                    var pos = new Vector3(Cell.P[i].X * _coefficient, Cell.P[i].Y * _coefficient,
                        Cell.P[i].Z * _coefficient);
                    var block = this.GetNeighbourBlock((int) pos.X, (int) pos.Y, (int) pos.Z);

                    Cell.Type[i] = block.Type;
                    Cell.Density[i] = block.Density;
                }
            }
            else
            {
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
                    Block waterBlock = this.GetNeighbourBlock((int) pos.X, (int) pos.Y, (int) pos.Z);

                    if (waterBlock.Type != BlockType.Water)
                    {
                        for (int k = (int) pos.Y - 3; k < BoundsY; k++)
                        {
                            waterBlock = this.GetNeighbourBlock((int) pos.X, k, (int) pos.Z);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }

                        for (int k = (int) pos.Y - 3; k < BoundsY; k++)
                        for (int kx = -2; kx < 3; kx++)
                        for (int kz = -2; kz < 3; kz++)
                        {
                            waterBlock = this.GetNeighbourBlock((int) pos.X + kx, k, (int) pos.Z + kz);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }
                    }
                    WATER_BREAK:

                    var neighbourChunk = this.GetNeighbourChunk((int) pos.X, Y, (int) pos.Z);
                    var x = (int) (pos.X % BoundsX);
                    var z = (int) (pos.Z % BoundsZ);

                    var newHeight = neighbourChunk?.GetWaterDensity(new Vector3(x, Y, z)) ?? default(Half);
                    for (int k = Math.Min(Y + 8, Chunk.Height - 1); k > -1 && Math.Abs(newHeight) < 0.005f; k--)
                    {
                        newHeight = neighbourChunk?.GetWaterDensity(new Vector3(x, k, z)) ?? default(Half);
                    }
                    for (int k = Math.Min(Y + 8, Chunk.Height - 1); k > -1; k--)
                    {
                        var block = neighbourChunk?.GetBlockAt(x, k, z) ?? new Block();
                        if (block.Type == BlockType.Seafloor)
                        {
                            Cell.Density[i] = 0;
                            Cell.P[i] = new Vector3(Cell.P[i].X, newHeight, Cell.P[i].Z);
                            break;
                        }
                    }
                }
            }

            for (var i = 0; i < Cell.Type.Length; i++)
            {
                if (Cell.Type[i] == BlockType.Temporal && Y < this.Height - 2)
                {
                    Success = false;
                    Cell.Type[i] = BlockType.Air;
                }
            }
        }

        private void BuildCell(ref GridCell Cell, int X, int Y, int Z, bool WaterCell, int Lod)
        {
            int lod = Lod;
            float blockSizeLod = BlockSize * lod;
            if (WaterCell)
            {
                Cell.P[0] = new Vector3(X, Y, Z);
                Cell.P[1] = new Vector3(X + blockSizeLod, Y, Z);
                Cell.P[2] = new Vector3(X + blockSizeLod, Y, Z + blockSizeLod);
                Cell.P[3] = new Vector3(X, Y, Z + blockSizeLod);  
                Cell.P[4] = new Vector3(X, Y + BlockSize, Z); 
                Cell.P[5] = new Vector3(X + blockSizeLod, Y + BlockSize, Z);
                Cell.P[6] = new Vector3(X + blockSizeLod, Y + BlockSize, Z + blockSizeLod); 
                Cell.P[7] = new Vector3(X, Y + BlockSize, Z + blockSizeLod);
            }
            else
            {
                Cell.P[0] = new Vector3(X * BlockSize, Y * BlockSize, Z * BlockSize);
                Cell.P[1] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[2] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[3] = new Vector3(Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[4] = new Vector3(Cell.P[0].X, BlockSize + Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[5] = new Vector3(blockSizeLod + Cell.P[0].X, BlockSize + Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[6] = new Vector3(blockSizeLod + Cell.P[0].X, BlockSize + Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[7] = new Vector3(Cell.P[0].X, BlockSize + Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
            }
        }

        private Chunk GetNeighbourChunk(int X, int Y, int Z)
        {
            if (X >= 0 && X < BoundsX && Z >= 0 && Z < BoundsZ) return _parent;
            var coords = World.ToChunkSpace(new Vector3(OffsetX + X * Chunk.BlockSize, 0, OffsetZ + Z * Chunk.BlockSize));
            return World.SearcheableChunks.ContainsKey(coords) ? World.SearcheableChunks[coords] : null;
        }

        private Block GetNeighbourBlock(int X, int Y, int Z)
        {
            var chunk = this.GetNeighbourChunk(X, Y, Z);
            if (!chunk?.Landscape?.BlocksSetted ?? true) return new Block(BlockType.Temporal);
            return chunk[Modulo(X, BoundsX)][Y][Modulo(Z, BoundsZ)];

        }
        
        // Source: https://codereview.stackexchange.com/a/58309
        private static int Modulo(int Index, int Bounds)
        {
            return (Index % Bounds + Bounds) % Bounds;
        }
    }
}
