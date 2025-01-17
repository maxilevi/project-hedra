using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Localization;
using SixLabors.ImageSharp;

namespace Hedra.Items
{
    public class RecipeHandler : ItemHandler
    {
        public override bool Consume(IPlayer Owner, Item Item)
        {
            bool result;
            if (!(result = Owner.Crafting.LearnRecipe(Item.Name)))
                Owner.MessageDispatcher.ShowNotification(Translations.Get("recipe_already_learned"), Color.Red, 4f);
            return result;
        }
    }
}