using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
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
            var distribution = new RandomDistribution(); 
            var instanceElements = Mesh.InstanceElements;
            for (var i = 0; i < instanceElements.Length; i++)
            {
                if(instanceElements[i].SkipOnLod && Lod != 1) continue; 
                ProcessInstanceData(instanceElements[i], Input.StaticData, i, Lod, distribution);
            }

            if (Lod == 1)
            {
                var lodedInstanceElements = Mesh.LodAffectedInstanceElements;
                for (var i = 0; i < lodedInstanceElements.Length; i++)
                {
                    ProcessInstanceData(lodedInstanceElements[i], Input.InstanceData, i, Lod, distribution);
                }
            }

            return new ChunkMeshBuildOutput(Input.StaticData, Input.WaterData, Input.InstanceData, Input.Failed, Input.HasWater);
        }

        private void ProcessInstanceData(InstanceData Instance, VertexData Model, int Index, int Lod, RandomDistribution Distribution)
        {
            var element = Instance.Get(Lod);
            var model = element.OriginalMesh.Clone();
            model.Transform(element.TransMatrix);
            
            SetColor(model, element, Index);
            SetExtraData(model, element, Distribution);
            AssertValidModel(model);
            PackAndAddModel(model, Model);
        }

        private static void PackAndAddModel(VertexData InstanceModel, VertexData Model)
        {
            for (var i = 0; i < InstanceModel.Extradata.Count; i++)
            {
                InstanceModel.Colors[i] = new Vector4(InstanceModel.Colors[i].Xyz, InstanceModel.Extradata[i]);
            }

            /* Manually add these vertex data's for maximum performance */
            for (var k = 0; k < InstanceModel.Indices.Count; k++)
                InstanceModel.Indices[k] += (uint) Model.Vertices.Count;
            
            Model.Vertices.AddRange(InstanceModel.Vertices);
            Model.Colors.AddRange(InstanceModel.Colors);
            Model.Normals.AddRange(InstanceModel.Normals);
            Model.Indices.AddRange(InstanceModel.Indices);
            Model.Extradata.AddRange(InstanceModel.Extradata);
            InstanceModel.Dispose();
        }
        
        private static void AssertValidModel(VertexData Model)
        {
            if(Model.Colors.Count != Model.Extradata.Count)
                throw new ArgumentOutOfRangeException($"Extradata '{Model.Extradata.Count}' or color '{Model.Colors.Count}' mismatch");
        }
        
        private void SetColor(VertexData Model, InstanceData Element, int Index)
        {
            Model.Colors = Element.ColorCache != null && CacheManager.CachedColors.ContainsKey(Element.ColorCache)
                ? CacheManager.CachedColors[Element.ColorCache].Decompress()
                : Element.Colors;

            if (Element.VariateColor)
            {
                var variateFactor = (new Random(OffsetX + OffsetZ + World.Seed + Index).NextFloat() * 2f - 1f) *
                                    (24 / 256f);
                for (var l = 0; l < Model.Colors.Count; l++)
                    Model.Colors[l] += new Vector4(variateFactor, variateFactor, variateFactor, 0);

            }

            if (Model.Colors.Count != Model.Vertices.Count)
            {
                _parent.RemoveInstance(Element);
                Log.WriteLine($"Removed instance data '{Model.Name}' because of mismatch. ({Model.Colors.Count}|{Model.Vertices.Count})");
                return;
                //throw new ArgumentOutOfRangeException($"Color '{Model.Colors.Count}' and vertices '{Model.Vertices.Count}' mismatch. This is probably a cache collision");
            }

            if (Element.GraduateColor)
            {
                Model.GraduateColor(Vector3.UnitY);
            }
        }

        private static void SetExtraData(VertexData Model, InstanceData Element, RandomDistribution Distribution)
        {
            Model.Extradata = Element.ExtraDataCache != null &&  CacheManager.CachedExtradata.ContainsKey(Element.ExtraDataCache) 
                ? CacheManager.CachedExtradata[Element.ExtraDataCache].Decompress() 
                : Element.ExtraData;

            if (!Element.HasExtraData)
                Model.Extradata = Enumerable.Repeat(0f, Model.Vertices.Count).ToList();
            
            /* Pack some randomness to the wind values */
            Distribution.Seed = Unique.GenerateSeed(Element.Position.Xz);
            var rng = Distribution.NextFloat();
            for (var k = 0; k < Model.Extradata.Count; k++)
            {
                if (Model.Extradata[k] != 0 && Model.Extradata[k] != -10f)
                    Model.Extradata[k] = Mathf.Pack(new Vector2(Model.Extradata[k], rng), 2048);

                if (Model.Extradata[k] == -10f)
                    Model.Extradata[k] = -1f;
            }
        }
    }
}
