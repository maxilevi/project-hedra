using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;

namespace Hedra.Engine.CraftingSystem.Templates
{
    public class IngredientsTemplate
    {
        public string Name { get; set; }
        public int Amount { get; set; }

        public override string ToString()
        {
            return $"• {Amount} {ItemPool.Grab(Name).DisplayName}";
        }
    }
}