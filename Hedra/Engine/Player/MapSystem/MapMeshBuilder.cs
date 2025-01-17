using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapMeshBuilder
    {
        private readonly int _chunkSize;
        private readonly CubeData _cubeData;
        private readonly CubeData _fullData;
        private readonly int _mapSize;
        private readonly IPlayer _player;

        public MapMeshBuilder(IPlayer Player, int MapSize, int ChunkSize)
        {
            _player = Player;
            _mapSize = MapSize;
            _chunkSize = ChunkSize;

            _cubeData = new CubeData();
            _cubeData.Scale(new Vector3(_chunkSize, _chunkSize * 3, _chunkSize));
            _cubeData.AddFace(Face.UP);

            _fullData = new CubeData();
            _fullData.Scale(new Vector3(_chunkSize, _chunkSize * 3, _chunkSize));
            _fullData.AddFace(Face.FRONT);
            _fullData.AddFace(Face.BACK);
            _fullData.AddFace(Face.RIGHT);
            _fullData.AddFace(Face.LEFT);
            _fullData.AddFace(Face.UP);
        }

        public MapBaseItem BuildItem(Vector2 Offset)
        {
            var mapData = new VertexData();
            var item = new MapBaseItem(_mapSize);
            for (var x = 0; x < _mapSize; x++)
            for (var z = 0; z < _mapSize; z++)
            {
                var rng = new Random((int)(1000f * (Offset.X / 13f + (int)Offset.Y / 7f + x / 13f + z / 7f)));
                var dataPiece = new VertexData();
                var realX = (x - _mapSize / 2f) * _chunkSize;
                var realZ = (z - _mapSize / 2f) * _chunkSize;
                var playerPos = World.ToChunkSpace(_player.Position);
                var chunkPosition = playerPos + Offset * Chunk.Width * _mapSize
                                              + new Vector2((x - _mapSize / 2) * Chunk.Width,
                                                  (z - _mapSize / 2) * Chunk.Width);
                var region = World.BiomePool.GetRegion(chunkPosition.ToVector3());
                var chunk = World.GetChunkByOffset(chunkPosition);
                var useChunkMesh = MapBaseItem.UsableChunk(chunk);
                var useFullCube = !IsNeighbourEqual(chunkPosition, useChunkMesh);
                if (!useChunkMesh)
                {
                    var blockColor = Utils.UniformVariateColor(region.Colors.GrassColor, 25, rng);
                    item.HasChunk[x * _mapSize + z] = false;
                    var cubeData = useFullCube ? _fullData.Clone() : _cubeData.Clone();
                    BlockType type;
                    cubeData.Scale(new Vector3(1, 2, 1));
                    cubeData.TransformVerts(new Vector3(realX, 0, realZ));
                    cubeData.Color = CubeData.CreateCubeColor(blockColor);

                    dataPiece = new VertexData
                    {
                        Normals = cubeData.Normals.ToList(),
                        Vertices = cubeData.VerticesArrays.ToArray().ToList(),
                        Indices = cubeData.Indices,
                        Colors = cubeData.Color.ToArray().ToList()
                    };
                    dataPiece.Translate(Vector3.UnitY * -20f);
                }
                else
                {
                    item.HasChunk[x * _mapSize + z] = true;
                }

                dataPiece.Translate(Offset.ToVector3() * _chunkSize * _mapSize);
                if (!useChunkMesh) mapData += dataPiece;
            }

            var baseMesh = ObjectMesh.FromVertexData(mapData);
            baseMesh.ApplyNoiseTexture = true;
            baseMesh.Dither = true;
            DrawManager.RemoveObjectMesh(baseMesh);
            item.Mesh = baseMesh;
            item.WasBuilt = true;
            return item;
        }

        private bool IsNeighbourEqual(Vector2 ChunkPosition, bool Condition)
        {
            for (var x = -1; x < 2; x++)
            for (var z = -1; z < 2; z++)
            {
                if (Math.Abs(z) == Math.Abs(x)) continue;
                var chunk = World.GetChunkByOffset(ChunkPosition + new Vector2(Chunk.Width * x, Chunk.Width * z));
                if (MapBaseItem.UsableChunk(chunk) != Condition) return false;
            }

            return true;
        }
    }
}