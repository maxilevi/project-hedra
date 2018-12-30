using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;
using NUnit.Framework;

namespace HedraTests.QuestSystem
{
    [TestFixture]
    public class QuestDesignTest
    {
        [Test]
        public void TestNamesDoNotConflict()
        {
            var designs = QuestPool.Designs;
            var nameList = designs.Select(D => D.Name).ToList();
            Assert.True(designs.All(D =>
            {
                nameList.Remove(D.Name);
                return !nameList.Contains(D.Name);
            }));
        }
    }
}