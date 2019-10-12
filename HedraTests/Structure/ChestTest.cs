using Hedra.Engine.ItemSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Items;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Structure
{
    //[TestFixture]
    public class ChestTest : BaseTest
    {
        private Chest _chest;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _chest = new Chest(Vector3.Zero, new Item());
        }

        //[Test]
        public void TestPickupIsInvoked()
        {
            Assert.Fail();
        }
        
        //[Test]
        public void TestCollidersArePlaced()
        {
            Assert.Fail();
        }
    }
}