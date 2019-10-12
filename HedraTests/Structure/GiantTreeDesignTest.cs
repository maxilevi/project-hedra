using Hedra;
using Hedra.Engine;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Structure
{
    [TestFixture]
    public class GiantTreeDesignTest : DesignTest<GiantTreeDesign>
    {
        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new GiantTree(Position);
        }

        [Test]
        public void TestGiantTreeSpawnsWithoutBossAndWithChestUnderWater()
        {
            base.DesiredHeight = BiomePool.SeaLevel - 1;
            var structure = this.CreateStructure();
            Design.Build(structure);
            Executer.Update();
            var structures = GetStructureObjects(structure);
            Assert.AreEqual(0, WorldEntities.Length);
            Assert.AreEqual(2, structures.Length);
            Assert.True(structures[0] is GiantTree);
            Assert.True(structures[1] is Chest);
        }
    }
}