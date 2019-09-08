using System;
using System.Runtime.CompilerServices;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
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

        public Vector4 GetColor(ref GridCell Cell, RegionColor RegionColor, int Lod, ref Vector3 AverageNormal)
        {
            var position = Cell.P[0] / _blockSize;
            int x = (int) position.X, y = (int) position.Y, z = (int) position.Z;
            var color = Vector4.Zero;
            var colorCount = 0;
            var dot = Vector3.Dot(AverageNormal, Vector3.UnitY);
            const float dotThreshold = 0.7f;
            var canBeGrass = dot > 0.7f;
            var noise = (float) World.GetNoise((Cell.P[0].X + _offsetX) * .00075f, (Cell.P[0].Z + _offsetZ) * .00075f);
            var regionColor = RegionColor;

            for (var _x = -Lod * 1; _x < 1 * Lod + 1; _x += Lod)
            for (var _z = -Lod * 1; _z < 1 * Lod + 1; _z += Lod)
            for (var _y = -1; _y < 1 + 1; _y++)
            {
                var y0 = this.GetNeighbourBlock(x + _x, (int) Mathf.Clamp(y + _y, 0, this._height - 1), z + _z);
                var type = y0.Type;
                if (type != BlockType.Water && type != BlockType.Air && type != BlockType.Temporal)
                {
                    var blockColor = DoGetColor(ref regionColor, ref x, type, ref noise);
                    /*if (!canBeGrass && y0.Type == BlockType.Grass)
                    {
                        blockColor = Mathf.Lerp(DoGetColor(ref regionColor, ref x, BlockType.Stone, ref noise), blockColor, (dot).Clamp01());
                    }*/

                    color += blockColor;
                    colorCount++;
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
            return new Vector4(colorCount == 0 ? regionColor.DirtColor.Xyz : color.Xyz / colorCount, 1.0f);
        }

        private static Vector4 DoGetColor(ref RegionColor RegionColor, ref int x, BlockType Type, ref float Noise)
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
                var shade = (Utils.Rng.NextFloat() * 2 - 1f) * .2f * (Type == BlockType.Stone ? .65f : 1f);
                blockColor += new Vector4(shade, shade, shade, 0); 
            }
            else if (Type == BlockType.FarmDirt)
            {
                if((x+1) % 2 == 0)
                    blockColor -= new Vector4(.1f, .1f, .1f, 0);
            }

            return blockColor;
        }

        public void CreateCell(ref GridCell Cell, ref int X, ref int Y, ref int Z, ref bool WaterCell, ref int Lod, out bool Success)
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
                    
                    var block = GetNeighbourBlock(posX, posY, posZ);
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
                    Block waterBlock = GetBlock(ref pos.X, ref pos.Y, ref pos.Z);

                    if (waterBlock.Type != BlockType.Water)
                    {
                        for (var k = pos.Y - 3; k < _boundsY; k++)
                        {
                            waterBlock = GetBlock(ref pos.X, ref k, ref pos.Z);
                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }

                        for (var k = pos.Y - 3; k < _boundsY; k++)
                        for (var kx = -2; kx < 3; kx++)
                        for (var kz = -2; kz < 3; kz++)
                        {
                            var waterX = pos.X + kx;
                            var waterZ = pos.Z + kz;
                            waterBlock = GetBlock(ref waterX, ref k, ref waterZ);


                            if (waterBlock.Type == BlockType.Water) goto WATER_BREAK;
                        }
                    }
                    WATER_BREAK:
 
                    var neighbourChunk = GetNeighbourChunk(ref pos.X, ref pos.Z, ref _offsetX, ref _offsetZ);
                    var x = (int) (pos.X % _boundsX);
                    var z = (int) (pos.Z % _boundsZ);

                    var newHeight = neighbourChunk?.GetWaterDensity(new Vector3(x, Y, z)) ?? default(Half);
                    for (int k = Math.Min(Y + 8, Chunk.Height - 1); k > -1 && Math.Abs(newHeight) < 0.005f; k--)
                    {
                        newHeight = neighbourChunk?.GetWaterDensity(new Vector3(x, k, z)) ?? default(Half);
                    }
                    for (int k = Math.Min(Y + 8, Chunk.Height - 1); k > -1; k--)
                    {
                        var block = neighbourChunk[x][k]?[z] ?? new Block();
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

        private Block GetBlock(ref float X, ref float Y, ref float Z)
        {
            var intX = (int) X;
            var intY = (int) Y;
            var intZ = (int) Z;
            if (X > 0 && X < _boundsX && Z > 0 && Z < _boundsZ) return _parent[intX][intY][intZ];
            return GetNeighbourBlock(ref intX, ref intY, ref intZ, ref _offsetX, ref _offsetZ);
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
