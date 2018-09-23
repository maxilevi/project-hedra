using Hedra;
using Hedra.Engine;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Structure
{
    [TestFixture]
    public class GiantTreeDesignTest : DesignTest<GiantTreeDesign>
    {
        [Test]
        public void TestGiantTreeSpawnsWithoutBossAndWithChestUnderWater()
        {
            base.DesiredHeight = BiomePool.SeaLevel - 1;
            var structure = this.CreateStructure();
            Design.Build(structure);
            Executer.Update();
            Assert.AreEqual(0, WorldEntities.Length);
            Assert.AreEqual(1, WorldStructures.Length);
            Assert.True(WorldStructures[0] is Chest);
        }
    }
}