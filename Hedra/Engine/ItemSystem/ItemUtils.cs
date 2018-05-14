using System.Drawing;

namespace Hedra.Engine.ItemSystem
{
    public static class ItemUtils
    {
        public static Color TierToColor(ItemTier Tier)
        {
            return
                Tier == ItemTier.Common ? Color.FromArgb(255, 126, 126, 126) :
                    Tier == ItemTier.Uncommon ? Color.FromArgb(255, 0, 170, 76) :
                        Tier == ItemTier.Rare ? Color.FromArgb(255, 0, 142, 193) :
                            Tier == ItemTier.Unique ? Color.FromArgb(255, 172, 0, 230) :
                                Tier == ItemTier.Legendary ? Color.FromArgb(255, 219, 158, 28) :
                                    Tier == ItemTier.Divine ? Color.FromArgb(255, 255, 215, 0) :
                                        Color.Transparent;
        }
    }
}
