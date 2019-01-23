using Hedra;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using NUnit.Framework;

namespace HedraTests.ItemSystem
{
    public class ItemPoolTest : BaseTest
    {
        public ItemPoolTest()
        {
            ItemLoader.LoadModules(GameLoader.AppPath);
        }

        [Test]
        public void TestItemPoolGivesLessOrEqualRarity()
        {
            for (var i = 0; i < (int) ItemTier.Misc; i++)
            {
                var tier = ItemPool.SelectTier((ItemTier)i, Utils.Rng);
                Assert.LessOrEqual((int)tier, i, $"Expected tier '{(ItemTier) i}' but got tier {tier}");
            }
        }
        
        [Test]
        public void TestItemPoolSeed()
        {
            var seed = Utils.Rng.Next(0, 9999999);
            var item = ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine)
            {
                Seed = seed
            });
            var newItem = ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine)
            {
                Seed = seed
            });
            ItemTest.AssertAreSame(item, newItem);
        }
    }
}