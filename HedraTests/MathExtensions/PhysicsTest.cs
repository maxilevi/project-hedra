using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using NUnit.Framework;
using System.Numerics;
using Hedra.Numerics;

namespace HedraTests.MathExtensions
{
    [TestFixture]
    public class PhysicsTest
    {
        [TestCaseSource(nameof(All))]
        public void DirectionToEulerTest(KeyValuePair<Vector3, Vector3> pair)
        {
            var expectedEuler = pair.Value;
            var euler = Physics.DirectionToEuler(pair.Key);

            Assert.True((euler - expectedEuler).Length() < 0.05f, $"Rotation of direction {pair.Key} was {euler} but should be {expectedEuler}");
        }

        private static IEnumerable All()
        {
            var samples = new Dictionary<Vector3, Vector3>
            {
                {new Vector3(0, 0, 1), new Vector3(0, 0, 0)},
                {new Vector3(0, 0, -1), new Vector3(0, 180, 0)},
                {new Vector3(0, 1, 0), new Vector3(-90, 90, 0)},
                {new Vector3(0, -1, 0), new Vector3(90, 90, 0)},
                {new Vector3(1, 0, 0), new Vector3(0, 90, 0)},
                {new Vector3(-1, 0, 0), new Vector3(0, -90, 0)},
                {new Vector3(1, 1, 1), new Vector3(-90, 45, 0)},
                {new Vector3(-1, -1, -1), new Vector3(90, 225, 0)},
                {new Vector3(-1, -1, 1), new Vector3(90, -45, 0)},
                {new Vector3(-1, 0, -1), new Vector3(0, 225, 0)},
                {new Vector3(-1, 1, 0), new Vector3(-90, -90, 0)},
                {new Vector3(0, -1, -1), new Vector3(90, 180, 0)},
            };
            foreach (var pair in samples)
            {
                yield return pair;
            } 
        }
    }
}