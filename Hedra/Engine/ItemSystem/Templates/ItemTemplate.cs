using System.Linq;
using Hedra.Engine.ModuleSystem;

namespace Hedra.Engine.ItemSystem.Templates
{    
    public class ItemTemplate : SerializableTemplate<ItemTemplate>
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
    }
}
