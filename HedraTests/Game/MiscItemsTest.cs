using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Items;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    public class MiscItemsTest
    {
        [Test]
        public void TestMiscItemsHavePriceAttribute()
        {
            AssetManager.Provider = new DummyAssetProvider();
            ItemLoader.LoadModules(GameLoader.AppPath);
            var items = ItemPool.Matching(T => T.Tier == ItemTier.Misc);
            for (var i = 0; i < items.Length; i++)
            {
                if(items[i].IsGold) continue;
                Assert.True(items[i].HasAttribute(CommonAttributes.Price), $"{items[i].DisplayName} does not contain a 'Price' attribute.");
            }
        }  
    }
}