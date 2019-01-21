using System.Collections.Generic;
using OpenTK.Input;

namespace Hedra.Engine.Localization
{
    public delegate void OnControlsChangedEvent();
    
    public class Controls
    {
        public const Key Respect = Key.F;
        public static event OnControlsChangedEvent OnControlsChanged;
        private static readonly Dictionary<string, Key> Mappings = new Dictionary<string, Key>
        {
            {"inventory_open_key", Key.I},
            {"interact_key", Key.E},
            {"forward_key", Key.W},
            {"leftward_key", Key.A},
            {"backward_key", Key.S},
            {"rightward_key", Key.D},
            {"climb_key", Key.ControlLeft},
            {"jump_key", Key.Space},
            {"descend_key", Key.ShiftLeft},
            {"handlamp_key", Key.F},
            {"respawn_key", Key.R},
            {"skilltree_key", Key.X},
            {"map_key", Key.M},
            {"eat_key", Key.Q},
            {"special_item_key", Key.G},
            {"crafting_key", Key.C},
            {"quest_log_key", Key.T},
        };
        
        public static Key InventoryOpen => Mappings["inventory_open_key"];
        public static Key Interact => Mappings["interact_key"];
        public static Key Forward => Mappings["forward_key"];
        public static Key Leftward => Mappings["leftward_key"];
        public static Key Backward => Mappings["backward_key"];
        public static Key Rightward => Mappings["rightward_key"];
        public static Key Climb => Mappings["climb_key"];
        public static Key Jump => Mappings["jump_key"];
        public static Key Descend => Mappings["descend_key"];
        public static Key Handlamp => Mappings["handlamp_key"];
        public static Key Respawn = Mappings["respawn_key"];
        public static Key Skilltree => Mappings["skilltree_key"];
        public static Key Map => Mappings["map_key"];
        public static Key Eat => Mappings["eat_key"];
        public static Key SpecialItem => Mappings["special_item_key"];
        public static Key Crafting => Mappings["crafting_key"];
        public static Key QuestLog => Mappings["quest_log_key"];

        public static void UpdateMapping(string Key, Key New)
        {
            Mappings[Key] = New;
        }

        public static KeyValuePair<string, Key>[] ChangeableKeys => Mappings.ToArray();
    }
}