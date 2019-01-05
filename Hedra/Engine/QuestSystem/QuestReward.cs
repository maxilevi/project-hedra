using System;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestReward
    {
        public int Gold { get; set; }
        public int Experience { get; set; }
        public Item Item { get; set; }

        public bool HasGold => Gold != 0;
        public bool HasExperience => Experience != 0;
        public bool HasItem => Item != null;
    }
}
/*
 *      None = 0,
        Gold = 1,
        Recipes = 2,
        Experience = 4,
        Weapons = 8
 * 
 */