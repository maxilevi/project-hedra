using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkTerrainMeshBuilderHelper
    {
        private static int Bounds = (int) (Chunk.Width / Chunk.BlockSize); 
        private readonly Chunk _parent;
        private readonly float _coefficient;    
        private readonly int _offsetX;
        private readonly int _offsetZ;
        private readonly int _boundsX;
        private readonly int _boundsY;
        private readonly int _boundsZ;
        private readonly int _height;
        private readonly float _blockSize;

        public ChunkTerrainMeshBuilderHelper(Chunk Parent)
        {
            _parent = Parent;
            _offsetX = _parent.OffsetX;
            _offsetZ = _parent.OffsetZ;
            _blockSize = Chunk.BlockSize;
            _boundsX = (int) (Chunk.Width / _blockSize);
            _boundsY = Chunk.Height;
            _boundsZ = (int) (Chunk.Width / _blockSize);
            _height = Chunk.Height;
            _coefficient =  1 / _blockSize;
        }

        public Vector4 GetColor(GridCell Cell, RegionColor RegionColor, int Lod)
        {
            Vector3 position = Cell.P[0] / _blockSize;
            Vector4 color = Vector4.Zero;
            float colorCount = 0;
            int x = (int) position.X, y = (int) position.Y, z = (int) position.Z;

            float noise =
                (float) OpenSimplexNoise.Evaluate((Cell.P[0].X + _offsetX) * .00075f, (Cell.P[0].Z + _offsetZ) * .00075f);
            RegionColor regionColor = RegionColor;

            for (var _x = -Lod * 1; _x < 1 * Lod + 1; _x += Lod)
            for (var _z = -Lod * 1; _z < 1 * Lod + 1; _z += Lod)
            for (var _y = -1; _y < 1 + 1; _y++)
            {
                Block y0 = this.GetNeighbourBlock(x + _x, (int) Mathf.Clamp(y + _y, 0, this._height - 1), z + _z);

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

        public void CreateCell(ref GridCell Cell, int X, int Y, int Z, bool ExtraData,
            bool WaterCell, int Lod, out bool Success)
        {
            Success = true;
            this.BuildCell(ref Cell, X, Y, Z, WaterCell, Lod);

            if (!WaterCell)
            {
                for (var i = 0; i < Cell.Type.Length; i++)
                {
                    var posX = (int) (Cell.P[i].X * _coefficient);
                    var posY = (int) (Cell.P[i].Y * _coefficient);
                    var posZ = (int) (Cell.P[i].Z * _coefficient);
                    
                    Block block;
                    unsafe
                    {
                        block = GetNeighbourBlock(&posX, &posY, &posZ);
                    }

                    Cell.Type[i] = block.Type;
                    Cell.Density[i] = block.Density;
                }
            }
            else
            {
                var cz = new GridCell();
                cz.P = new Vector3[4];
                cz.P[0] = new Vector3(X * _blockSize, Y * _blockSize, Z * _blockSize);
                cz.P[1] = new Vector3(_blockSize * Lod + cz.P[0].X, cz.P[0].Y, cz.P[0].Z);
                cz.P[2] = new Vector3(_blockSize * Lod + cz.P[0].X, cz.P[0].Y, _blockSize * Lod + cz.P[0].Z);
                cz.P[3] = new Vector3(cz.P[0].X, cz.P[0].Y, _blockSize * Lod + cz.P[0].Z);

                for (var i = 0; i < 4; i++)
                {
                    var pos = new Vector3(cz.P[i].X * _coefficient, cz.P[i].Y * _coefficient,
                        cz.P[i].Z * _coefficient); // LOD is 1
                    Block waterBlock = this.GetNeighbourBlock((int) pos.X, (int) pos.Y, (int) pos.Z);

                    if (waterBlock.Type != BlockType.Water)
                    {
                        for (int k = (int) pos.Y - 3; k < _boundsY; k++)
                        {
                            waterBlock = this.GetNeighbourBlock((int) pos.X, k, (int) pos.Z);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }

                        for (var k = (int) pos.Y - 3; k < _boundsY; k++)
                        for (var kx = -2; kx < 3; kx++)
                        for (var kz = -2; kz < 3; kz++)
                        {
                            waterBlock = this.GetNeighbourBlock((int) pos.X + kx, k, (int) pos.Z + kz);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }
                    }
                    WATER_BREAK:

                    var neighbourChunk = this.GetNeighbourChunk((int) pos.X, (int) pos.Z);
                    var x = (int) (pos.X % _boundsX);
                    var z = (int) (pos.Z % _boundsZ);

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
                if (Cell.Type[i] == BlockType.Temporal && Y < this._height - 2)
                {
                    Success = false;
                    Cell.Type[i] = BlockType.Air;
                }
            }
        }

        private void BuildCell(ref GridCell Cell, int X, int Y, int Z, bool WaterCell, int Lod)
        {
            int lod = Lod;
            float blockSizeLod = _blockSize * lod;
            if (WaterCell)
            {
                Cell.P[0] = new Vector3(X, Y, Z);
                Cell.P[1] = new Vector3(X + blockSizeLod, Y, Z);
                Cell.P[2] = new Vector3(X + blockSizeLod, Y, Z + blockSizeLod);
                Cell.P[3] = new Vector3(X, Y, Z + blockSizeLod);  
                Cell.P[4] = new Vector3(X, Y + _blockSize, Z); 
                Cell.P[5] = new Vector3(X + blockSizeLod, Y + _blockSize, Z);
                Cell.P[6] = new Vector3(X + blockSizeLod, Y + _blockSize, Z + blockSizeLod); 
                Cell.P[7] = new Vector3(X, Y + _blockSize, Z + blockSizeLod);
            }
            else
            {
                Cell.P[0] = new Vector3(X * _blockSize, Y * _blockSize, Z * _blockSize);
                Cell.P[1] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[2] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[3] = new Vector3(Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[4] = new Vector3(Cell.P[0].X, _blockSize + Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[5] = new Vector3(blockSizeLod + Cell.P[0].X, _blockSize + Cell.P[0].Y, Cell.P[0].Z);
                Cell.P[6] = new Vector3(blockSizeLod + Cell.P[0].X, _blockSize + Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
                Cell.P[7] = new Vector3(Cell.P[0].X, _blockSize + Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
            }
        }

        private unsafe Chunk GetNeighbourChunk(int X, int Z)
        {
            return GetNeighbourChunk(&X, &Z);
        }
        
        //Use ref to avoid copying the structs since this function has a very high call rate.
        private unsafe Chunk GetNeighbourChunk(int* X, int* Z)
        {
            if (*X >= 0 && *X < _boundsX && *Z >= 0 && *Z < _boundsZ) return _parent;
            var coords = World.ToChunkSpace(new Vector3(_offsetX + *X * _blockSize, 0, _offsetZ + *Z * _blockSize));
            World.SearcheableChunks.TryGetValue(coords, out var ch);
            return ch;
        }

        private unsafe Block GetNeighbourBlock(int X, int Y, int Z)
        {
            return GetNeighbourBlock(&X, &Y, &Z);
        }

        private unsafe Block GetNeighbourBlock(int* X, int* Y, int* Z)
        {
            var chunk = GetNeighbourChunk(X, Z);
            if (!chunk?.Landscape.BlocksSetted ?? true) return new Block(BlockType.Temporal);
            return chunk[Modulo(X)][*Y][Modulo(Z)];
        }
        
        // Source: https://codereview.stackexchange.com/a/58309
        private static unsafe int Modulo(int* Index)
        {
            return (*Index % Bounds + Bounds) % Bounds;
        }
    }
}
