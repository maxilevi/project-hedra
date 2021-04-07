using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Game;
using Newtonsoft.Json;
using Silk.NET.Input.Common;

namespace Hedra.Localization
{
    public delegate void OnControlsChangedEvent();
    
    public class Controls
    {
        public const Key Respect = Key.F;
        public static event OnControlsChangedEvent OnControlsChanged;
        private static readonly Dictionary<string, Key> DefaultMappings = new Dictionary<string, Key>
        {
            {"inventory_open_key", Key.I},
            {"interact_key", Key.E},
            {"forward_key", Key.W},
            {"leftward_key", Key.A},
            {"backward_key", Key.S},
            {"rightward_key", Key.D},
            {"climb_key", Key.AltLeft},
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
            {"open_chat_key", Key.Enter},
            {"sprint_key", Key.ControlLeft},
            {"help_key", Key.F4},
        };

        private static Dictionary<string, Key> Mappings = DefaultMappings;
        
        public static Key InventoryOpen => (Key) Mappings["inventory_open_key"];
        public static Key Interact => (Key) Mappings["interact_key"];
        public static Key Forward => (Key) Mappings["forward_key"];
        public static Key Leftward => (Key) Mappings["leftward_key"];
        public static Key Backward => (Key) Mappings["backward_key"];
        public static Key Rightward => (Key) Mappings["rightward_key"];
        public static Key Climb => (Key) Mappings["climb_key"];
        public static Key Jump => (Key) Mappings["jump_key"];
        public static Key Descend => (Key) Mappings["descend_key"];
        public static Key Handlamp => (Key) Mappings["handlamp_key"];
        public static Key Respawn = (Key) Mappings["respawn_key"];
        public static Key Skilltree => (Key) Mappings["skilltree_key"];
        public static Key Map => (Key) Mappings["map_key"];
        public static Key Eat => (Key) Mappings["eat_key"];
        public static Key SpecialItem => (Key) Mappings["special_item_key"];
        public static Key Crafting => (Key) Mappings["crafting_key"];
        public static Key QuestLog => (Key) Mappings["quest_log_key"];
        public static Key OpenChat => (Key) Mappings["open_chat_key"];
        public static Key Sprint => (Key) Mappings["sprint_key"];
        public static Key Help => (Key) Mappings["help_key"];

        public static void UpdateMapping(string Key, Key New)
        {
            Mappings[Key] = New;
            Save();
            OnControlsChanged?.Invoke();
        }

        public static Dictionary<string, Key> ChangeableKeys => Mappings;

        static Controls()
        {
            Mappings = File.Exists(SavePath) 
                ? Deserialize(SavePath)
                : new Dictionary<string, Key>(DefaultMappings);
        }

        private static Dictionary<string, Key> Deserialize(string Path)
        {
            var saved = JsonConvert.DeserializeObject<Dictionary<string, Key>>(File.ReadAllText(Path));
            foreach (var pair in DefaultMappings)
            {
                if(!saved.ContainsKey(pair.Key))
                    saved.Add(pair.Key, pair.Value);
            }

            return saved;
        }
        
        public static void Reset()
        {
            Mappings = new Dictionary<string, Key>(DefaultMappings);
            Save();
            OnControlsChanged?.Invoke();
        }
        
        private static void Save()
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(Mappings, Formatting.Indented));
        }

        private static string SavePath => $"{GameLoader.AppData}/controls.cfg";
    }
    
    public enum MouseKey
    {
        Middle
    }
}