using System;
using System.Linq;
using System.Text;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.ItemSystem
{
    public class Item
    {
        private static string GoldItemName = "Gold";
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ItemTier Tier { get; set; }
        public string EquipmentType { get; set; }
        public ItemModelTemplate ModelTemplate { get; private set; }
        private readonly AttributeArray _attributes;
        private Weapon _weaponCache;
        private ArmorPiece _armorCache;
        private VertexData _model;
        private bool _armorCacheDirty;
        private bool _weaponCacheDirty;

        public Item()
        {
            _attributes = new AttributeArray();
        }

        public static Item FromTemplate(ItemTemplate Template)
        {
            var item = new Item
            {
                DisplayName = Template.DisplayName,
                Name = Template.Name,
                Tier = Template.Tier,
                Description = Template.Description,
                EquipmentType = Template.EquipmentType,
                ModelTemplate = Template.Model,
            };
            item.SetAttributes(Template.Attributes);
            return item;
        }

        public bool HasAttribute(CommonAttributes Attribute)
        {
            return this.HasAttribute(Attribute.ToString());
        }

        public bool HasAttribute(string Attribute)
        {
            return _attributes.Has(Attribute);
        }
        public T GetAttribute<T>(CommonAttributes Attribute)
        {
            return this.GetAttribute<T>(Attribute.ToString());
        }

        public void DeleteAttribute(CommonAttributes Attribute)
        {
            this.DeleteAttribute(Attribute.ToString());
        }

        public void SetAttribute(CommonAttributes Attribute, object Value)
        {
            this.SetAttribute(Attribute, Value, false);
        }

        public void SetAttribute(CommonAttributes Attribute, object Value, bool Hidden)
        {
            this.SetAttribute(Attribute.ToString(), Value, Hidden);
        }

        public void SetAttribute(CommonAttributes Attribute, object Value, bool Hidden, string Display)
        {
            this.SetAttribute(Attribute.ToString(), Value, Hidden, Display);
        }

        public void SetAttribute(string Attribute, object Value)
        {
            this.SetAttribute(Attribute, Value, false);
        }

        public void SetAttribute(string Attribute, object Value, bool Hidden)
        {
            this.SetAttribute(Attribute, Value, Hidden, null);
        }

        public void SetAttribute(string Attribute, object Value, bool Hidden, string Display)
        {
            _attributes.Set(Attribute, Value, Hidden, Display);
        }

        public T GetAttribute<T>(string Attribute)
        {
            return _attributes.Get<T>(Attribute);
        }

        public void DeleteAttribute(string Attribute)
        {
            _attributes.Delete(Attribute);
        }

        public AttributeTemplate[] GetAttributes()
        {
            return _attributes.Gather();
        }

        public void SetAttributes(AttributeTemplate[] Templates)
        {
            foreach (var attribute in Templates)
            {
                this.SetAttribute(attribute.Name, attribute.Value, attribute.Hidden, attribute.Display);
            }
        }

        public void ClearAttributes()
        {
            _attributes.Clear();
        }

        public void FlushCache()
        {
            _weaponCacheDirty = true;
        }

        public static Item FromArray(byte[] Array)
        {
            var savedTemplate = ItemTemplate.FromJSON(Encoding.ASCII.GetString(Array));
            if (!ItemPool.Exists(savedTemplate.Name)) return null;
            var defaultTemplate = ItemFactory.Templater[savedTemplate.Name];
            savedTemplate.Model = defaultTemplate.Model;
            savedTemplate.Description = defaultTemplate.Description;
            savedTemplate.DisplayName = defaultTemplate.DisplayName;
            savedTemplate.Tier = defaultTemplate.Tier;
            var item = FromTemplate(savedTemplate);
            if (savedTemplate.EquipmentType == null) return UpdateAttributes(item);
            
            var newItem = ItemPool.Grab(savedTemplate.Name);
            if (!item.HasAttribute(CommonAttributes.Seed)) item.SetAttribute(CommonAttributes.Seed, Utils.Rng.Next(int.MinValue, int.MaxValue), true);

            newItem.SetAttribute(CommonAttributes.Seed, item.GetAttribute<int>(CommonAttributes.Seed), true);
            return ItemPool.Randomize(newItem, new Random(newItem.GetAttribute<int>(CommonAttributes.Seed)));           
        }

        private static Item UpdateAttributes(Item Item)
        {
            if (Item.HasAttribute(CommonAttributes.Amount))
            {
                if(!Item.IsFood && !Item.IsGold) 
                    throw new ArgumentOutOfRangeException($"Type of item {Item.Name} was not expected");
                var amount = Item.GetAttribute<int>(CommonAttributes.Amount);
                var newItem = ItemPool.Grab(Item.Name);
                newItem.SetAttribute(CommonAttributes.Amount, amount);
                return newItem;
            }
            return Item;
        }

        public byte[] ToArray()
        {
            return Encoding.ASCII.GetBytes(ItemTemplate.ToJson(ItemTemplate.FromItem(this)));
        }

        public bool IsGold => Name == GoldItemName;
        public bool IsFood => HasAttribute("IsFood") && GetAttribute<bool>("IsFood") || Name == "Berry";
        public bool IsWeapon => WeaponFactory.Contains(this);
        public bool IsArmor => ArmorFactory.Contains(this);
        public bool IsRing => EquipmentType == ItemSystem.EquipmentType.Ring.ToString();
        public bool IsEquipment => IsWeapon || IsRing || IsArmor;

        public VertexData Model
        {
            get
            {
                if (_model == null)
                    Model = ItemModelLoader.Load(ModelTemplate);
                return _model;
            }
            set
            {
                _model = value;
                _weaponCacheDirty = true;
            }
        }
        
        public HelmetPiece Helmet => GetArmor<HelmetPiece>();
        
        public ChestPiece Chestplate => GetArmor<ChestPiece>();
        
        public PantsPiece Pants => GetArmor<PantsPiece>();
        
        public BootsPiece Boots => GetArmor<BootsPiece>();

        public Weapon Weapon
        {
            get
            {
                if (_weaponCache != null && !_weaponCacheDirty && !_weaponCache.Disposed) return _weaponCache;

                var weapon = WeaponFactory.Get(this);
                _weaponCache = weapon;
                _weaponCacheDirty = false;

                return _weaponCache;
            }
        }
        
        private T GetArmor<T>() where T : ArmorPiece
        {
            if (_armorCache != null && !_armorCacheDirty && !_armorCache.Disposed) return (T) _armorCache;

            var armor = ArmorFactory.Get(this);
            _armorCache = armor;
            _armorCacheDirty = false;

            return (T) _armorCache;
        }
    }
}
