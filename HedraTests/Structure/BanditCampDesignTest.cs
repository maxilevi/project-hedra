using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Structure
{
    [TestFixture]
    public class BanditCampDesignTest : DesignTest<BanditCampDesign>
    {
        protected override CollidableStructure CreateStructure()
        {
            var structure = base.CreateStructure();
            structure.Parameters.Set("ScaleMatrix", Matrix4.Identity);
            structure.Parameters.Set("TentParameters", new BanditCampDesign.TentParameters[0]);
            return structure;
        }
    }
}