using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Structure
{
    [TestFixture]
    public class GraveyardDesignTest : DesignTest<GraveyardDesign>
    {
        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new Graveyard(Position, 0);
        }
    }
}