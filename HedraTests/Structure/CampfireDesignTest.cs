using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Structure
{
    [TestFixture]
    public class CampfireDesignTest : DesignTest<CampfireDesign>
    {
        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new Campfire(Position);
        }
    }
}