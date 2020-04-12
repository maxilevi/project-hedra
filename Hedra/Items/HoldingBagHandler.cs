using System.IO;
using System.Linq;
using System.Text;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Items
{
    public class HoldingBagHandler : ItemHandler
    {
        public const int Size = 24;
        public override bool Consume(IPlayer Owner, Item Item)
        {
            ReadInventory(Item);
            Owner.ShowInventoryFor(Item);
            return false;
        }

        public static void ReadInventory(Item Bag)
        {
            var inventoryArray = new InventoryArray(Size);
            var objects = Bag.GetAttribute<JObject>("Objects");
            foreach (var prop in objects.Properties())
            {
                var index = int.Parse(prop.Name);
                inventoryArray.SetItem(index, Item.FromTemplate(ItemTemplate.FromJson(prop.Value.ToString())));
            }
            Bag.SetAttribute("Inventory", inventoryArray, true);
        }

        public static void SaveInventory(Item Bag, InventoryArray Array)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                var count = Array.Items.Count(I => I != null);
                writer.WriteStartObject();
                for (var i = 0; i < Array.Length; ++i)
                {
                    if(Array[i] == null) continue;
                    writer.WritePropertyName(i.ToString());
                    writer.WriteValue(ItemTemplate.ToJson(ItemTemplate.FromItem(Array[i])));
                }
                writer.WriteEndObject();
            }
            Bag.SetAttribute("Objects", JObject.Parse(sb.ToString()));
        }
    }
}