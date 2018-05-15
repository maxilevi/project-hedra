using System.Drawing;

namespace Hedra.Engine.ItemSystem
{
    public static class ItemUtils
    {
        public static Color TierToColor(ItemTier Tier)
        {
            int a = 0;
            return
                Tier == ItemTier.Common ? Color.FromArgb(255, 126, 126, 126) :
                    Tier == ItemTier.Uncommon ? Color.LawnGreen :
                        Tier == ItemTier.Rare ? Color.DodgerBlue :
                            Tier == ItemTier.Unique ? Color.Magenta :
                                Tier == ItemTier.Legendary ? Color.Gold :
                                    Tier == ItemTier.Divine ? Color.OrangeRed :
                                        Color.Transparent;
        }
    }
}
