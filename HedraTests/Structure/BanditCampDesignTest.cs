using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using System.Numerics;

namespace HedraTests.Structure
{
    [TestFixture]
    public class BanditCampDesignTest : DesignTest<BanditCampDesign>
    {
        protected override BaseStructure CreateBaseStructure(Vector3 Position)
        {
            return new BanditCamp(Position, 0);
        }
        
        protected override CollidableStructure CreateStructure()
        {
            var structure = base.CreateStructure();
            structure.Parameters.Set("ScaleMatrix", Matrix4x4.Identity);
            structure.Parameters.Set("TentParameters", new BanditCampDesign.TentParameters[0]);
            return structure;
        }
    }
}