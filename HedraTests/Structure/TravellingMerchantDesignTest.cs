using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Structure
{
    [TestFixture]
    public class TravellingMerchantDesignTest : DesignTest<TravellingMerchantDesign>
    {
        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new TravellingMerchant(Position);
        }
    }
}