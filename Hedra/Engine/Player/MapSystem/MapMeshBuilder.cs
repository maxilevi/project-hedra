using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapMeshBuilder
    {
        private readonly LocalPlayer _player;
        private readonly int _mapSize;
        private readonly int _chunkSize;
        private readonly CubeData _cubeData;

        public MapMeshBuilder(LocalPlayer Player, int MapSize, int ChunkSize)
        {
            _player = Player;
            _mapSize = MapSize;
            _chunkSize = ChunkSize;

            _cubeData = new CubeData();
            _cubeData.Scale(new Vector3(_chunkSize, _chunkSize * 2f, _chunkSize));
            _cubeData.AddFace(Face.UP);
            _cubeData.AddFace(Face.FRONT);
            _cubeData.AddFace(Face.BACK);
            _cubeData.AddFace(Face.RIGHT);
            _cubeData.AddFace(Face.LEFT);
        }

        public ObjectMesh BuildMesh(Vector2 Offset)
        {
            var mapData = new VertexData();
            var chunkData = new VertexData();
            for (var x = 0; x < _mapSize; x++)
            {
                for (var z = 0; z < _mapSize; z++)
                {
                    VertexData dataPiece;
                    var realX = (x - _mapSize / 2f) * _chunkSize;
                    var realZ = (z - _mapSize / 2f) * _chunkSize;
                    var playerPos = World.ToChunkSpace(_player.Position);
                    var chunkPosition = playerPos + Offset * Chunk.Width * _mapSize
                                        + new Vector2((x - _mapSize / 2f) * Chunk.Width, (z - _mapSize / 2f) * Chunk.Width);
                    var chunk = World.GetChunkByOffset(chunkPosition);
                    var region = World.BiomePool.GetRegion(chunkPosition.ToVector3());
                    var useChunkMesh = chunk != null && chunk.Landscape.StructuresPlaced && chunk.NeighboursExist;
                    if (useChunkMesh)
                    {
                        var output = chunk.CreateTerrainMesh(chunk.NeighbourChunks, 8);
                        output = chunk.AddStructuresMeshes(output);
                        output.StaticData.Transform(-new Vector3(chunk.OffsetX, 0, chunk.OffsetZ));
                        output.StaticData.Scale(Vector3.One * (_chunkSize / (float) Chunk.Width));
                        output.StaticData.Transform(new Vector3(realX, 0, realZ));
                        dataPiece = output.StaticData;
                    }
                    else
                    {
                        var cubeData = _cubeData.Clone();
                        BlockType type;
                        var lerpedHeight = Mathf.Lerp(
                            region.Generation.GetHeight(chunkPosition.X, chunkPosition.Y, null, out type) * 2.0f, 0,
                            Mathf.Clamp( (playerPos - chunkPosition).LengthFast / Chunk.Width / _mapSize - 2f, 0, 1));
                        cubeData.TransformVerts(new Vector3(realX, Chunk.BaseHeight * 2.0f + lerpedHeight, realZ));

                        cubeData.Color = CubeData.CreateCubeColor(Utils.UniformVariateColor(Color.DodgerBlue.ToVector4() * .9f, 25, Utils.Rng));
                        
                        dataPiece = new VertexData
                        {
                            Normals = cubeData.Normals.ToList(),
                            Vertices = cubeData.VerticesArrays.ToArray().ToList(),
                            Indices = cubeData.Indices,
                            Colors = cubeData.Color.ToArray().ToList()
                        };
                        dataPiece.Transform(Vector3.UnitY * -20f);
                    }
                    dataPiece.Transform(Offset.ToVector3() * _chunkSize * _mapSize + Vector3.UnitY * (-Chunk.BaseHeight * 2.0f - _chunkSize * 6f) );
                    if (useChunkMesh) chunkData += dataPiece;
                    else mapData += dataPiece;
                }
            }
            var baseMesh = ObjectMesh.FromVertexData(mapData + chunkData);
            baseMesh.DontCull = true;
            baseMesh.ApplyNoiseTexture = true;
            return baseMesh;
        }
    }
}
/*
 * var array = new[]
                        {
                            new Vector3(chunkPosition.X + Chunk.Width, 0, chunkPosition.Y+ Chunk.Width),
                            new Vector3(chunkPosition.X + Chunk.Width, 0, chunkPosition.Y),
                            new Vector3(chunkPosition.X, 0, chunkPosition.Y),
                            new Vector3(chunkPosition.X, 0, chunkPosition.Y),
                            new Vector3(chunkPosition.X, 0, chunkPosition.Y + Chunk.Width),
                            new Vector3(chunkPosition.X + Chunk.Width, 0, chunkPosition.Y + Chunk.Width)
                        };
                        cubeData.Color = CubeData.CreateCubeColor(region.Colors.GrassColor);
                        /*for (var i = 0; i < cubeData.Indices.Count; i++)
                        {
                            BlockType type;
                            cubeData.VerticesArrays[cubeData.Indices[i]].Y =
                                region.Generation.GetHeight(
                                    array[i].X,
                                    array[i].Z,
                                    null, out type
                                    ) + Chunk.BaseHeight * 1.5f;
                            var noise = (float)OpenSimplexNoise.Evaluate(array[i].X * .00075f, array[i].Z * .00075f);
                            cubeData.Color[cubeData.Indices[i]] = region.Colors.GrassColor * (noise + 1f);
                        }*/
//cubeData.RecalculateNormals();