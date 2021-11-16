using System;
using System.Linq;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Items;
using Hedra.WeaponSystem;

namespace Hedra.Engine.ItemSystem.Templates
{
    public class ItemTemplate : SerializableTemplate<ItemTemplate>, INamedTemplate
    {
        public string DisplayName { get; set; }
        public ItemTier Tier { get; set; }
        public string EquipmentType { get; set; }
        public string Description { get; set; }
        public AttributeTemplate[] Attributes { get; set; }
        public ItemModelTemplate Model { get; set; }
        public bool IsWeapon => WeaponFactory.Contains(EquipmentType);
        public bool IsArmor => ArmorFactory.Contains(EquipmentType);

        public bool IsRecipe => string.Equals(EquipmentType, Items.EquipmentType.Recipe.ToString(),
            StringComparison.InvariantCultureIgnoreCase);

        public string Name { get; set; }

        public static ItemTemplate FromItem(Item Item)
        {
            return new ItemTemplate
            {
                Name = Item.Name,
                DisplayName = Item.DisplayName,
                Tier = Item.Tier,
                EquipmentType = Item.EquipmentType,
                Description = Item.Description,
                Attributes = Item.GetAttributes().Where(A => A.Persist).ToArray(),
                Model = Item.ModelTemplate
            };
        }
    }
}