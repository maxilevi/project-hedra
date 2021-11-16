using Hedra.Engine.Core;
using Hedra.Engine.ItemSystem.Templates;

namespace Hedra.Engine.ItemSystem
{
    public class ItemLoader : ModuleLoader<ItemLoader, ItemTemplate>
    {
        public ItemLoader()
        {
            Templater = new ItemTemplater(Templates, Lock);
        }

        public static ItemTemplater Templater { get; private set; }

        protected override string FolderPrefix => "Items";

        public void Load(params ItemTemplate[] NewTemplates)
        {
            for (var i = 0; i < NewTemplates.Length; ++i)
                Templates.Add(NewTemplates[i].Name.ToLowerInvariant(), NewTemplates[i]);
        }
    }
}