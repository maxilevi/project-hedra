using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkStructuresMeshBuilder
    {
        private readonly Chunk _parent;

        public ChunkStructuresMeshBuilder(Chunk Parent)
        {
            _parent = Parent;
        }

        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private int BuildedLod => _parent.BuildedLod;
        private bool Disposed => _parent.Disposed;
        private ChunkMesh Mesh => _parent.Mesh;

        public ChunkMeshBuildOutput AddStructuresMeshes(ChunkMeshBuildOutput Input, int Lod)
        {
            List<VertexData> staticElements;
            try
            {
                staticElements = _parent.StaticElements.ToList();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.WriteLine(e.Message);
                return null;
            }
            for (var i = 0; i < staticElements.Count; i++)
            {
                if (staticElements[i].Extradata.Count != staticElements[i].Vertices.Count)
                {
                    float extraDataCount = staticElements[i].Vertices.Count - staticElements[i].Extradata.Count;
                    for (var k = 0; k < extraDataCount; k++) staticElements[i].Extradata.Add(0);
                }

                Input.StaticData += staticElements[i];
            }

            var instanceElements = Mesh.InstanceElements;
            for (var i = 0; i < instanceElements.Length; i++)
            {
                if (Lod != 1
                    && (instanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Grass)
                        || instanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Wheat)))
                    continue;

                var model = instanceElements[i].MeshCache.Clone();
                if (instanceElements[i].ColorCache != -Vector4.One &&
                    CacheManager.CachedColors.ContainsKey(instanceElements[i].ColorCache))
                    model.Colors = CacheManager.CachedColors[instanceElements[i].ColorCache].Clone();
                else
                    model.Colors = instanceElements[i].Colors;

                var variateFactor = (new Random(OffsetX + OffsetZ + World.Seed + i).NextFloat() * 2f - 1f) *
                                      (24 / 256f);
                for (var l = 0; l < model.Colors.Count; l++)
                    model.Colors[l] += new Vector4(variateFactor, variateFactor, variateFactor, 0);

                if (CacheManager.CachedExtradata.ContainsKey(instanceElements[i].ExtraDataCache))
                    model.Extradata = CacheManager.CachedExtradata[instanceElements[i].ExtraDataCache]
                        .Clone();
                else
                    model.Extradata = instanceElements[i].ExtraData;

                if (instanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Grass) ||
                    instanceElements[i].MeshCache == CacheManager.GetModel(CacheItem.Wheat))
                {
                    var instancePosition = instanceElements[i].TransMatrix.ExtractTranslation();
                    var grassRng = new Random((int)(instancePosition.X * instancePosition.Z));
                    instancePosition += Vector3.UnitY * (grassRng.NextFloat() * .2f - .2f);
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
                                           (grassRng.NextFloat() * .4f);
                        shadow.DeleteWhen = () => BuildedLod != 1 || Disposed;
                    }
                }

                model.Transform(Mesh.InstanceElements[i].TransMatrix);
                //Pack some randomness to the wind values
                float rng = Utils.Rng.NextFloat();
                for (var k = 0; k < model.Extradata.Count; k++)
                {
                    if (model.Extradata[k] != 0 && model.Extradata[k] != -10f)
                        model.Extradata[k] = Mathf.Pack(new Vector2(model.Extradata[k], rng), 2048);

                    if (model.Extradata[k] == -10f)
                        model.Extradata[k] = -1f;
                }

                //StaticBuffer.VData += Model;
                //Manually add these vertex data's for maximum performance
                for (var k = 0; k < model.Indices.Count; k++)
                    model.Indices[k] += (uint)Input.StaticData.Vertices.Count;
                Input.StaticData.Vertices.AddRange(model.Vertices);
                Input.StaticData.Colors.AddRange(model.Colors);
                Input.StaticData.Normals.AddRange(model.Normals);
                Input.StaticData.Indices.AddRange(model.Indices);
                Input.StaticData.Extradata.AddRange(model.Extradata);

                model.Dispose();
            }
            return new ChunkMeshBuildOutput(Input.StaticData, Input.WaterData, Input.Failed, Input.HasNoise3D, Input.HasWater);
        }
    }
}
