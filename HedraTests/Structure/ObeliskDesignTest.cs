using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using System.Numerics;

namespace HedraTests.Structure
{
    [TestFixture]
    public class ObeliskDesignTest : DesignTest<ObeliskDesign>
    {
        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new Obelisk(Position);
        }
    }
}