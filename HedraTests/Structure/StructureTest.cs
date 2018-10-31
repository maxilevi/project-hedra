using System;
using System.Linq;
using Hedra.Engine.StructureSystem;
using NUnit.Framework;

namespace HedraTests.Structure
{
    //[TestFixture]
    public class StructureTest
    {
        //[Test]
        public void TestDesignsDontAddCollisionMeshesToChunks()
        {
            var designs = StructureHandler.GetTypes().Select(Activator.CreateInstance);
        }
    }
}