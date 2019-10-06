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
using Hedra.Engine.Native;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Game;
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

        public unsafe ChunkMeshBuildOutput AddStructuresMeshes(ChunkMeshBuildOutput Input, int Lod)
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

            var size = Allocator.Megabyte * 2;
            var mem = stackalloc byte[size];
            using (var allocator = new StackAllocator(size, mem))
            {
                var structureData = new NativeVertexData(allocator);
                var instanceData = new NativeVertexData(allocator);
                for (var i = 0; i < staticElements.Count; i++)
                {
                    var clone = staticElements[i].NativeClone(allocator);
                    if (Lod > 1)
                    {
                        clone.UniqueVertices();
                        MeshOptimizer.SimplifySloppy(allocator, clone.Indices, clone.Vertices, LODMap[Lod]);
                        clone.Flat();
                    }

                    if (clone.Extradata.Count != clone.Vertices.Count)
                    {
                        float extraDataCount = clone.Vertices.Count - clone.Extradata.Count;
                        for (var k = 0; k < extraDataCount; k++) clone.Extradata.Add(0);
                    }

                    /* Manually add these vertex data's for maximum performance */
                    for (var k = 0; k < clone.Indices.Count; k++)
                        clone.Indices[k] += (uint) Input.StaticData.Vertices.Count;

                    structureData.Vertices.AddRange(clone.Vertices);
                    structureData.Colors.AddRange(clone.Colors);
                    structureData.Normals.AddRange(clone.Normals);
                    structureData.Indices.AddRange(clone.Indices);
                    structureData.Extradata.AddRange(clone.Extradata);
                    clone.Dispose();
                }

                var distribution = new RandomDistribution();
                var instanceElements = Mesh.InstanceElements;
                for (var i = 0; i < instanceElements.Length; i++)
                {
                    if (instanceElements[i].SkipOnLod && Lod != 1) continue;
                    ProcessInstanceData(instanceElements[i], structureData, i, Lod, distribution, instanceElements[i].CanSimplifyProgramatically ? LODMap[Lod] : -1);
                }

                var addGrass = GameSettings.Quality && Lod <= 2 || Lod == 1 && !GameSettings.Quality;
                if (addGrass)
                {
                    var lodedInstanceElements = Mesh.LodAffectedInstanceElements;
                    for (var i = 0; i < lodedInstanceElements.Length; i++)
                    {
                        ProcessInstanceData(lodedInstanceElements[i], instanceData, i, Lod, distribution, 1f / Lod - 0.2f);
                    }
                }

                AddNativeVertexData(Input.StaticData, structureData);
                AddNativeVertexData(Input.InstanceData, instanceData);
            }

            return new ChunkMeshBuildOutput(Input.StaticData, Input.WaterData, Input.InstanceData, Input.Failed);
        }

        private static void AddNativeVertexData(VertexData Data, NativeVertexData Native)
        {
            /* Manually add these vertex data's for maximum performance */
            for (var k = 0; k < Native.Indices.Count; k++)
                Native.Indices[k] += (uint) Data.Vertices.Count;

            Data.Vertices.AddRange(Native.Vertices);
            Data.Colors.AddRange(Native.Colors);
            Data.Normals.AddRange(Native.Normals);
            Data.Indices.AddRange(Native.Indices);
            Data.Extradata.AddRange(Native.Extradata);
        }

        private unsafe void ProcessInstanceData(InstanceData Instance, NativeVertexData Model, int Index, int Lod, RandomDistribution Distribution, float SimplificationThreshold = -1)
        {
            var element = Instance.Get(Lod);
            const int size = Allocator.Kilobyte * 256;
            var mem = stackalloc byte[size];
            using (var allocator = new StackAllocator(size, mem))
            {
                var model = element.OriginalMesh.NativeClone(allocator);
                model.Transform(element.TransMatrix);

                SetColor(model, element, Index);
                SetExtraData(model, element, Distribution);
                if (SimplificationThreshold > 0 && SimplificationThreshold < 1f)
                {
                    model.UniqueVertices();
                    MeshOptimizer.SimplifySloppy(allocator, model.Indices, model.Vertices, SimplificationThreshold);
                    model.Flat();
                }

                AssertValidModel(model);
                PackAndAddModel(model, Model);
            }
        }

        private static void PackAndAddModel(NativeVertexData InstanceModel, NativeVertexData Model)
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
        
        private static void AssertValidModel(NativeVertexData Model)
        {
            if(Model.Colors.Count != Model.Extradata.Count)
                throw new ArgumentOutOfRangeException($"Extradata '{Model.Extradata.Count}' or color '{Model.Colors.Count}' mismatch");
            
            if(Model.Colors.Count != Model.Vertices.Count)
                throw new ArgumentOutOfRangeException($"Vertex '{Model.Extradata.Count}' and color '{Model.Colors.Count}' mismatch");
        }
        
        private void SetColor(NativeVertexData Model, InstanceData Element, int Index)
        {
            var retrieval = Element.ColorCache != null && CacheManager.CachedColors.ContainsKey(Element.ColorCache)
                ? CacheManager.CachedColors[Element.ColorCache]
                : Element.Colors;

            Model.Colors.Set(retrieval);

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

        private static void SetExtraData(NativeVertexData Model, InstanceData Element, RandomDistribution Distribution)
        {
            var retrieval = Element.ExtraDataCache != null &&  CacheManager.CachedExtradata.ContainsKey(Element.ExtraDataCache) 
                ? CacheManager.CachedExtradata[Element.ExtraDataCache]
                : Element.ExtraData;

            Model.Extradata.Set(retrieval);

            if (!Element.HasExtraData)
                Model.Extradata.Set(0, Model.Vertices.Count);
            
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
        
        private static readonly Dictionary<int, float> LODMap = new Dictionary<int, float>
        {
            {1, 1.0f},
            {2, 0.7f},
            {4, 0.4f},
            {8, 0.25f}
        };
    }
}
