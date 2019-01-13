using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Structure
{
    [TestFixture]
    public class WellDesignTest : DesignTest<WellDesign>
    {
        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new Well(Position, 32);
        }
    }
}