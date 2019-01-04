using System.Collections;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Handling
{
    [TestFixture]
    public class CoroutineTest
    {
        [Test]
        public void TestCoroutineIsRemoved()
        {
            RoutineManager.Clear();
            RoutineManager.StartRoutine(TestCoroutine);
            Assert.AreEqual(1, RoutineManager.Count);
            RoutineManager.Update();
            RoutineManager.Update();
            Assert.AreEqual(0, RoutineManager.Count);
        }

        private IEnumerator TestCoroutine()
        {
            yield return null;
        }
    }
}