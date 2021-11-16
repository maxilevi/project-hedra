/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/06/2016
 * Time: 01:28 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Object to transfer player information.
    /// </summary>
    public class PlayerInformation
    {
        private Dictionary<int, Item> _items;
        private List<string> _learnedRecipes;
        private List<SerializedQuest> _quests;

        public PlayerInformation()
        {
            Level = 1;
            Health = int.MaxValue;
            Mana = int.MaxValue;
            _items = new Dictionary<int, Item>();
            _learnedRecipes = new List<string>();
            _quests = new List<SerializedQuest>();
            ToolbarData = new byte[4];
            SkillsData = new byte[20];
            RealmData = new byte[0];
        }

        public string Name { get; set; }
        public int Level { get; set; }
        public float Xp { get; set; }
        public float Mana { get; set; }
        public float Health { get; set; }
        public Vector3 Rotation { get; set; }
        public byte[] RealmData { get; set; }
        public byte[] SkillsData { get; set; }
        public byte[] ToolbarData { get; set; }
        public ClassDesign Class { get; set; }
        public CustomizationData Customization { get; set; }
        public float RandomFactor { get; set; }

        public KeyValuePair<int, Item>[] Items
        {
            get => _items.ToArray();
            set => _items = value.FromArray();
        }

        public string[] Recipes
        {
            get => _learnedRecipes.ToArray();
            set => _learnedRecipes = value.ToList();
        }

        public SerializedQuest[] Quests
        {
            get => _quests.ToArray();
            set => _quests = value.ToList();
        }

        public bool IsCorrupt => Rotation.IsInvalid() || Xp.IsInvalid() || Mana.IsInvalid() || Health.IsInvalid();

        public void AddItem(int Index, Item ItemSpecification)
        {
            _items.Add(Index, ItemSpecification);
        }

        public void AddRecipe(string RecipeName)
        {
            _learnedRecipes.Add(RecipeName);
        }
    }
}