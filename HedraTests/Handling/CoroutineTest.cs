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
            CoroutineManager.Clear();
            CoroutineManager.StartCoroutine(TestCoroutine);
            Assert.AreEqual(1, CoroutineManager.Count);
            CoroutineManager.Update();
            CoroutineManager.Update();
            Assert.AreEqual(0, CoroutineManager.Count);
        }

        private IEnumerator TestCoroutine()
        {
            yield return null;
        }
    }
}