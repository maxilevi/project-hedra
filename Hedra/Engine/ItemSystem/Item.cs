using System;
using System.Text;
using Hedra.Core;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Localization;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.ItemSystem
{
    public class Item
    {
        private static string GoldItemName = "Gold";
        public string Name { get; set; }
        public ItemTier Tier { get; set; }
        public string EquipmentType { get; set; }
        public ItemModelTemplate ModelTemplate { get; private set; }
        private string _defaultDescription;
        private string _defaultDisplayName;
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
        
        public T GetAttribute<T>(CommonAttributes Attribute, T Default)
        {
            return HasAttribute(Attribute) ? this.GetAttribute<T>(Attribute.ToString()) : Default;
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
            this.SetAttribute(Attribute.ToString(), Value, Hidden, Display, false);
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
            this.SetAttribute(Attribute, Value, Hidden, Display, false);
        }

        public void SetAttribute(string Attribute, object Value, bool Hidden, string Display, bool Persist)
        {
            _attributes.Set(Attribute, Value, Hidden, Display, Persist);
        }

        public T GetAttribute<T>(string Attribute)
        {
            return _attributes.Get<T>(Attribute);
        }

        public object RawAttribute(string Attribute)
        {
            return _attributes.Raw(Attribute);
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
                this.SetAttribute(attribute.Name, attribute.Value, attribute.Hidden, attribute.Display, attribute.Persist);
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
            var savedTemplate = ItemTemplate.FromJson(Encoding.ASCII.GetString(Array));
            if (savedTemplate == null || !ItemPool.Exists(savedTemplate.Name)) return null;
            var savedItem = FromTemplate(savedTemplate);
            var item = ItemPool.Grab(savedItem.Name);
            CopyAttributes(savedItem, item);
            if (!item.HasAttribute(CommonAttributes.Seed)) 
                item.SetAttribute(CommonAttributes.Seed, Unique.RandomSeed(), true);
            return ItemPool.Randomize(item, new Random(item.GetAttribute<int>(CommonAttributes.Seed)));
        }

        private static Item CopyAttributes(Item SavedItem, Item NewItem)
        {
            var attributes = SavedItem.GetAttributes();
            for (var i = 0; i < attributes.Length; ++i)
            {
                NewItem.SetAttribute(attributes[i].Name, attributes[i].Value, attributes[i].Hidden, attributes[i].Display, attributes[i].Persist);
            }
            return NewItem;
        }


        public byte[] ToArray()
        {
            return Encoding.ASCII.GetBytes(ItemTemplate.ToJson(ItemTemplate.FromItem(this)));
        }

        public bool IsGold => Name == GoldItemName;
        public bool IsFood => HasAttribute(CommonAttributes.IsFood) && GetAttribute<bool>(CommonAttributes.IsFood) || Name == "Berry";
        public bool IsAmmo => string.Equals(EquipmentType, Items.EquipmentType.Ammo.ToString(), StringComparison.InvariantCultureIgnoreCase);
        public bool IsWeapon => WeaponFactory.Contains(this);
        public bool IsArmor => ArmorFactory.Contains(this);
        public bool IsRing => EquipmentType == Items.EquipmentType.Ring.ToString();
        public bool IsEquipment => IsWeapon || IsRing || IsArmor;
        public bool IsConsumable => HasAttribute(CommonAttributes.IsConsumable) && GetAttribute<bool>(CommonAttributes.IsConsumable);
        public bool IsRecipe => HasAttribute(CommonAttributes.Handler) && GetAttribute<string>(CommonAttributes.Handler) == "Recipe";
        
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

        public string DisplayName
        {
            get
            {
                var displayNameKey = $"item_{Name}_display_name";
                if (!Translations.IsEnglish && Translations.Has(displayNameKey))
                    return Translations.Get(displayNameKey);
                return _defaultDisplayName;
            }
            private set => _defaultDisplayName = value;
        }

        public string Description
        {
            get
            {
                var descriptionKey = $"item_{Name}_description";
                if (!Translations.IsEnglish && Translations.Has(descriptionKey))
                    return Translations.Get(descriptionKey);
                return _defaultDescription;
            }
            private set => _defaultDescription = value;
        }
    }
}
