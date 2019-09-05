using Hedra;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.PhysicsSystem
{
    [TestFixture]
    public class CollisionGroupTest
    {

        [SetUp]
        public void Setup()
        {
            World.Provider = new WorldProvider();
        }
        
        [Test]
        [Ignore("We aren't sure of what we should test.'")]
        public void TestOffsetsAreCalculatedCorrectly()
        {
            var width = Chunk.Width;
            var group = new CollisionGroup(new []
            {
                new CollisionShape(new []
                {
                    new Vector3(-width, 0, -width * 3),
                    new Vector3(width, 0, width),
                }),
            });
            Assert.True(group.Contains(Vector2.Zero));
            Assert.True(group.Contains(new Vector2(-width, -width * 3)));
            Assert.True(group.Contains(new Vector2(-width, -width * 2)));
            Assert.True(group.Contains(new Vector2(-width, -width * 1)));
            Assert.True(group.Contains(new Vector2(-width, 0)));
            Assert.True(group.Contains(new Vector2(-width, width)));
            Assert.True(group.Contains(new Vector2(0, -width * 3)));
            Assert.True(group.Contains(new Vector2(0, -width * 2)));
            Assert.True(group.Contains(new Vector2(0, -width * 1)));
            Assert.True(group.Contains(new Vector2(0, 0)));
            Assert.True(group.Contains(new Vector2(0, width)));
            Assert.True(group.Contains(new Vector2(width, -width * 3)));
            Assert.True(group.Contains(new Vector2(width, -width * 2)));
            Assert.True(group.Contains(new Vector2(width, -width * 1)));
            Assert.True(group.Contains(new Vector2(width, 0)));
            Assert.True(group.Contains(new Vector2(width, width)));
        }
    }
}