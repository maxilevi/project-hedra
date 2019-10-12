using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.ComplexMath;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.MathExtensions
{
    [TestFixture]
    public class QuaternionExtensionTest
    {
        [Test]
        public void TestQuaternionToEuler()
        {
            var euler = new Vector3(56, 12, 42);
            var quaternion = QuaternionMath.FromEuler(euler * Mathf.Radian);
            var newEuler = quaternion.ToEuler();
            Assert.True( (newEuler - euler).Length < 0.0001f );
        }

        [Test]
        public void TestEulerToQuaternion()
        {
            var quaternion = new Quaternion(0.2688731f, 0.4689636f, -0.08115894f, 0.8373731f);
            var euler = quaternion.ToEuler();
            var newQuaternion = QuaternionMath.FromEuler(euler * Mathf.Radian);
            Assert.True((newQuaternion - quaternion).Length < 0.0001f);
        }
    }
}