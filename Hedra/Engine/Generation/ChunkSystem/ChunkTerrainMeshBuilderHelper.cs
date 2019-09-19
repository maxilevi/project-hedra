using System;
using System.Runtime.CompilerServices;
using Hedra.BiomeSystem;
using Hedra.Core;
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
        private int _offsetX;
        private int _offsetZ;
        private readonly int _boundsX;
        private readonly int _boundsY;
        private readonly int _boundsZ;
        private readonly int _height;
        private readonly float _blockSize;
        private int _sampleWidth;
        private int _sampleHeight; 

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

        private void SetSampleSize(int Lod)
        {
            _sampleWidth = Lod;
            _sampleHeight = Lod;
        }

        public Vector4 GetColor(SampledBlock[][][] Grid, ref GridCell Cell, RegionColor RegionColor)
        {
            var position = Cell.P[0] / _blockSize;
            int x = (int) position.X, y = (int) position.Y, z = (int) position.Z;
            var color = Vector4.Zero;
            var colorCount = 0;
            var noise = (float) World.GetNoise((Cell.P[0].X + _offsetX) * .00075f, (Cell.P[0].Z + _offsetZ) * .00075f);
            var regionColor = RegionColor;

            for (var _x = -1; _x < 1 + 1; _x+=1)
            for (var _z = -1; _z < 1 + 1; _z+=1)
            for (var _y = -2; _y < 1 + 2; _y+=2)
            {
                AddColorIfNecessary(Grid, x + _x, _y + y, z + _z, ref regionColor, ref noise, ref color, ref colorCount);
            }
            /* Try with a broader search */
            if (colorCount == 0)
            {
                for (var _x = -_sampleWidth; _x < 1 + _sampleWidth; _x+=_sampleWidth)
                for (var _z = -_sampleWidth; _z < 1 + _sampleWidth; _z+=_sampleWidth)
                for (var _y = -_sampleHeight; _y < 1 + _sampleHeight; _y+=_sampleHeight)
                {
                    AddColorIfNecessary(Grid, x + _x, _y + y, z + _z, ref regionColor, ref noise, ref color, ref colorCount);
                }
            }
            /*float wSeed = World.Seed * 0.0001f;
            var f = .005f;
            var voronoi = (int) (World.StructureHandler.SeedGenerator.GetValue(_offsetX * f + wSeed, _offsetZ * f + wSeed) * 100f);
            var rng = new Random(voronoi);
            var pointCoords = World.StructureHandler.SeedGenerator.GetGridPoint(_offsetX * f + wSeed, _offsetZ * f + wSeed);
            var chunkCoords = World.ToChunkSpace(new Vector2((int)((pointCoords.X - wSeed) / f), (int)((pointCoords.Y - wSeed) / f)));
            var isPoint = chunkCoords == new Vector2(_offsetX, _offsetZ);
            var c = new Vector4(rng.NextFloat(), rng.NextFloat(), rng.NextFloat(), 1.0f);
            return isPoint ? new Vector4(0, 0, 0, 1.0f) : c;*/
            return new Vector4(colorCount == 0 ? Vector3.Zero : color.Xyz / colorCount, 1.0f);
        }

        private void AddColorIfNecessary(SampledBlock[][][] Grid, int X, int Y, int Z, ref RegionColor RegionColor, ref float Noise, ref Vector4 Color, ref int ColorCount)
        {
            GetSampleOrNeighbour(Grid, X, Y.Clamp0(), Z, out var type);
            if (type != BlockType.Water && type != BlockType.Air && type != BlockType.Temporal)
            {
                var blockColor = DoGetColor(ref RegionColor, ref X, ref Z, type, ref Noise);
                Color += blockColor * 32;
                ColorCount += 32;
            }
        }

        private static Vector4 DoGetColor(ref RegionColor RegionColor, ref int x, ref int z, BlockType Type, ref float Noise)
        {
            var blockColor = Block.GetColor(Type, RegionColor);
            if (Type == BlockType.Grass)
            {
                var clampNoise = (Noise + 1) * .5f;
                var levelSize = 1.0f / RegionColor.GrassColors.Length;
                var nextIndex = (int) Math.Ceiling(clampNoise / levelSize);
                if (nextIndex == RegionColor.GrassColors.Length) nextIndex = 0;

                Vector4 A = RegionColor.GrassColors[(int) Math.Floor(clampNoise / levelSize)],
                    B = RegionColor.GrassColors[nextIndex];

                float delta = clampNoise / levelSize - (float) Math.Floor(clampNoise / levelSize);

                blockColor = Mathf.Lerp(A, B, delta);
            }
            else if (Type == BlockType.StonePath || Type == BlockType.Stone)
            {
                var shade = (Utils.Rng.NextFloat() * 2 - 1f) * .2f * (Type == BlockType.Stone ? 1f : 1);
                blockColor += new Vector4(shade, shade, shade, 0); 
            }
            else if (Type == BlockType.FarmDirt)
            {
                if((x+1) % 2 == 0)
                    blockColor -= new Vector4(.1f, .1f, .1f, 0);
            }
            return blockColor;
        }
        
        public SampledBlock[][][] BuildDensityGrid(int Lod)
        {
            SetSampleSize(Lod);
            var noiseValuesMapWidth = (_boundsX / _sampleWidth) + 1;
            var noiseValuesMapHeight = (_boundsY / _sampleHeight);
            var densities = new SampledBlock[noiseValuesMapWidth][][];
            for (var x = 0; x < noiseValuesMapWidth; x++)
            {
                densities[x] = new SampledBlock[noiseValuesMapHeight][];
                for (var y = 0; y < noiseValuesMapHeight; y++)
                {
                    densities[x][y] = new SampledBlock[noiseValuesMapWidth];
                    for (var z = 0; z < noiseValuesMapWidth; z++)
                    {
                        var block = GetNeighbourBlock(x * _sampleWidth, y * _sampleHeight, z * _sampleWidth);
                        densities[x][y][z] = new SampledBlock
                        {
                            Density = block.Density,
                            Type = block.Type
                        };
                    }
                }
            }
            return densities;
        }

        public void CreateCell(SampledBlock[][][] Grid, ref GridCell Cell, ref int X, ref int Y, ref int Z, bool isWaterCell, out bool Success)
        {
            Success = true;
            this.BuildCell(ref Cell, X, Y, Z, isWaterCell);

            for (var i = 0; i < Cell.Type.Length; i++)
            {
                var posX = (int) (Cell.P[i].X * _coefficient);
                var posY = (int) (Cell.P[i].Y * _coefficient);
                var posZ = (int) (Cell.P[i].Z * _coefficient);
                
                if (isWaterCell)
                {
                    Cell.Type[i] = GetNeighbourBlock(posX, posY, posZ).Type;
                    Cell.Density[i] = Cell.Type[i] == BlockType.Water ? 1 : 0;
                }
                else
                {
                    Cell.Density[i] = GetSampleOrNeighbour(Grid, posX, posY, posZ, out Cell.Type[i]);
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

        private float GetSampleOrNeighbour(SampledBlock[][][] Grid, int x, int y, int z, out BlockType Type)
        {
            y = Math.Min(y, _boundsY - _sampleHeight - 1);
            if (x <= 0 || z <= 0 || x >= _boundsX - 1 || z >= _boundsZ - 1)
            {
                var b = GetNeighbourBlock(x, y, z);
                Type = b.Type;
                return b.Density;
            }
            else
            {
                return GetSample(Grid, x, y, z, out Type);
            }
        }

        private float GetSample(SampledBlock[][][] Grid, int x, int y, int z, out BlockType Type)
        {
            var x2 = (x / _sampleWidth);
            var y2 = (y / _sampleHeight);
            var z2 = (z / _sampleWidth);
            Type = Grid[x2][y2][z2].Type;
            return Mathf.LinearInterpolate3D(
                Grid[x2][y2][z2].Density, Grid[x2 + 1][y2][z2].Density,
                Grid[x2][y2 + 1][z2].Density, Grid[x2 + 1][y2 + 1][z2].Density,
                Grid[x2][y2][z2 + 1].Density, Grid[x2 + 1][y2][z2 + 1].Density,
                Grid[x2][y2 + 1][z2 + 1].Density, Grid[x2 + 1][y2 + 1][z2 + 1].Density,
                (x % _sampleWidth) / (float) _sampleWidth,
                (y % _sampleHeight) / (float) _sampleHeight,
                (z % _sampleWidth) / (float) _sampleWidth
            );
        }
        
        private void BuildCell(ref GridCell Cell, int X, int Y, int Z, bool isWaterCell)
        {
            var lod = isWaterCell ? 2 : 1;
            var blockSizeLod = _blockSize * lod;
            Cell.P[0] = new Vector3(X * _blockSize, Y * _blockSize, Z * _blockSize);
            Cell.P[1] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, Cell.P[0].Z);
            Cell.P[2] = new Vector3(blockSizeLod + Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
            Cell.P[3] = new Vector3(Cell.P[0].X, Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
            Cell.P[4] = new Vector3(Cell.P[0].X, _blockSize + Cell.P[0].Y, Cell.P[0].Z);
            Cell.P[5] = new Vector3(blockSizeLod + Cell.P[0].X, _blockSize + Cell.P[0].Y, Cell.P[0].Z);
            Cell.P[6] = new Vector3(blockSizeLod + Cell.P[0].X, _blockSize + Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
            Cell.P[7] = new Vector3(Cell.P[0].X, _blockSize + Cell.P[0].Y, blockSizeLod + Cell.P[0].Z);
        }

        //Use ref to avoid copying the structs since this function has a very high call rate.
        [MethodImpl(256)]
        private static Chunk GetNeighbourChunk(ref float X, ref float Z, ref int _offsetX, ref int _offsetZ)
        {
            World.SearcheableChunksReference.TryGetValue(new Vector2(((int) (_offsetX + X * Chunk.BlockSize) >> 7) << 7, ((int) (_offsetZ + Z * Chunk.BlockSize) >> 7) << 7), out var ch);
            return ch;
        }
        
        //Use ref to avoid copying the structs since this function has a very high call rate.
        [MethodImpl(256)]
        private static Chunk GetNeighbourChunk(ref int X, ref int Z, ref int _offsetX, ref int _offsetZ)
        {
            World.SearcheableChunksReference.TryGetValue(new Vector2(((int) (_offsetX + X * Chunk.BlockSize) >> 7) << 7, ((int) (_offsetZ + Z * Chunk.BlockSize) >> 7) << 7), out var ch);
            return ch;
        }

        [MethodImpl(256)]
        private Block GetNeighbourBlock(int X, int Y, int Z)
        {
            return GetNeighbourBlock(ref X, ref Y, ref Z, ref _offsetX, ref _offsetZ);
        }

        [MethodImpl(256)]
        private static Block GetNeighbourBlock(ref int X, ref int Y, ref int Z, ref int _offsetX, ref int _offsetZ)
        {
            var chunk = GetNeighbourChunk(ref X, ref Z, ref _offsetX, ref _offsetZ);
            if (!chunk?.Landscape.BlocksSetted ?? true) return new Block(BlockType.Temporal);
            return chunk[Modulo(ref X)][Y][Modulo(ref Z)];
        }
        
        // Source: https://codereview.stackexchange.com/a/58309
        [MethodImpl(256)]
        private static int Modulo(ref int Index)
        {
            return (Index % Bounds + Bounds) % Bounds;
        }
    }
}
