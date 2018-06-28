using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.ItemSystem
{
    internal class ItemTemplater
    {
        private static Dictionary<string, ItemTemplate> _itemTemplates;

        public ItemTemplater(Dictionary<string, ItemTemplate> TemplatesDict)
        {
            _itemTemplates = TemplatesDict;
        }

        public ItemTemplate[] Templates => _itemTemplates.Values.ToArray();
        public ItemTemplate this[string Key] => _itemTemplates[Key.ToLowerInvariant()];
    }
}
