using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Placers;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Mission;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CottageWithFarmDesign : QuestGiverStructureDesign<CottageWithFarm>
    {
        public override int PlateauRadius => 160;
        public override VertexData Icon => null;
        public override bool CanSpawnInside => true;
        protected override int StructureChance => StructureGrid.CottageWithFarmChance;
        protected override CacheItem? Cache => null;
        protected override BlockType PathType => BlockType.None;

        protected override CottageWithFarm Create(Vector3 Position, float Size)
        {
            return new CottageWithFarm(Position);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng);
            var farmBuilder = new FarmBuilder(structure);
            var region = World.BiomePool.GetRegion(structure.Position);
            var root = VillageLoader.Designer[region.Structures.VillageType];
            var farmPlacer = new FarmPlacer(root.Template.Farm, root.Template.Farm.Designs, root.Template.Windmill.Designs, Rng);

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

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            var houseBuilder = new HouseBuilder(Structure);
            var root = Structure.Parameters.Get<VillageRoot>("Root");
            var farmPosition = Structure.Parameters.Get<Vector3>("FarmPosition");
            var farmParameters = Structure.Parameters.Get<FarmParameters>("FarmParameters");
            var farmBuilder = Structure.Parameters.Get<FarmBuilder>("FarmBuilder");
            
            /* The builder expects the position to be set at 0,0 */
            farmParameters.Position = new Vector3(farmParameters.Position.X, 0, farmParameters.Position.Z);

            var farmOutput = farmBuilder.Build(farmParameters, farmParameters.Design, root.Cache, Rng, Vector3.Zero);
            AddOutputToStructure(Structure, farmOutput, farmPosition);

            var housePosition = Structure.Position + Vector3.UnitZ * 80;
            var houseDesign = root.Template.House.Designs[0];
            var houseParameters = new HouseParameters
            {
                Design = houseDesign,
                GroundworkType = GroundworkType.Rounded,
                Position = housePosition,
                Rng = Rng,
                Rotation = Vector3.Zero,//Rotation.ExtractRotation().ToEuler(),
                Type = BlockType.StonePath,
                WellTemplate = root.Template.Well.Designs[Rng.Next(0, root.Template.Well.Designs.Length)]
            };
            
            /* The builder expects the position to be set at 0,0 */
            houseParameters.Position = new Vector3(houseParameters.Position.X, 0, houseParameters.Position.Z);
            
            var houseOutput = houseBuilder.Build(houseParameters, houseDesign, root.Cache, Rng, housePosition);
            AddOutputToStructure(Structure, houseOutput, housePosition);
        }

        private static void AddOutputToStructure(CollidableStructure Structure, BuildingOutput Output, Vector3 Position)
        {
            var onlyYMatrix = Matrix4x4.CreateTranslation(Vector3.UnitY * Position.Y);
            Structure.AddCollisionShape(Output.Shapes.Select(S => S.Transform(onlyYMatrix)).ToArray());
            Structure.AddStaticElement(Output.Models.Select(M => M.Translate(Position)).ToArray());
            Structure.AddInstance(Output.Instances.Select(I => I.Apply(onlyYMatrix)).ToArray());
            var structures = Output.Structures.ToArray();
            for (var i = 0; i < structures.Length; ++i)
            {
                structures[i].Position += Position.Y * Vector3.UnitY;
            }
            Structure.WorldObject.AddChildren(structures);
        }

        protected override Vector3 NPCOffset => Vector3.Zero;
        protected override float QuestChance => 1f;

        protected override IHumanoid CreateQuestGiverNPC(Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            return NPCCreator.SpawnQuestGiver(HumanType.Farmer, Position, Quest, Rng);
        }

        protected override IMissionDesign SelectQuest(Vector3 Position, Random Rng)
        {
            return MissionPool.Random(Position, QuestTier.Any, QuestHint.Farm);
        }
    }
}