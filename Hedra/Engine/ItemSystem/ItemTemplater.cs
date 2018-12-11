using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.ItemSystem.Templates;

namespace Hedra.Engine.ItemSystem
{
    public class ItemTemplater
    {
        private static Dictionary<string, ItemTemplate> _itemTemplates;
        private static object _lock;

        public ItemTemplater(Dictionary<string, ItemTemplate> TemplatesDict, object Lock)
        {
            _itemTemplates = TemplatesDict;
            ItemTemplater._lock = Lock;
        }

        public ItemTemplate[] Templates
        {
            get
            {
                lock (_lock)
                {
                    return _itemTemplates.Values.ToArray();
                }
            }
        }

        public ItemTemplate this[string Key]
        {
            get
            {
                lock (_lock)
                {
                    return _itemTemplates[Key.ToLowerInvariant()];
                }
            }
        }

        public bool Contains(string Key)
        {
            lock (_lock)
            {
                return _itemTemplates.ContainsKey(Key.ToLowerInvariant());
            }
        }
    }
}
