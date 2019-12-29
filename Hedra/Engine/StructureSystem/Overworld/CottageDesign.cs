using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class CottageDesign<T> : QuestGiverStructureDesign<T>, IFindableStructureDesign where T : Cottage, IQuestStructure
    {
        public string DisplayName => Translations.Get("structure_cottage");
        public override int PlateauRadius => 160;
        public override VertexData Icon => null;
        public override bool CanSpawnInside => true;
        protected override int StructureChance => StructureGrid.CottageWithFarmChance;
        protected override BlockType PathType => BlockType.None;
        protected override CacheItem? Cache => null;
        protected override Vector3 NPCOffset => Vector3.Zero;
        protected override float QuestChance => 1;
        
        protected static void AddHouse(CollidableStructure Structure, Vector3 HouseOffset, VillageRoot Root, Random Rng)
        {
            var houseBuilder = new HouseBuilder(Structure);
            var housePosition = Structure.Position + HouseOffset;
            var houseDesign = Root.Template.House.Designs[0];
            var houseParameters = new HouseParameters
            {
                Design = houseDesign,
                GroundworkType = GroundworkType.Rounded,
                Position = housePosition,
                Rng = Rng,
                Rotation = Vector3.Zero,//Rotation.ExtractRotation().ToEuler(),
                Type = BlockType.StonePath,
                WellTemplate = Root.Template.Well.Designs[Rng.Next(0, Root.Template.Well.Designs.Length)]
            };
            
            /* The builder expects the position to be set at 0,0 */
            houseParameters.Position = new Vector3(houseParameters.Position.X, 0, houseParameters.Position.Z);
            
            var houseOutput = houseBuilder.Build(houseParameters, houseDesign, Root.Cache, Rng, housePosition);
            AddOutputToStructure(Structure, houseOutput, housePosition);
        }

        protected static void AddOutputToStructure(CollidableStructure Structure, BuildingOutput Output, Vector3 Position)
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
    }
}