using System.Collections.Generic;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    public class ItemModelTest
    {
        [TestCaseSource(nameof(All))]
        public void TestItemModelCanBeLoaded(Item Item)
        {
            AssertCanLoad(Item);
        }

        private static void AssertCanLoad(Item Item)
        {
            if(!(AssetManager.Provider is CompressedAssetProvider))
                AssetManager.Provider = new CompressedAssetProvider();
            var model = Item.Model;
            Assert.NotNull(model);
        }
        
        private static IEnumerable<Item> All()
        {
            ItemLoader.LoadModules(GameLoader.AppPath);
            var templates = ItemLoader.Templater.Templates;
            for (var i = 0; i < templates.Length; i++)
            {
                var newItem = Item.FromTemplate(templates[i]);
                yield return newItem;
            }
        } 
    }
}