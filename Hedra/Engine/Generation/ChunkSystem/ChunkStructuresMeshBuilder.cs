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
        public bool HasLodedElements { get; set; }
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
                ProcessInstanceData(instanceElements[i], Input.StaticData, i);
            }

            if (Lod == 1)
            {
                var lodedInstanceElements = Mesh.LodAffectedInstanceElements;
                for (var i = 0; i < lodedInstanceElements.Length; i++)
                {
                    ProcessInstanceData(lodedInstanceElements[i], Input.InstanceData, i);
                }
            }

            return new ChunkMeshBuildOutput(Input.StaticData, Input.WaterData, Input.InstanceData, Input.Failed, Input.HasNoise3D, Input.HasWater);
        }

        private void ProcessInstanceData(InstanceData Element, VertexData Model, int Index)
        {
            var model = Element.MeshCache.Clone();
            if (Element.ColorCache != -Vector4.One &&
                CacheManager.CachedColors.ContainsKey(Element.ColorCache))
                model.Colors = CacheManager.CachedColors[Element.ColorCache].Clone();
            else
                model.Colors = Element.Colors;

            var variateFactor = (new Random(OffsetX + OffsetZ + World.Seed + Index).NextFloat() * 2f - 1f) *
                                  (24 / 256f);
            for (var l = 0; l < model.Colors.Count; l++)
                model.Colors[l] += new Vector4(variateFactor, variateFactor, variateFactor, 0);

            if (CacheManager.CachedExtradata.ContainsKey(Element.ExtraDataCache))
                model.Extradata = CacheManager.CachedExtradata[Element.ExtraDataCache].Clone();
            else
                model.Extradata = Element.ExtraData;
/*
            if (Element.MeshCache == CacheManager.GetModel(CacheItem.Grass) ||
                Element.MeshCache == CacheManager.GetModel(CacheItem.Wheat))
            {
                var instancePosition = Element.TransMatrix.ExtractTranslation();
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
            }*/

            model.Transform(Element.TransMatrix);
            //Pack some randomness to the wind values
            float rng = Utils.Rng.NextFloat();
            for (var k = 0; k < model.Extradata.Count; k++)
            {
                if (model.Extradata[k] != 0 && model.Extradata[k] != -10f)
                    model.Extradata[k] = Mathf.Pack(new Vector2(model.Extradata[k], rng), 2048);

                if (model.Extradata[k] == -10f)
                    model.Extradata[k] = -1f;
            }

            //Manually add these vertex data's for maximum performance
            for (var k = 0; k < model.Indices.Count; k++)
                model.Indices[k] += (uint) Model.Vertices.Count;

            if (model.Extradata.Count != model.Colors.Count)
                throw new ArgumentOutOfRangeException("Extradata or color mismatch");
            for (var i = 0; i < model.Extradata.Count; i++)
            {
                model.Colors[i] = new Vector4(model.Colors[i].Xyz, model.Extradata[i]);
            }

            Model.Vertices.AddRange(model.Vertices);
            Model.Colors.AddRange(model.Colors);
            Model.Normals.AddRange(model.Normals);
            Model.Indices.AddRange(model.Indices);
            Model.Extradata.AddRange(model.Extradata);
            model.Dispose();
        }
    }
}
