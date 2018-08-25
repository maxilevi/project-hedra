using System;
using System.Linq;
using Hedra.Engine.StructureSystem;
using NUnit.Framework;

namespace HedraTests.Structure
{
    [TestFixture]
    public class StructureTest
    {
        //[Test]
        public void TestDesignsDontAddCollisionMeshesToChunks()
        {
            var designs = StructureGenerator.GetTypes().Select(Activator.CreateInstance);
        }
    }
}