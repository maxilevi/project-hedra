using Hedra.Items;

namespace Hedra.Crafting.Templates
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