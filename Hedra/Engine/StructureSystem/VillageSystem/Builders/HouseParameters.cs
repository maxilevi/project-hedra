using System;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class HouseParameters : IBuildingParameters
    {
        public HouseDesignTemplate Design { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Random Rng { get; set; }
        public DesignTemplate WellTemplate { get; set; }
        public BlockType Type { get; set; } = BlockType.Grass;
        public GroundworkType GroundworkType { get; set; }

        DesignTemplate IBuildingParameters.Design
        {
            get => Design;
            set => Design = value as HouseDesignTemplate;
        }

        public float GetSize(VillageCache Cache)
        {
            return Cache.GrabSize(Design.Path).Xz.LengthFast * .5f;
        }
    }
}