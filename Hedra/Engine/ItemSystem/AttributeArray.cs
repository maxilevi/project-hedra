using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ItemSystem
{
    public class AttributeArray
    {
        private readonly Dictionary<string, object> _attributes;

        public AttributeArray()
        {
            _attributes = new Dictionary<string, object>();
        }

        public void Set(string Attribute, object Value)
        {
            if (!_attributes.ContainsKey(Attribute))
                _attributes.Add(Attribute, Value);
            else
                _attributes[Attribute] = Value;
        }

        public T Get<T>(string Attribute)
        {
            if(!_attributes.ContainsKey(Attribute)) throw new KeyNotFoundException($"Provided attribute '{Attribute}' does not exist.");
            return (T)Convert.ChangeType(_attributes[Attribute], typeof(T));
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
                    Value = pair.Value
                });
            }
            return list.ToArray();
        }

        public void Clear()
        {
            _attributes.Clear();
        }
    }
}
