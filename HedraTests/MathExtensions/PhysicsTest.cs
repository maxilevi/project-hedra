using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using NUnit.Framework;
using System.Numerics;

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
                {new Vector3(1, 1, 1), new Vector3(-84.92001f, 35.18748f, 35.18751f)},
                {new Vector3(-1, -1, -1), new Vector3(41.78011f, -100.8305f, 100.8306f)},
                {new Vector3(-1, -1, 1), new Vector3(84.92001f, -35.18748f, 35.18751f)},
                {new Vector3(-1, 0, -1), new Vector3(0, -134.9858f, 0)},
                {new Vector3(-1, 1, 0), new Vector3(-69.28205f, -69.28208f, -69.28213f)},
                {new Vector3(0, -1, -1), new Vector3(0, -124.6111f, 124.6112f)},
            };
            foreach (var sample in samples)
            {
                var expectedEuler = sample.Value;
                var euler = Physics.DirectionToEuler(sample.Key);
                Assert.True((euler - expectedEuler).Length() < 0.05f, $"Expected euler: {expectedEuler} should be equal to {euler}");
            }
        }
    }
}