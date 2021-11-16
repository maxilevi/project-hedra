using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Game;
using Newtonsoft.Json;
using Silk.NET.Input;

namespace Hedra.Localization
{
    public delegate void OnControlsChangedEvent();

    public class Controls
    {
        public const Key Respect = Key.F;

        private static readonly Dictionary<string, Key> DefaultMappings = new Dictionary<string, Key>
        {
            { "inventory_open_key", Key.I },
            { "interact_key", Key.E },
            { "forward_key", Key.W },
            { "leftward_key", Key.A },
            { "backward_key", Key.S },
            { "rightward_key", Key.D },
            { "climb_key", Key.AltLeft },
            { "jump_key", Key.Space },
            { "descend_key", Key.ShiftLeft },
            { "handlamp_key", Key.F },
            { "respawn_key", Key.R },
            { "skilltree_key", Key.X },
            { "map_key", Key.M },
            { "eat_key", Key.Q },
            { "special_item_key", Key.G },
            { "crafting_key", Key.C },
            { "quest_log_key", Key.T },
            { "open_chat_key", Key.Enter },
            { "sprint_key", Key.ControlLeft },
            { "help_key", Key.F4 }
        };

        public static Key Respawn = ChangeableKeys["respawn_key"];

        static Controls()
        {
            ChangeableKeys = File.Exists(SavePath)
                ? Deserialize(SavePath)
                : new Dictionary<string, Key>(DefaultMappings);
        }

        public static Key InventoryOpen => ChangeableKeys["inventory_open_key"];
        public static Key Interact => ChangeableKeys["interact_key"];
        public static Key Forward => ChangeableKeys["forward_key"];
        public static Key Leftward => ChangeableKeys["leftward_key"];
        public static Key Backward => ChangeableKeys["backward_key"];
        public static Key Rightward => ChangeableKeys["rightward_key"];
        public static Key Climb => ChangeableKeys["climb_key"];
        public static Key Jump => ChangeableKeys["jump_key"];
        public static Key Descend => ChangeableKeys["descend_key"];
        public static Key Handlamp => ChangeableKeys["handlamp_key"];
        public static Key Skilltree => ChangeableKeys["skilltree_key"];
        public static Key Map => ChangeableKeys["map_key"];
        public static Key Eat => ChangeableKeys["eat_key"];
        public static Key SpecialItem => ChangeableKeys["special_item_key"];
        public static Key Crafting => ChangeableKeys["crafting_key"];
        public static Key QuestLog => ChangeableKeys["quest_log_key"];
        public static Key OpenChat => ChangeableKeys["open_chat_key"];
        public static Key Sprint => ChangeableKeys["sprint_key"];
        public static Key Help => ChangeableKeys["help_key"];

        public static Dictionary<string, Key> ChangeableKeys { get; private set; } = DefaultMappings;

        private static string SavePath => $"{GameLoader.AppData}/controls.cfg";
        public static event OnControlsChangedEvent OnControlsChanged;

        public static void UpdateMapping(string Key, Key New)
        {
            ChangeableKeys[Key] = New;
            Save();
            OnControlsChanged?.Invoke();
        }

        private static Dictionary<string, Key> Deserialize(string Path)
        {
            var saved = JsonConvert.DeserializeObject<Dictionary<string, Key>>(File.ReadAllText(Path));
            foreach (var pair in DefaultMappings)
                if (!saved.ContainsKey(pair.Key))
                    saved.Add(pair.Key, pair.Value);

            return saved;
        }

        public static void Reset()
        {
            ChangeableKeys = new Dictionary<string, Key>(DefaultMappings);
            Save();
            OnControlsChanged?.Invoke();
        }

        private static void Save()
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(ChangeableKeys, Formatting.Indented));
        }
    }

    public enum MouseKey
    {
        Middle
    }
}