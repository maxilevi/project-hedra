using System.Linq;
using System.Text;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ItemSystem
{
    public class Item
    {
        private static string GoldItemName = "Gold";
        private static string[] FoodItemNames = {"Berry"};
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ItemTier Tier { get; set; }
        public string EquipmentType { get; set; }
        public ItemModelTemplate ModelTemplate { get; set; }
        private readonly AttributeArray _attributes;
        private Weapon _weaponCache;
        private VertexData _model;
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
                Model = ItemModelLoader.Load(Template.Model)
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

        public void SetAttribute(CommonAttributes Attribute, object Value)
        {
            this.SetAttribute(Attribute.ToString(), Value);
        }

        public T GetAttribute<T>(CommonAttributes Attribute)
        {
            return this.GetAttribute<T>(Attribute.ToString());
        }

        public void DeleteAttribute(CommonAttributes Attribute)
        {
            this.DeleteAttribute(Attribute.ToString());
        }

        public void SetAttribute(string Attribute, object Value)
        {
            _attributes.Set(Attribute, Value);
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
                this.SetAttribute(attribute.Name, attribute.Value);
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
            var defaultTemplate = ItemFactory.Templater[savedTemplate.Name];
            savedTemplate.Model = defaultTemplate.Model;
            savedTemplate.Description = defaultTemplate.Description;
            savedTemplate.DisplayName = defaultTemplate.DisplayName;
            return FromTemplate(savedTemplate);
        }

        public byte[] ToArray()
        {
            return Encoding.ASCII.GetBytes(ItemTemplate.ToJson(ItemTemplate.FromItem(this)));
        }

        public bool IsGold => Name == Item.GoldItemName;
        public bool IsFood => FoodItemNames.Contains(Name);

        public VertexData Model
        {
            get { return _model; }
            set
            {
                _model = value;
                _weaponCacheDirty = true;
            }
        }

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
    }
}
