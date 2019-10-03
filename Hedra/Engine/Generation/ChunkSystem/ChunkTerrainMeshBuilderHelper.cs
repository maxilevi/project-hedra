using System;
using System.Runtime.CompilerServices;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public unsafe class ChunkTerrainMeshBuilderHelper
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
        private int noiseValuesMapWidth;
        private int noiseValuesMapHeight;
        private SampledBlock* _grid;

        public ChunkTerrainMeshBuilderHelper(Chunk Parent, int Lod, SampledBlock* Grid)
        {
            _parent = Parent;
            _grid = Grid;
            _offsetX = _parent.OffsetX;
            _offsetZ = _parent.OffsetZ;
            _blockSize = Chunk.BlockSize;
            _boundsX = (int) (Chunk.Width / _blockSize);
            _boundsY = Chunk.Height;
            _boundsZ = (int) (Chunk.Width / _blockSize);
            _height = Chunk.Height;
            _coefficient =  1 / _blockSize;
            SetSampleSize(Lod);
            BuildDensityGrid(Lod);
        }
        
        public static int CalculateGridSize(int Lod)
        {
            var width = (Chunk.BoundsX / Lod) + 1;
            var height = (Chunk.BoundsY / Lod);
            return width * height * width;
        }

        private void SetSampleSize(int Lod)
        {
            _sampleWidth = Lod;
            _sampleHeight = Lod;
            noiseValuesMapWidth = (_boundsX / _sampleWidth) + 1;
            noiseValuesMapHeight = (_boundsY / _sampleHeight);
        }

        public Vector4 GetColor(ref GridCell Cell, RegionColor RegionColor)
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
                AddColorIfNecessary(x + _x, _y + y, z + _z, ref regionColor, ref noise, ref color, ref colorCount);
            }
            /* Try with a broader search */
            if (colorCount == 0)
            {
                for (var _x = -_sampleWidth; _x < 1 + _sampleWidth; _x+=_sampleWidth)
                for (var _z = -_sampleWidth; _z < 1 + _sampleWidth; _z+=_sampleWidth)
                for (var _y = -_sampleHeight; _y < 1 + _sampleHeight; _y+=_sampleHeight)
                {
                    AddColorIfNecessary(x + _x, _y + y, z + _z, ref regionColor, ref noise, ref color, ref colorCount);
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

        private void AddColorIfNecessary(int X, int Y, int Z, ref RegionColor RegionColor, ref float Noise, ref Vector4 Color, ref int ColorCount)
        {
            GetSampleOrNeighbour(X, Y.Clamp0(), Z, out var type);
            if (type != BlockType.Water && type != BlockType.Air && type != BlockType.None)
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

                var delta = clampNoise / levelSize - (float) Math.Floor(clampNoise / levelSize);
                blockColor = Mathf.Lerp(A, B, delta);
            }
            else if (Type == BlockType.Stone || Type == BlockType.StonePath)
            {
                var shade = (Utils.Rng.NextFloat() * 2 - 1f) * .2f * (Type == BlockType.StonePath ? 3f : 1f);
                blockColor += new Vector4(shade, shade, shade, 0); 
            }
            else if (Type == BlockType.FarmDirt)
            {
                if((x+1) % 2 == 0)
                    blockColor -= new Vector4(.1f, .1f, .1f, 0);
            }
            return blockColor;
        }

        private void BuildDensityGrid(int Lod)
        {
            for (var x = 0; x < noiseValuesMapWidth; x++)
            {
                for (var y = 0; y < noiseValuesMapHeight; y++)
                {
                    for (var z = 0; z < noiseValuesMapWidth; z++)
                    {
                        var block0 = GetNeighbourBlock(x * _sampleWidth, y * _sampleHeight, z * _sampleWidth);
                        var mainDensity = block0.Density;
                        var density = mainDensity;
                        var count = 1;
                        if (Lod > 1)
                        {

                            for (var _y = -1; _y < 2; _y++)
                            {
                                if (_y == 0 || (y + _y) < 0 || (y + _y) >= _boundsY) continue;
                                var block = GetNeighbourBlock(x * _sampleWidth, (y+_y) * _sampleHeight, z * _sampleWidth);
                                density += block.Density;
                                count++;
                            }
                            density /= count;
                            density = mainDensity > 0 && density < 0 ? mainDensity : density;
                        }

                        _grid[x * noiseValuesMapHeight * noiseValuesMapWidth + y * noiseValuesMapWidth + z] = new SampledBlock
                        {
                            Density = density,
                            Type = block0.Type
                        };
                    }
                }
            }
        }

        public void CreateCell(ref GridCell Cell, ref int X, ref int Y, ref int Z, bool isWaterCell, int HorizontalLod, int VerticalLod, out bool Success)
        {
            Success = true;
            this.BuildCell(ref Cell, X, Y, Z, HorizontalLod, VerticalLod);

            for (var i = 0; i < Cell.Type.Length; i++)
            {
                var posX = (int) (Cell.P[i].X * _coefficient);
                var posY = (int) (Cell.P[i].Y * _coefficient);
                var posZ = (int) (Cell.P[i].Z * _coefficient);
                
                if (isWaterCell)
                {
                    Cell.Type[i] = GetNeighbourBlock(posX, posY, posZ).Type;
                    Cell.Density[i] = Cell.Type[i] == BlockType.Water ? 1 : -1;
                }
                else
                {
                    Cell.Density[i] = GetSampleOrNeighbour(posX, posY, posZ, out Cell.Type[i]);
                    //Cell.Density[i] = Cell.Type[i] != BlockType.Air && Cell.Type[i] != BlockType.Water ? 1 : -1;
                }
            }

            for (var i = 0; i < Cell.Type.Length; i++)
            {
                if (Cell.Type[i] == BlockType.None && Y < this._height - 2)
                {
                    Success = false;
                    Cell.Type[i] = BlockType.Air;
                }
            }
        }

        private float GetSampleOrNeighbour(int x, int y, int z, out BlockType Type)
        {
            y = Math.Min(y, _boundsY - _sampleHeight - 1);
            if (x <= 0 || z <= 0 || y <= 4 || x >= _boundsX - 1 || z >= _boundsZ - 1 || _sampleWidth == 1 && _sampleHeight == 1)
            {
                var b = GetNeighbourBlock(x, y, z);
                Type = b.Type;
                return b.Density;
            }
            else
            {
                return GetSample(x, y, z, out Type);
            }
        }
        
        [MethodImpl(256)]
        private SampledBlock Get(int _x, int _y, int _z)
        {
            return _grid[_x * noiseValuesMapWidth * noiseValuesMapHeight + _y * noiseValuesMapWidth + _z];
        }

        private float GetSample(int x, int y, int z, out BlockType Type)
        {
            var x2 = (x / _sampleWidth);
            var y2 = (y / _sampleHeight);
            var z2 = (z / _sampleWidth);

            var b0 = Get(x2,y2,z2);
            var b1 = Get(x2 + 1, y2, z2);
            var b2 = Get(x2, y2 + 1, z2);
            var b3 = Get(x2 + 1, y2 + 1, z2);
            var b4 = Get(x2, y2, z2 + 1);
            var b5 = Get(x2 + 1, y2, z2 + 1);
            var b6 = Get(x2, y2 + 1, z2 + 1);
            var b7 = Get(x2 + 1, y2 + 1, z2 + 1);
            Type = b0.Type;
            return Mathf.LinearInterpolate3D(
                b0.Density, b1.Density,
                b2.Density, b3.Density,
                b4.Density, b5.Density,
                b6.Density, b7.Density,
                (x % _sampleWidth) / (float) _sampleWidth,
                (y % _sampleHeight) / (float) _sampleHeight,
                (z % _sampleWidth) / (float) _sampleWidth
            );
        }

        private void BuildCell(ref GridCell Cell, int X, int Y, int Z, int HorizontalLod, int VerticalLod)
        {
            var horizontalBlockSize = _blockSize * HorizontalLod;
            var verticalBlockSize = _blockSize * VerticalLod;
            Cell.P[0] = new Vector3(X * _blockSize, Y * _blockSize, Z * _blockSize);
            Cell.P[1] = new Vector3(horizontalBlockSize + Cell.P[0].X, Cell.P[0].Y, Cell.P[0].Z);
            Cell.P[2] = new Vector3(horizontalBlockSize + Cell.P[0].X, Cell.P[0].Y, horizontalBlockSize + Cell.P[0].Z);
            Cell.P[3] = new Vector3(Cell.P[0].X, Cell.P[0].Y, horizontalBlockSize + Cell.P[0].Z);
            Cell.P[4] = new Vector3(Cell.P[0].X, verticalBlockSize + Cell.P[0].Y, Cell.P[0].Z);
            Cell.P[5] = new Vector3(horizontalBlockSize + Cell.P[0].X, verticalBlockSize + Cell.P[0].Y, Cell.P[0].Z);
            Cell.P[6] = new Vector3(horizontalBlockSize + Cell.P[0].X, verticalBlockSize + Cell.P[0].Y, horizontalBlockSize + Cell.P[0].Z);
            Cell.P[7] = new Vector3(Cell.P[0].X, verticalBlockSize + Cell.P[0].Y, horizontalBlockSize + Cell.P[0].Z);
        }

        [MethodImpl(256)]
        private Chunk GetNeighbourChunk(ref int X, ref int Z)
        {
            var offset = new Vector2(((int) (_offsetX + X * Chunk.BlockSize) >> 7) << 7, ((int) (_offsetZ + Z * Chunk.BlockSize) >> 7) << 7);
            if ((int)offset.X == this._offsetX && (int)offset.Y == this._offsetZ) return _parent;
            World.SearcheableChunksReference.TryGetValue(offset, out var chunk);
            return chunk;
        }

        [MethodImpl(256)]
        private Block GetNeighbourBlock(int X, int Y, int Z)
        {
            return GetNeighbourBlock(ref X, ref Y, ref Z);
        }

        [MethodImpl(256)]
        private Block GetNeighbourBlock(ref int X, ref int Y, ref int Z)
        {
            var chunk = GetNeighbourChunk(ref X, ref Z);
            if (!chunk?.Landscape.BlocksSetted ?? true) return new Block(BlockType.None);
            return chunk[Modulo(ref X) * _boundsY * _boundsZ + Y * _boundsZ + Modulo(ref Z)];
        }
        
        // Source: https://codereview.stackexchange.com/a/58309
        [MethodImpl(256)]
        private static int Modulo(ref int Index)
        {
            return (Index % Bounds + Bounds) % Bounds;
        }
    }
}
