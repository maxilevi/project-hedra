using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class BuildingOutput
    {
        public IList<InstanceData> Instances { get; set; }
        public IList<VertexData> LodModels { get; set; }
        public IList<VertexData> Models { get; set; }
        public IList<Matrix4x4> TransformationMatrices;
        public List<CollisionShape> Shapes { get; set; }
        public List<BaseStructure> Structures { get; set; }
        public bool BuildAsInstance { get; set; } = true;
        public bool GraduateColors { get; set; } = true;

        public BuildingOutput()
        {
            Structures = new List<BaseStructure>();
            Instances = new List<InstanceData>();
        }
        
        public void Color(Vector4 Find, Vector4 Replacement)
        {
            for (var i = 0; i < Models.Count; i++)
            {
                Models[i].Color(Find, Replacement);
            }
        }
        
        public CompressedBuildingOutput Compress()
        {
            return new CompressedBuildingOutput
            {
                Models = BuildAsInstance ? new List<CompressedVertexData>() : Models.Select(M => M.AsCompressed()).ToList(),
                Instances = BuildAsInstance ? BuildInstances() : new List<InstanceData>(),
                Shapes = Shapes,
                Structures = Structures
            };
        }

        private List<InstanceData> BuildInstances()
        {
            var list = new List<InstanceData>();
            if (LodModels != null && Models.Count != LodModels.Count) 
                throw new ArgumentOutOfRangeException("The LOD model and normal model count needs to be the same");
            
            for (var i = 0; i < Models.Count; i++)
            {
                if(Models[i].IsEmpty) continue;
                var mainInstance = BuildInstance(Models[i], TransformationMatrices[i]);
                if(LodModels != null) mainInstance.AddLOD(BuildInstance(LodModels[i], TransformationMatrices[i]), 2);
                list.Add(mainInstance);
            }
            list.AddRange(Instances);
            return list;
        }

        private InstanceData BuildInstance(VertexData Model, Matrix4x4 TransformationMatrix)
        {
            if(!Model.IsClone) 
                throw new ArgumentOutOfRangeException("All models need to be clones in order to build the instances");
            if (Model.Vertices.Count == 0)
            {
                int a = 0;
            }
            var model = new InstanceData
            {
                OriginalMesh = Model.Original,
                Colors = Model.Colors,
                ExtraData = Model.Extradata,
                TransMatrix = TransformationMatrix,
                GraduateColor = GraduateColors
            };
            CacheManager.Check(model);
            return model;
        }

        public BuildingOutput Concat(BuildingOutput New)
        {
            Models = Models.Concat(New.Models).ToArray();
            Shapes.AddRange(New.Shapes);
            TransformationMatrices = TransformationMatrices.Concat(New.TransformationMatrices).ToArray();
            Structures.AddRange(New.Structures);
            LodModels = LodModels?.Concat(New.LodModels ?? new VertexData[0]).ToArray();
            Instances = Instances.Concat(New.Instances).ToList();
            return this;
        }
        
        public bool IsEmpty => Models.Count == 0 && Shapes.Count == 0 && Structures.Count == 0;

        public static BuildingOutput Empty => new BuildingOutput
        {
            Models = new List<VertexData>(),
            Shapes = new List<CollisionShape>(),
            Structures = new List<BaseStructure>(),
            TransformationMatrices = new List<Matrix4x4>()
        };
    }
}