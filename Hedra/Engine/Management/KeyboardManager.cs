using System;
using System.Collections.Generic;
using Hedra.Engine.Events;
using OpenTK.Input;
namespace Hedra.Engine.Management
{
    public class KeyboardManager
    {
        private readonly Dictionary<Key, bool> _mappings;

        public KeyboardManager()
        {
            _mappings = new Dictionary<Key, bool>();
            var length = Enum.GetNames(typeof(Key)).Length;
            for (var i = 0; i < length; i++)
            {
                _mappings.Add((Key) i, false);
            }

            EventDispatcher.RegisterKeyDown(this, (O,E) => _mappings[E.Key] = true);
            EventDispatcher.RegisterKeyUp(this, (O, E) => _mappings[E.Key] = false);
        }

        public bool this[Key Code] => _mappings[Code];
    }
}
