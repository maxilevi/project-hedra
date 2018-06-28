using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Audio;

namespace Hedra.Engine.IO
{
    internal class ConfigurationFile
    {
        private Dictionary<string, object> _values;

        public ConfigurationFile()
        {
            _values = new Dictionary<string, object>();
        }

        public T Get<T> (string Key)
        {
            if(typeof(T) != typeof(ValueType) && typeof(T) != typeof(string))
                throw new ArgumentException("Configuration files only support value types and strings.");

            if (_values.ContainsKey(Key))
                return (T) _values[Key];
            return default(T);
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
            foreach (var pair in _values)
            {
                builder.AppendLine(pair.Key + "=" + pair.Value);
            }
            File.WriteAllText(Path, builder.ToString());
        }

        public void Load(string Path)
        {
            if (!File.Exists(Path)) return;

            var values = new Dictionary<string, object>();
            string[] lines = File.ReadAllLines(Path);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('=');
                values.Add(parts[0], parts[1]);
            }
            _values = values;
        }
    }
}
