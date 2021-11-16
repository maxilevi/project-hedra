using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Placers;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CottageWithFarmDesign : CottageDesign<CottageWithFarm>
    {
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng);
            var farmBuilder = new FarmBuilder(structure);
            var region = World.BiomePool.GetRegion(structure.Position);
            var root = VillageLoader.Designer[region.Structures.VillageType];
            var farmPlacer = new FarmPlacer(root.Template.Farm, root.Template.Farm.Designs,
                root.Template.Windmill.Designs, Rng);

            var farmPosition = structure.Position - Vector3.UnitZ * 16;
            var farmParameters = farmPlacer.Place(new PlacementPoint
            {
                CanBeRemoved = false,
                Position = farmPosition
            });
            farmBuilder.Place(farmParameters, root.Cache);
            structure.Parameters.Set("Root", root);
            structure.Parameters.Set("FarmPosition", farmPosition);
            structure.Parameters.Set("FarmParameters", farmParameters);
            structure.Parameters.Set("FarmBuilder", farmBuilder);

            return structure;
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            var root = Structure.Parameters.Get<VillageRoot>("Root");
            var farmPosition = Structure.Parameters.Get<Vector3>("FarmPosition");
            var farmParameters = Structure.Parameters.Get<FarmParameters>("FarmParameters");
            var farmBuilder = Structure.Parameters.Get<FarmBuilder>("FarmBuilder");

            /* The builder expects the position to be set at 0,0 */
            farmParameters.Position = new Vector3(farmParameters.Position.X, 1f, farmParameters.Position.Z);

            var farmOutput = farmBuilder.Build(farmParameters, farmParameters.Design, root.Cache, Rng, Vector3.Zero);
            AddOutputToStructure(Structure, farmOutput, farmPosition);

            AddHouse(Structure, Vector3.UnitZ * 80, root, Rng);
        }

        protected override IHumanoid CreateQuestGiverNPC(Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            return NPCCreator.SpawnQuestGiver(HumanType.Farmer, Position, Quest, Rng);
        }

        protected override IMissionDesign SelectQuest(Vector3 Position, Random Rng)
        {
            return MissionPool.Random(Position, QuestTier.Any, QuestHint.Farm);
        }

        protected override CottageWithFarm Create(Vector3 Position, float Size)
        {
            return new CottageWithFarm(Position, Size);
        }
    }
}