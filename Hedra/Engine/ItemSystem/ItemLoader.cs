using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.API;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem.Templates;
using Newtonsoft.Json;

namespace Hedra.Engine.ItemSystem
{
    public class ItemLoader : ModuleLoader<ItemLoader, ItemTemplate>
    {
        public static ItemTemplater Templater { get; private set; }

        public ItemLoader()
        {
            Templater = new ItemTemplater(Templates, Lock);
        }

        public void Load(params ItemTemplate[] NewTemplates)
        {
            for (var i = 0; i < NewTemplates.Length; ++i)
            {
                Templates.Add(NewTemplates[i].Name.ToLowerInvariant(), NewTemplates[i]);
            }
        }
        
        protected override string FolderPrefix => "Items";
    }
}
