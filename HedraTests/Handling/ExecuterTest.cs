using Hedra.Engine.Management;
using System.Collections.Generic;
using NUnit.Framework;

namespace HedraTests.Handling
{
    [TestFixture]
    public class ExecuterTest
    {
        [Test]
        public void TestFILO()
        {
            Executer.Flush();
            var resultList = new List<object>();
            var object1 = new object();
            var object2 = new object();
            Executer.ExecuteOnMainThread(() => resultList.Add(object1));
            Executer.ExecuteOnMainThread(() => resultList.Add(object2));
            Executer.Update();
            Assert.AreSame(object1, resultList[0]);
            Assert.AreSame(object2, resultList[1]);
        }
    }
}