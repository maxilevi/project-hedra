using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hedra.Engine.IO
{
    public class ConfigurationFile
    {
        private Dictionary<string, object> _values;

        public ConfigurationFile()
        {
            _values = new Dictionary<string, object>();
        }

        public T Get<T>(string Key)
        {
            if (typeof(T) != typeof(ValueType) && typeof(T) != typeof(string))
                throw new ArgumentException("Configuration files only support value types and strings.");

            if (_values.ContainsKey(Key))
                return (T)_values[Key];
            return default;
        }

        public void Set(string Key, object Value)
        {
            if (Value.GetType() != typeof(ValueType) && Value.GetType() != typeof(string))
                throw new ArgumentException("Configuration files only support value types and strings.");

            if (_values.ContainsKey(Key))
                _values[Key] = Value;
            else
                _values.Add(Key, Value);
        }

        public void Save(string Path)
        {
            var builder = new StringBuilder();
            foreach (var pair in _values) builder.AppendLine(pair.Key + "=" + pair.Value);
            File.WriteAllText(Path, builder.ToString());
        }

        public void Load(string Path)
        {
            if (!File.Exists(Path)) return;

            var values = new Dictionary<string, object>();
            var lines = File.ReadAllLines(Path);

            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split('=');
                values.Add(parts[0], parts[1]);
            }

            _values = values;
        }
    }
}