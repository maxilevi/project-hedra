using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem.Templates;

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

        public void Set(string Attribute, object Value, bool Hidden = false, string Display = null,
            bool Persist = false)
        {
            if (!_attributes.ContainsKey(Attribute))
                _attributes.Add(Attribute, new AttributeObject(Value, Hidden, Display, Persist));
            else
                _attributes[Attribute] = new AttributeObject(Value, _attributes[Attribute].Hidden,
                    Display ?? _attributes[Attribute].Display, _attributes[Attribute].Persist);
        }

        public object Raw(string Attribute)
        {
            return _attributes[Attribute].Value;
        }

        public T Get<T>(string Attribute)
        {
            if (!_attributes.ContainsKey(Attribute))
                throw new KeyNotFoundException($"Provided attribute '{Attribute}' does not exist.");
            if (typeof(T).IsEnum && _attributes[Attribute].Value is string)
                return (T)Enum.Parse(typeof(T), (string)_attributes[Attribute].Value, true);
            return typeof(T).IsAssignableFrom(typeof(IConvertible)) || typeof(T).IsValueType
                ? (T)Convert.ChangeType(_attributes[Attribute].Value, typeof(T))
                : (T)_attributes[Attribute].Value;
        }

        public void Delete(string Attribute)
        {
            if (!_attributes.ContainsKey(Attribute)) return;
            _attributes.Remove(Attribute);
        }

        public AttributeTemplate[] Gather()
        {
            var list = new List<AttributeTemplate>();
            foreach (var pair in _attributes)
                list.Add(new AttributeTemplate
                {
                    Name = pair.Key,
                    Value = pair.Value.Value,
                    Hidden = pair.Value.Hidden,
                    Display = pair.Value.Display,
                    Persist = pair.Value.Persist
                });
            return list.ToArray();
        }

        public void Clear()
        {
            _attributes.Clear();
        }
    }

    public class AttributeObject
    {
        public AttributeObject(object Value, bool Hidden, string Display, bool Persist)
        {
            this.Value = Value;
            this.Hidden = Hidden;
            this.Display = Display;
            this.Persist = Persist;
        }

        public object Value { get; set; }
        public bool Hidden { get; set; }
        public string Display { get; set; }
        public bool Persist { get; set; }
    }
}