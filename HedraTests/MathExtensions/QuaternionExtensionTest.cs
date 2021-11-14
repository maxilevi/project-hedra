using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine;
using NUnit.Framework;
using System.Numerics;
using Hedra.Numerics;

namespace HedraTests.MathExtensions
{
    [TestFixture]
    public class QuaternionExtensionTest
    {

        [TestCaseSource(nameof(All))]
        public void TestQuaternionToEuler(Vector3 euler)
        {
            var quaternion = QuaternionMath.FromEuler(euler * Mathf.Radian);
            var newEuler = quaternion.ToEuler();
            Assert.Less((newEuler - euler).Length(), 0.001f, $"newEuler was {newEuler} but expected {euler}");
        }

        [Test]
        public void TestEulerToQuaternion()
        {
            var quaternion = new Quaternion(0.2688731f, 0.4689636f, -0.08115894f, 0.8373731f);
            var euler = quaternion.ToEuler();
            var newQuaternion = QuaternionMath.FromEuler(euler * Mathf.Radian);
            Assert.True((newQuaternion - quaternion).Length() < 0.0001f);
        }

        private static IEnumerable<Vector3> All()
        {
            yield return new Vector3(0, 150, 0);
            yield return new Vector3(52, 150, 63);
            yield return new Vector3(0, 0, 150);
            yield return new Vector3(12, 0, 169);
            yield return new Vector3(0, -150, 0);
            yield return new Vector3(-12, 150, 0);
            yield return new Vector3(0, 180, 0);
            yield return new Vector3(0, -180, 0);
            yield return new Vector3(0, 0, 0);
            yield return new Vector3(0, -179.99f, 0);
            yield return new Vector3(0, 270, 0);
            yield return new Vector3(0, -270, 0);
            yield return new Vector3(45, 0, 0);
            yield return new Vector3(0, 360, 0);
        } 
    }
}