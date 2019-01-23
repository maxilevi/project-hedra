using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using NUnit.Framework;

namespace HedraTests.ItemSystem
{
    [TestFixture]
    public class ItemTest : BaseTest
    {
        public ItemTest()
        {
            ItemLoader.LoadModules(GameLoader.AppPath);
        }
       
        [Test]
        public void TestItemSerializationIsEqual()
        {
            var item = ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine));
            var newItem = Item.FromArray(item.ToArray());
            
            AssertAreSame(item, newItem);
        }

        public static void AssertAreSame(Item A, Item B)
        {
            Assert.AreEqual(A.Name, B.Name);
            Assert.AreEqual(A.Description, B.Description);
            Assert.AreEqual(A.Tier, B.Tier);
            Assert.AreEqual(A.DisplayName, B.DisplayName);
            Assert.AreEqual(A.EquipmentType, B.EquipmentType);

            var attributesA = A.GetAttributes();
            var attributesB = B.GetAttributes();
            Assert.AreEqual(attributesA.Length, attributesB.Length);
            for (var i = 0; i < attributesA.Length; i++)
            {
                Assert.AreEqual(attributesA[i].Name, attributesB[i].Name);
                Assert.AreEqual(attributesA[i].Value, attributesB[i].Value,
                    $"Expected '{attributesA[i].Name}' to have {attributesA[i].Value} but was {attributesB[i].Value}");
            }
        }
    }
}