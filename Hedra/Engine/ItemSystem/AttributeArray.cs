using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ItemSystem
{
    public class AttributeArray
    {
        private readonly Dictionary<string, AttributeObject> _attributes;

        public AttributeArray()
        {
            _attributes = new Dictionary<string, AttributeObject>();
        }

        public bool Has(string Attribute)
        {
            return _attributes.ContainsKey(Attribute);
        }

        public void Set(string Attribute, object Value)
        {
            this.Set(Attribute, Value, false, null);
        }

        public void Set(string Attribute, object Value, bool Hidden, string Display)
        {
            if (!_attributes.ContainsKey(Attribute))
                _attributes.Add(Attribute, new AttributeObject(Value, Hidden, Display));
            else
                _attributes[Attribute] = new AttributeObject(Value, Hidden, Display ?? _attributes[Attribute].Display);
        }

        public T Get<T>(string Attribute)
        {
            if(!_attributes.ContainsKey(Attribute)) throw new KeyNotFoundException($"Provided attribute '{Attribute}' does not exist.");
            return typeof(T).IsAssignableFrom(typeof(IConvertible)) || typeof(T).IsValueType
                ? (T) Convert.ChangeType(_attributes[Attribute].Value, typeof(T)) 
                : (T) _attributes[Attribute].Value;
        }

        public void Delete(string Attribute)
        {
            if(!_attributes.ContainsKey(Attribute)) return;
            _attributes.Remove(Attribute);
        }

        public AttributeTemplate[] Gather()
        {
            var list = new List<AttributeTemplate>();
            foreach (var pair in _attributes)
            {
                list.Add( new AttributeTemplate
                {
                    Name = pair.Key,
                    Value = pair.Value.Value,
                    Hidden = pair.Value.Hidden,
                    Display = pair.Value.Display
                });
            }
            return list.ToArray();
        }

        public void Clear()
        {
            _attributes.Clear();
        }
    }

    internal class AttributeObject
    {
        public object Value { get; set; }
        public bool Hidden { get; set; }
        public string Display { get; set; }

        public AttributeObject(object Value, bool Hidden, string Display)
        {
            this.Value = Value;
            this.Hidden = Hidden;
            this.Display = Display;
        }
    }
}
