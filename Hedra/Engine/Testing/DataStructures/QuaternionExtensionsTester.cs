using System;
using Hedra.Engine.ComplexMath;
using OpenTK;

namespace Hedra.Engine.Testing.DataStructures
{
    public class QuaternionExtensionsTester : BaseTest
    {
        [TestMethod]
        public void TestQuaternionToEuler()
        {
            var euler = new Vector3(56, 12, 42);
            var quaternion = QuaternionMath.FromEuler(euler * Mathf.Radian);
            var newEuler = quaternion.ToEuler();
            this.AssertTrue( (newEuler - euler).Length < 0.0001f );
        }

        [TestMethod]
        public void TestEulerToQuaternion()
        {
            var quaternion = new Quaternion(0.2688731f, 0.4689636f, -0.08115894f, 0.8373731f);
            var euler = quaternion.ToEuler();
            var newQuaternion = QuaternionMath.FromEuler(euler * Mathf.Radian);
            this.AssertTrue((newQuaternion - quaternion).Length < 0.0001f);
        }
    }
}
