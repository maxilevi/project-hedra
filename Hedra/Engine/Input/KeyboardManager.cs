using System;
using System.Collections.Generic;
using Hedra.Engine.Events;
using OpenToolkit.Windowing.Common.Input;

namespace Hedra.Engine.Input
{
    public class KeyboardManager : IDisposable
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

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
            EventDispatcher.UnregisterKeyUp(this);
        }
    }
}
