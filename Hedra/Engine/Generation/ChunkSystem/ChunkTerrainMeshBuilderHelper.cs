using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Hedra.BiomeSystem;
using Hedra.Engine.Rendering;
using Hedra.Numerics;


namespace Hedra.Engine.Generation.ChunkSystem
{
    public unsafe class ChunkTerrainMeshBuilderHelper : IDisposable
    {
        private static readonly int Bounds = (int)(Chunk.Width / Chunk.BlockSize);
        private readonly float _blockSize;
        private readonly int _boundsX;
        private readonly int _boundsY;
        private readonly int _boundsZ;
        private readonly float _coefficient;
        public readonly SampledBlock* _grid;
        private readonly int _height;
        private readonly Chunk _parent;
        private readonly Chunk[] _neighbours = new Chunk[9];
        private float _invSampleHeight;
        private float _invSampleWidth;
        private readonly int _offsetX;
        private readonly int _offsetZ;
        private int _sampleHeight;
        private int _sampleWidth;
        private int noiseValuesMapHeight;
        private int noiseValuesMapWidth;
        private SampledBlock* _cellSamples;
        private int _cellFirstY;
        private int _cellHeight;
        private ulong[] _cellSignRows;
        private ulong[] _grassRows;
        private ulong[] _otherColorRows;
        private uint _cellStrideMask;
        private BlockType[] _waterTypes;
        private ulong[] _waterSignRows;
        private int _waterFirstY;
        private int _waterHeight;
        private bool _cacheColorSamples;
        private float[] _colorNoise;
        private bool[] _colorNoiseSet;
        private RegionColor[] _grassRegions;
        private Vector4[] _grassColors;
        private const int CellWidth = Chunk.BoundsX + 1;
        private static readonly bool VerifyCells = Environment.GetEnvironmentVariable("HEDRA_VERIFY_MESH_CELLS") == "1";
        private readonly GridCell _referenceCell = VerifyCells ? new GridCell
        {
            P = new Vector3[8], Type = new BlockType[8], Density = new float[8]
        } : default;

        public ChunkTerrainMeshBuilderHelper(Chunk Parent, int Lod, SampledBlock* Grid)
        {
            _parent = Parent;
            _grid = Grid;
            _offsetX = _parent.OffsetX;
            _offsetZ = _parent.OffsetZ;
            _blockSize = Chunk.BlockSize;
            _boundsX = (int)(Chunk.Width / _blockSize);
            _boundsY = Chunk.Height;
            _boundsZ = (int)(Chunk.Width / _blockSize);
            _height = Chunk.Height;
            _coefficient = 1 / _blockSize;
            for (var x = -1; x <= 1; ++x)
            for (var z = -1; z <= 1; ++z)
                _neighbours[(x + 1) * 3 + z + 1] = x == 0 && z == 0 ? Parent
                    : World.GetChunkByOffset(_offsetX + x * Chunk.Width, _offsetZ + z * Chunk.Width);
            SetSampleSize(Lod);
            // LOD 1 always samples original blocks, so its coarse grid was unused.
            if (Lod != 1) BuildDensityGrid(Lod);
        }

        public static int CalculateGridSize(int Lod)
        {
            var width = Chunk.BoundsX / Lod + 1;
            var height = Chunk.BoundsY / Lod;
            return width * height * width;
        }

        private void SetSampleSize(int Lod)
        {
            _sampleWidth = Lod;
            _sampleHeight = Lod;
            _invSampleHeight = 1f / _sampleHeight;
            _invSampleWidth = 1f / _sampleWidth;
            noiseValuesMapWidth = _boundsX / _sampleWidth + 1;
            noiseValuesMapHeight = _boundsY / _sampleHeight;
        }

        public Vector4 GetColor(ref GridCell Cell, RegionColor RegionColor)
        {
            //return (World.TreeGenerator.PlacementNoise(new Vector3(Cell.P[0].X + _offsetX, 0, Cell.P[0].Z + _offsetZ)) < 0) ? new Vector4(1, 0, 0, 1) : new Vector4(0, 0, 1, 1);
            var position = Cell.P[0] / _blockSize;
            int x = (int)position.X, y = (int)position.Y, z = (int)position.Z;
            var color = Vector4.Zero;
            var colorCount = 0;
            var column = x * Chunk.BoundsZ + z;
            float noise;
            if (_cacheColorSamples && _colorNoiseSet[column]) noise = _colorNoise[column];
            else
            {
                noise = World.GetNoise((Cell.P[0].X + _offsetX) * .00075f, (Cell.P[0].Z + _offsetZ) * .00075f);
                if (_cacheColorSamples)
                {
                    _colorNoise[column] = noise;
                    _colorNoiseSet[column] = true;
                }
            }
            var regionColor = RegionColor;
            var hasGrassColor = _cacheColorSamples && ReferenceEquals(_grassRegions[column], RegionColor);
            var grassColor = hasGrassColor ? _grassColors[column] : Vector4.Zero;

            // The common interior case has all 27 material samples in the
            // fine cache. Resolve its bounds once, then walk rows directly in
            // the same x/z/y order as the neighbour sampler (including RNG).
            var cachedInterior = _cacheColorSamples && x > 0 && z > 0 && x < Chunk.BoundsX && z < Chunk.BoundsZ
                && y >= _cellFirstY + 2 && y + 2 < _cellFirstY + _cellHeight
                && y + 2 <= _boundsY - _sampleHeight - 1;
            if (cachedInterior && TryCountGrassSamples(x, y, z, out var grassCount))
            {
                if (!hasGrassColor)
                {
                    grassColor = DoGetColor(ref regionColor, ref x, ref z, BlockType.Grass, ref noise);
                    _grassRegions[column] = regionColor;
                    _grassColors[column] = grassColor;
                }
                // All contributing colors are identical. Preserve the original
                // repeated-add rounding while skipping 27 material/shading calls.
                var weighted = grassColor * 32;
                for (var i = 0; i < grassCount; i++) color += weighted;
                return new Vector4(color.Xyz() / (grassCount * 32), 1);
            }
            if (cachedInterior)
            {
                for (var dx = -1; dx <= 1; dx++)
                for (var dz = -1; dz <= 1; dz++)
                {
                    var sample = _cellSamples + (x + dx) * _cellHeight * CellWidth
                        + (y - 2 - _cellFirstY) * CellWidth + z + dz;
                    AddColorIfNecessary(sample->Type, x + dx, z + dz, ref regionColor, ref noise, ref color,
                        ref colorCount, ref grassColor, ref hasGrassColor);
                    AddColorIfNecessary(sample[2 * CellWidth].Type, x + dx, z + dz, ref regionColor, ref noise,
                        ref color, ref colorCount, ref grassColor, ref hasGrassColor);
                    AddColorIfNecessary(sample[4 * CellWidth].Type, x + dx, z + dz, ref regionColor, ref noise,
                        ref color, ref colorCount, ref grassColor, ref hasGrassColor);
                }
            }
            else
            {
                for (var _x = -1; _x < 1 + 1; _x += 1)
                for (var _z = -1; _z < 1 + 1; _z += 1)
                for (var _y = -2; _y < 1 + 2; _y += 2)
                    AddColorIfNecessary(x + _x, _y + y, z + _z, ref regionColor, ref noise, ref color, ref colorCount,
                        ref grassColor, ref hasGrassColor);
            }
            /* Try with a broader search */
            if (colorCount == 0)
                for (var _x = -_sampleWidth; _x < 1 + _sampleWidth; _x += _sampleWidth)
                for (var _z = -_sampleWidth; _z < 1 + _sampleWidth; _z += _sampleWidth)
                for (var _y = -_sampleHeight; _y < 1 + _sampleHeight; _y += _sampleHeight)
                    AddColorIfNecessary(x + _x, _y + y, z + _z, ref regionColor, ref noise, ref color, ref colorCount,
                        ref grassColor, ref hasGrassColor);
            
            if (_cacheColorSamples && hasGrassColor)
            {
                _grassRegions[column] = RegionColor;
                _grassColors[column] = grassColor;
            }
            return colorCount != 0 ? new Vector4(color.Xyz() / colorCount, 1.0f) : GetDefaultColor(RegionColor);
        }

        private bool TryCountGrassSamples(int X, int Y, int Z, out int Count)
        {
            var mask = 7UL << (Z - 1);
            Count = 0;
            for (var x = X - 1; x <= X + 1; x++)
            for (var y = Y - 2; y <= Y + 2; y += 2)
            {
                var row = x * _cellHeight + y - _cellFirstY;
                if ((_otherColorRows[row] & mask) != 0) return false;
                Count += BitOperations.PopCount(_grassRows[row] & mask);
            }
            return Count != 0;
        }

        private Vector4 GetDefaultColor(RegionColor RegionColor)
        {
            var shade = CalculateStoneShade(BlockType.Stone) * 0.5f;
            return Block.GetColor(BlockType.Stone, RegionColor) + new Vector4(shade, shade, shade, 0);
        }

        private void AddColorIfNecessary(int X, int Y, int Z, ref RegionColor RegionColor, ref float Noise,
            ref Vector4 Color, ref int ColorCount, ref Vector4 GrassColor, ref bool HasGrassColor)
        {
            var type = GetSampleTypeOrNeighbour(X, Y.Clamp0(), Z);
            AddColorIfNecessary(type, X, Z, ref RegionColor, ref Noise, ref Color, ref ColorCount,
                ref GrassColor, ref HasGrassColor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddColorIfNecessary(BlockType type, int X, int Z, ref RegionColor RegionColor, ref float Noise,
            ref Vector4 Color, ref int ColorCount, ref Vector4 GrassColor, ref bool HasGrassColor)
        {
            if (type != BlockType.Water && type != BlockType.Air && type != BlockType.None)
            {
                Vector4 blockColor;
                if (_cacheColorSamples && type == BlockType.Grass)
                {
                    if (!HasGrassColor)
                    {
                        GrassColor = DoGetColor(ref RegionColor, ref X, ref Z, type, ref Noise);
                        HasGrassColor = true;
                    }
                    blockColor = GrassColor;
                }
                else blockColor = DoGetColor(ref RegionColor, ref X, ref Z, type, ref Noise);
                Color += blockColor * 32;
                ColorCount += 32;
            }
        }

        private static Vector4 DoGetColor(ref RegionColor RegionColor, ref int x, ref int z, BlockType Type,
            ref float Noise)
        {
            var blockColor = Block.GetColor(Type, RegionColor);
            if (Type == BlockType.Grass)
            {
                var clampNoise = (Noise + 1) * .5f;
                var levelSize = 1.0f / RegionColor.GrassColors.Length;
                var nextIndex = (int)Math.Ceiling(clampNoise / levelSize);
                if (nextIndex == RegionColor.GrassColors.Length) nextIndex = 0;

                Vector4 A = RegionColor.GrassColors[(int)Math.Floor(clampNoise / levelSize)],
                    B = RegionColor.GrassColors[nextIndex];

                var delta = clampNoise / levelSize - (float)Math.Floor(clampNoise / levelSize);
                blockColor = Mathf.Lerp(A, B, delta);
            }
            else if (Type == BlockType.Stone || Type == BlockType.StonePath)
            {
                var shade = CalculateStoneShade(Type);
                blockColor *= (1 - shade);
            }
            else if (Type == BlockType.FarmDirt)
            {
                if ((x + 1) % 2 == 0)
                    blockColor -= new Vector4(.1f, .1f, .1f, 0);
            }

            return blockColor;
        }

        private static float CalculateStoneShade(BlockType Type)
        {
            return (Utils.Rng.NextFloat() * 2 - 1f) * .2f * (Type == BlockType.StonePath ? 3f : 1.5f);
        }

        private void BuildDensityGrid(int Lod)
        {
            for (var x = 0; x < noiseValuesMapWidth; x++)
            for (var y = 0; y < noiseValuesMapHeight; y++)
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
                        if (_y == 0 || y + _y < 0 || y + _y >= _boundsY) continue;
                        var block = GetNeighbourBlock(x * _sampleWidth, (y + _y) * _sampleHeight, z * _sampleWidth);
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

        public void CacheCellSamples(SampledBlock* Samples, int FirstY, int LastY, int HorizontalStep, int VerticalStep,
            bool BuildSurfaceMasks = true)
        {
            _cellSamples = Samples;
            _cellFirstY = FirstY;
            _cellHeight = LastY - FirstY + VerticalStep + 1;
            _cacheColorSamples = BuildSurfaceMasks && HorizontalStep == 1 && VerticalStep == 1;
            if (_cacheColorSamples)
            {
                _colorNoise = new float[Chunk.BoundsX * Chunk.BoundsZ];
                _colorNoiseSet = new bool[_colorNoise.Length];
                _grassRegions = new RegionColor[_colorNoise.Length];
                _grassColors = new Vector4[_colorNoise.Length];
                var rows = CellWidth * _cellHeight;
                _grassRows = ArrayPool<ulong>.Shared.Rent(rows);
                _otherColorRows = ArrayPool<ulong>.Shared.Rent(rows);
                Array.Clear(_grassRows, 0, rows);
                Array.Clear(_otherColorRows, 0, rows);
            }
            _cellSignRows = BuildSurfaceMasks ? new ulong[CellWidth * _cellHeight] : null;
            _cellStrideMask = 0;
            for (var z = 0; z < Chunk.BoundsZ; z += HorizontalStep) _cellStrideMask |= 1u << z;
            for (var x = 0; x <= Chunk.BoundsX; x += HorizontalStep)
            for (var y = FirstY; y <= LastY + VerticalStep; y += VerticalStep)
            for (var z = 0; z <= Chunk.BoundsZ; z += HorizontalStep)
            {
                var density = GetSampleOrNeighbour(x, y, z, out var type);
                Samples[x * _cellHeight * CellWidth + (y - FirstY) * CellWidth + z] =
                    new SampledBlock { Density = density, Type = type };
                if (BuildSurfaceMasks && density > 0)
                    _cellSignRows[x * _cellHeight + y - FirstY] |= 1UL << z;
                if (_cacheColorSamples)
                {
                    var row = x * _cellHeight + y - FirstY;
                    if (type == BlockType.Grass) _grassRows[row] |= 1UL << z;
                    else if (type != BlockType.Air && type != BlockType.Water && type != BlockType.None)
                        _otherColorRows[row] |= 1UL << z;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetSurfaceMask(int X, int Y, int HorizontalStep, int VerticalStep)
        {
            if (_cellSignRows == null) return _cellStrideMask;
            var row = X * _cellHeight + Y - _cellFirstY;
            var right = row + HorizontalStep * _cellHeight;
            return SurfaceCellMask.FromRows(_cellSignRows[row], _cellSignRows[right],
                _cellSignRows[row + VerticalStep], _cellSignRows[right + VerticalStep], HorizontalStep, _cellStrideMask);
        }

        public void CacheWaterSamples(int FirstY, int LastY)
        {
            _waterFirstY = FirstY;
            _waterHeight = LastY - FirstY + 2;
            _waterTypes = ArrayPool<BlockType>.Shared.Rent(CellWidth * _waterHeight * CellWidth);
            _waterSignRows = new ulong[CellWidth * _waterHeight];
            for (var x = 0; x <= Chunk.BoundsX; x++)
            for (var y = FirstY; y <= LastY + 1; y++)
            for (var z = 0; z <= Chunk.BoundsZ; z++)
            {
                // Water uses original blocks even when terrain is interpolated at
                // a coarser LOD, and does not clamp the top samples like terrain.
                var type = GetNeighbourBlock(x, y, z).Type;
                _waterTypes[(x * _waterHeight + y - FirstY) * CellWidth + z] = type;
                if (type == BlockType.Water) _waterSignRows[x * _waterHeight + y - FirstY] |= 1UL << z;
            }
        }

        public uint GetWaterSurfaceMask(int X, int Y)
        {
            var row = X * _waterHeight + Y - _waterFirstY;
            var right = row + _waterHeight;
            return SurfaceCellMask.FromRows(_waterSignRows[row], _waterSignRows[right],
                _waterSignRows[row + 1], _waterSignRows[right + 1], 1, uint.MaxValue);
        }

        private bool TryCreateWaterCell(ref GridCell Cell, int X, int Y, int Z, out bool Success)
        {
            var index = (X * _waterHeight + Y - _waterFirstY) * CellWidth + Z;
            var dx = _waterHeight * CellWidth;
            SetWaterCellSample(ref Cell, 0, _waterTypes[index]);
            SetWaterCellSample(ref Cell, 1, _waterTypes[index + dx]);
            SetWaterCellSample(ref Cell, 2, _waterTypes[index + dx + 1]);
            SetWaterCellSample(ref Cell, 3, _waterTypes[index + 1]);
            SetWaterCellSample(ref Cell, 4, _waterTypes[index + CellWidth]);
            SetWaterCellSample(ref Cell, 5, _waterTypes[index + dx + CellWidth]);
            SetWaterCellSample(ref Cell, 6, _waterTypes[index + dx + CellWidth + 1]);
            SetWaterCellSample(ref Cell, 7, _waterTypes[index + CellWidth + 1]);
            Success = true;
            for (var i = 0; i < 8; i++)
                if (Cell.Type[i] == BlockType.None && Y < _height - 2)
                {
                    Success = false;
                    Cell.Type[i] = BlockType.Air;
                }
            BuildCell(ref Cell, X, Y, Z, 1, 1);
            var usable = MarchingCubes.Usable(Cell);
            if (VerifyCells)
            {
                var reference = _referenceCell;
                CreateCell(ref reference, X, Y, Z, true, 1, 1, out var referenceSuccess);
                if (referenceSuccess != Success || MarchingCubes.Usable(reference) != usable)
                    throw new InvalidOperationException("Cached water cell classification differs from reference");
                for (var i = 0; i < 8; i++)
                    if (reference.Type[i] != Cell.Type[i] || reference.Density[i] != Cell.Density[i] || reference.P[i] != Cell.P[i])
                        throw new InvalidOperationException($"Cached water cell differs at {X},{Y},{Z} corner {i}");
            }
            return usable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetWaterCellSample(ref GridCell Cell, int Corner, BlockType Type)
        {
            Cell.Type[Corner] = Type;
            Cell.Density[Corner] = Type == BlockType.Water ? 1 : -1;
        }

        public void Dispose()
        {
            if (_waterTypes != null) ArrayPool<BlockType>.Shared.Return(_waterTypes);
            if (_grassRows != null) ArrayPool<ulong>.Shared.Return(_grassRows);
            if (_otherColorRows != null) ArrayPool<ulong>.Shared.Return(_otherColorRows);
            _waterTypes = null;
            _grassRows = null;
            _otherColorRows = null;
        }

        public bool TryCreateCell(ref GridCell Cell, int X, int Y, int Z, bool IsWater, int HorizontalStep,
            int VerticalStep, out bool Success)
        {
            if (IsWater && _waterTypes != null && HorizontalStep == 1 && VerticalStep == 1)
                return TryCreateWaterCell(ref Cell, X, Y, Z, out Success);
            if (IsWater || _cellSamples == null)
            {
                CreateCell(ref Cell, X, Y, Z, IsWater, HorizontalStep, VerticalStep, out Success);
                return MarchingCubes.Usable(Cell);
            }

            var index = X * _cellHeight * CellWidth + (Y - _cellFirstY) * CellWidth + Z;
            var dx = HorizontalStep * _cellHeight * CellWidth;
            var dy = VerticalStep * CellWidth;
            var dz = HorizontalStep;
            var signs = 0;
            if (_cellSamples[index].Density > 0) signs |= 1;
            if (_cellSamples[index + dx].Density > 0) signs |= 2;
            if (_cellSamples[index + dx + dz].Density > 0) signs |= 4;
            if (_cellSamples[index + dz].Density > 0) signs |= 8;
            if (_cellSamples[index + dy].Density > 0) signs |= 16;
            if (_cellSamples[index + dx + dy].Density > 0) signs |= 32;
            if (_cellSamples[index + dx + dy + dz].Density > 0) signs |= 64;
            if (_cellSamples[index + dy + dz].Density > 0) signs |= 128;
            var usable = signs != 0 && signs != 255;
            // The caller ignores missing samples in cells without a surface.
            Success = true;
            if (!usable && !VerifyCells) return false;
            SetCellSample(ref Cell, 0, _cellSamples[index]);
            SetCellSample(ref Cell, 1, _cellSamples[index + dx]);
            SetCellSample(ref Cell, 2, _cellSamples[index + dx + dz]);
            SetCellSample(ref Cell, 3, _cellSamples[index + dz]);
            SetCellSample(ref Cell, 4, _cellSamples[index + dy]);
            SetCellSample(ref Cell, 5, _cellSamples[index + dx + dy]);
            SetCellSample(ref Cell, 6, _cellSamples[index + dx + dy + dz]);
            SetCellSample(ref Cell, 7, _cellSamples[index + dy + dz]);
            for (var i = 0; i < 8; i++)
                if (Cell.Type[i] == BlockType.None && Y < _height - 2)
                {
                    Success = false;
                    Cell.Type[i] = BlockType.Air;
                }
            if (usable) BuildCell(ref Cell, X, Y, Z, HorizontalStep, VerticalStep);
            if (VerifyCells)
            {
                var reference = _referenceCell;
                CreateCell(ref reference, X, Y, Z, false, HorizontalStep, VerticalStep, out var referenceSuccess);
                if (referenceSuccess != Success || MarchingCubes.Usable(reference) != usable)
                    throw new InvalidOperationException("Cached mesh cell classification differs from reference");
                for (var i = 0; i < 8; i++)
                    if (reference.Type[i] != Cell.Type[i] || reference.Density[i] != Cell.Density[i] ||
                        usable && reference.P[i] != Cell.P[i])
                        throw new InvalidOperationException($"Cached mesh cell differs at {X},{Y},{Z} corner {i}");
            }
            return usable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetCellSample(ref GridCell Cell, int Corner, SampledBlock Sample)
        {
            Cell.Density[Corner] = Sample.Density;
            Cell.Type[Corner] = Sample.Type;
        }

        public void CreateCell(ref GridCell Cell, in int X, in int Y, in int Z, bool isWaterCell, int HorizontalLod,
            int VerticalLod, out bool Success)
        {
            Success = true;
            BuildCell(ref Cell, X, Y, Z, HorizontalLod, VerticalLod);

            for (var i = 0; i < Cell.Type.Length; i++)
            {
                var posX = (int)(Cell.P[i].X * _coefficient);
                var posY = (int)(Cell.P[i].Y * _coefficient);
                var posZ = (int)(Cell.P[i].Z * _coefficient);

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
                if (Cell.Type[i] == BlockType.None && Y < _height - 2)
                {
                    Success = false;
                    Cell.Type[i] = BlockType.Air;
                }
        }

        private float GetSampleOrNeighbour(int x, int y, int z, out BlockType Type)
        {
            y = Math.Min(y, _boundsY - _sampleHeight - 1);
            if (x <= 0 || z <= 0 || y <= 4 || x >= _boundsX - 1 || z >= _boundsZ - 1 ||
                _sampleWidth == 1 && _sampleHeight == 1)
            {
                var b = GetNeighbourBlock(x, y, z);
                Type = b.Type;
                return b.Density;
            }

            return GetSample(x, y, z, out Type);
        }

        private BlockType GetSampleTypeOrNeighbour(int x, int y, int z)
        {
            y = Math.Min(y, _boundsY - _sampleHeight - 1);
            if (_cacheColorSamples && (uint)x < CellWidth && (uint)z < CellWidth && (uint)(y - _cellFirstY) < _cellHeight)
                return _cellSamples[x * _cellHeight * CellWidth + (y - _cellFirstY) * CellWidth + z].Type;
            if (x <= 0 || z <= 0 || y <= 4 || x >= _boundsX - 1 || z >= _boundsZ - 1 || _sampleWidth == 1)
                return GetNeighbourBlock(x, y, z).Type;
            return Get((int)(x * _invSampleWidth), (int)(y * _invSampleHeight), (int)(z * _invSampleWidth)).Type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SampledBlock Get(int _x, int _y, int _z)
        {
            return _grid[_x * noiseValuesMapWidth * noiseValuesMapHeight + _y * noiseValuesMapWidth + _z];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetSample(int x, int y, int z, out BlockType Type)
        {
            var x2 = (int)(x * _invSampleWidth);
            var y2 = (int)(y * _invSampleHeight);
            var z2 = (int)(z * _invSampleWidth);

            var b0 = Get(x2, y2, z2);
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
                x % _sampleWidth * _invSampleWidth,
                y % _sampleHeight * _invSampleHeight,
                z % _sampleWidth * _invSampleWidth
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildCell(ref GridCell Cell, int X, int Y, int Z, int HorizontalLod, int VerticalLod)
        {
            var horizontalBlockSize = _blockSize * HorizontalLod;
            var verticalBlockSize = _blockSize * VerticalLod;
            var baseOffset = new Vector3(X, Y, Z) * _blockSize;
            
            Cell.P[0] = baseOffset;
            Cell.P[1] = new Vector3(horizontalBlockSize, 0, 0) + baseOffset;
            Cell.P[2] = new Vector3(horizontalBlockSize, 0, horizontalBlockSize) + baseOffset;
            Cell.P[3] = new Vector3(0, 0, horizontalBlockSize) + baseOffset;
            Cell.P[4] = new Vector3(0, verticalBlockSize, 0) + baseOffset;
            Cell.P[5] = new Vector3(horizontalBlockSize, verticalBlockSize, 0) + baseOffset;
            Cell.P[6] = new Vector3(horizontalBlockSize, verticalBlockSize, horizontalBlockSize) + baseOffset;
            Cell.P[7] = new Vector3(0, verticalBlockSize, horizontalBlockSize) + baseOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Chunk GetNeighbourChunk(ref int X, ref int Z)
        {
            var nx = (X >> 5) + 1;
            var nz = (Z >> 5) + 1;
            if ((uint)nx < 3 && (uint)nz < 3) return _neighbours[nx * 3 + nz];
            var offset = new Vector2(((int)(_offsetX + X * Chunk.BlockSize) >> 7) << 7,
                ((int)(_offsetZ + Z * Chunk.BlockSize) >> 7) << 7);
            if ((int)offset.X == _offsetX && (int)offset.Y == _offsetZ) return _parent;
            World.SearcheableChunksReference.TryGetValue(offset, out var chunk);
            return chunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Block GetNeighbourBlock(int X, int Y, int Z)
        {
            return GetNeighbourBlock(ref X, ref Y, ref Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Block GetNeighbourBlock(ref int X, ref int Y, ref int Z)
        {
            var chunk = GetNeighbourChunk(ref X, ref Z);
            if (!chunk?.Landscape.BlocksSetted ?? true) return new Block(BlockType.None);
            return chunk[Modulo(ref X) * _boundsY * _boundsZ + Y * _boundsZ + Modulo(ref Z)];
        }

        // Source: https://codereview.stackexchange.com/a/58309
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Modulo(ref int Index)
        {
            return (Index & (Bounds-1) + Bounds) & (Bounds - 1);
        }
    }
}
