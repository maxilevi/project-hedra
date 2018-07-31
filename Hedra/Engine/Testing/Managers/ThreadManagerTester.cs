using System.Collections.Generic;
using Hedra.Engine.Management;

namespace Hedra.Engine.Testing.Managers
{
    public class ThreadManagerTester : BaseTest
    {
        [TestMethod]
        //Test threadmanager works in a first in last out fashion.
        public void TestThreadManagerHandlesUpdatesInTheCorrectOrder()
        {
            var resultList = new List<object>();
            var object1 = new object();
            var object2 = new object();
            Executer.ExecuteOnMainThread(() => resultList.Add(object1));
            Executer.ExecuteOnMainThread(() => resultList.Add(object2));
            Executer.Update();
            this.AssertEqual(object1, resultList[0]);
            this.AssertEqual(object2, resultList[1]);
        }
    }
}
