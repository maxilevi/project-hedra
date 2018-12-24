using Hedra.Engine.PhysicsSystem;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.PhysicsSystem
{
    [TestFixture]
    public class BoxTest
    {
        [Test]
        public void TestBoxToShape()
        {
            var box = new Box(Vector3.Zero, Vector3.One);
            var shape = box.ToShape();
            Assert.AreEqual(new []
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1),
                
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(1, 1, 1)
            }, shape.Vertices);
        }
    }
}