using System;
using Hedra.Items;
using SixLabors.ImageSharp;

namespace Hedra.Engine.ItemSystem
{
    public static class ItemUtils
    {
        public static Color TierToColor(ItemTier Tier)
        {
            return
                Tier == ItemTier.Common || Tier == ItemTier.Misc ? Color.White :
                Tier == ItemTier.Uncommon ? Color.LawnGreen :
                Tier == ItemTier.Rare ? Color.CornflowerBlue :
                Tier == ItemTier.Unique ? Color.Magenta :
                Tier == ItemTier.Legendary ? Color.Gold :
                Tier == ItemTier.Divine ? Color.OrangeRed :
                throw new ArgumentOutOfRangeException();
        }
    }
}