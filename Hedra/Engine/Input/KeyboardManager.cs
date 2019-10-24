using System;
using System.Collections.Generic;
using Hedra.Engine.Events;
using Silk.NET.Input.Common;

namespace Hedra.Engine.Input
{
    public class KeyboardManager : IDisposable
    {
        private readonly Dictionary<int, bool> _mappings;

        public KeyboardManager()
        {
            _mappings = new Dictionary<int, bool>();
            var values = Enum.GetValues(typeof(Key));
            for (var i = 0; i < values.Length; i++)
            {
                var key = (int)values.GetValue(i);
                if(!_mappings.ContainsKey(key))
                    _mappings.Add(key, false);
            }

            EventDispatcher.RegisterKeyDown(this, (O,E) => _mappings[(int)E.Key] = true);
            EventDispatcher.RegisterKeyUp(this, (O, E) => _mappings[(int)E.Key] = false);
        }

        public bool this[Key Code] => _mappings[(int)Code];

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
            EventDispatcher.UnregisterKeyUp(this);
        }
    }
}
