using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class GenericParameters : ILivableBuildingParameters
    {
        public GenericDesignTemplate Design { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Random Rng { get; set; }
        public BlockType Type { get; set; } = BlockType.Grass;
        public GroundworkType GroundworkType { get; set; }
        public bool HasNPC => NPCSettings != null;
        public GenericNPCSettings NPCSettings { get; set; }

        BuildingDesignTemplate ILivableBuildingParameters.Design => Design;
        
        DesignTemplate IBuildingParameters.Design
        {
            get => Design;
            set => Design = value as GenericDesignTemplate;
        }

        public float GetSize(VillageCache Cache)
        {
            return Cache.GrabSize(Design.Path).Xz().LengthFast() * .65f;
        }
    }
}