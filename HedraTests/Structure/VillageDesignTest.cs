using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Structure
{
    [TestFixture]
    public class VillageDesignTest : DesignTest<VillageDesign>
    {
        public VillageDesignTest()
        {
            VillageLoader.LoadModules(GameLoader.AppPath);
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CacheManager.Provider = new CacheProvider();
            CacheManager.Load();
        }

        protected override CollidableStructure CreateStructure()
        {
            var structure = base.CreateStructure();
            var builder = new VillageAssembler(structure, VillageLoader.Designer[VillageType.Woodland], new Random());
            var design = builder.DesignVillage();
            design.Translate(structure.Position);
            builder.PlaceGroundwork(design);

            structure.Parameters.Set("Builder", builder);
            structure.Parameters.Set("Design", design);
            return structure;
        }

        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new Village(Position);
        }
    }
}