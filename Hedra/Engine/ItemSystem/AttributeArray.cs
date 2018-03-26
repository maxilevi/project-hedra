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
            this.Set(Attribute, Value, false);
        }

        public void Set(string Attribute, object Value, bool Hidden)
        {
            if (!_attributes.ContainsKey(Attribute))
                _attributes.Add(Attribute, new AttributeObject(Value, Hidden));
            else
                _attributes[Attribute] = new AttributeObject(Value, Hidden);
        }

        public T Get<T>(string Attribute)
        {
            if(!_attributes.ContainsKey(Attribute)) throw new KeyNotFoundException($"Provided attribute '{Attribute}' does not exist.");
            return (T)Convert.ChangeType(_attributes[Attribute].Value, typeof(T));
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
                    Hidden = pair.Value.Hidden
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

        public AttributeObject(object Value, bool Hidden)
        {
            this.Value = Value;
            this.Hidden = Hidden;
        }
    }
}
