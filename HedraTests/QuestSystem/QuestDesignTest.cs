using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Mission;
using NUnit.Framework;

namespace HedraTests.QuestSystem
{
    [TestFixture]
    public class QuestDesignTest
    {
        [Test]
        public void TestNamesDoNotConflict()
        {
            MissionPool.Load();
            var designs = MissionPool.Designs;
            var nameList = designs.Select(D => D.Name).ToList();
            Assert.True(designs.All(D =>
            {
                nameList.Remove(D.Name);
                return !nameList.Contains(D.Name);
            }));
        }
    }
}