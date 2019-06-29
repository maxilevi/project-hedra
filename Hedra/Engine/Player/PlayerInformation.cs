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
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Game;
using OpenTK;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Items;


namespace Hedra.Engine.Player
{
    /// <summary>
    /// Object to transfer player information.
    /// </summary>
    public class PlayerInformation
    {
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
        public float RandomFactor { get; set; }
        private Dictionary<int, Item> _items;
        private List<string> _learnedRecipes;
        private List<QuestTemplate> _quests;

        public PlayerInformation()
        {
            this.Level = 1;
            this.Health = int.MaxValue;
            this.Mana = int.MaxValue;
            this._items = new Dictionary<int, Item>();
            this._learnedRecipes = new List<string>();
            this._quests = new List<QuestTemplate>();
            this.ToolbarData = new byte[4];
            this.SkillsData = new byte[20];
            this.RealmData = new byte[0];
        }

        public void AddItem(int Index, Item ItemSpecification)
        {
            _items.Add(Index, ItemSpecification);
        }
        
        public void AddRecipe(string RecipeName)
        {
            _learnedRecipes.Add(RecipeName);
        }

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

        public QuestTemplate[] Quests
        {
            get => _quests.ToArray();
            set => _quests = value.ToList();
        }

        public bool IsCorrupt => Rotation.IsInvalid() || Xp.IsInvalid() || Mana.IsInvalid() || Health.IsInvalid();
    }
}
