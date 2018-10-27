
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Engine.ItemSystem
{
    public class ItemTemplate
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ItemTier Tier { get; set; }
        public string EquipmentType { get; set; }
        public string Description { get; set; }
        public AttributeTemplate[] Attributes { get; set; }
        public ItemModelTemplate Model { get; set; }

        public static ItemTemplate FromItem(Item Item)
        {
            return new ItemTemplate
            {
                Name = Item.Name,
                DisplayName = Item.DisplayName,
                Tier = Item.Tier,
                EquipmentType = Item.EquipmentType,
                Description = Item.Description,
                Attributes = Item.GetAttributes(),
                Model = Item.ModelTemplate
            };
        }

        public static string ToJson(ItemTemplate Template)
        {
            return JObject.FromObject(Template).ToString(Formatting.None);
        }

        public static ItemTemplate FromJSON(string Data)
        {
            return JsonConvert.DeserializeObject<ItemTemplate>(Data);
        }
    }
}
