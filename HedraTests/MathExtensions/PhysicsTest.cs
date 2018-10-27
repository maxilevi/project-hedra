using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.MathExtensions
{
    [TestFixture]
    public class PhysicsTest
    {
        [Test]
        public void DirectionToEulerTest()
        {
            var samples = new Dictionary<Vector3, Vector3>
            {
                {new Vector3(0, 1, 0), new Vector3(-90, 0, 0)},
                {new Vector3(0, -1, 0), new Vector3(90, 0, 0)},
                {new Vector3(0, 0, 1), new Vector3(0, 0, 0)},
                {new Vector3(0, 0, -1), new Vector3(0, 180, 0)},
                {new Vector3(1, 0, 0), new Vector3(0, 90, 0)},
                {new Vector3(-1, 0, 0), new Vector3(0, -90, 0)},
                {new Vector3(1, 1, 1), new Vector3(-37.2f, 37.2f, 0)},
                {new Vector3(-1, -1, -1), new Vector3(37.2f, 217.2f, 0)},
                {new Vector3(-1, -1, 1), new Vector3(37.2f, -37.2f, 0)},
                {new Vector3(-1, 0, -1), new Vector3(0, 225, 0)},
                {new Vector3(-1, 1, 0), new Vector3(-56.75f, -56.75f, 0)},
                {new Vector3(0, -1, -1), new Vector3(45, 180, 0)},
            };
            foreach (var sample in samples)
            {
                var expectedEuler = sample.Value;
                var euler = Physics.DirectionToEuler(sample.Key);
                Assert.True((euler - expectedEuler).Length < 0.05f, $"Expected euler: {expectedEuler} should be equal to {euler}");
            }
        }
    }
}