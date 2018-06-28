/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/06/2016
 * Time: 01:28 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Collections.Generic;
using Hedra.Engine.ClassSystem;
using OpenTK;
using Hedra.Engine.ItemSystem;


namespace Hedra.Engine.Player
{
    /// <summary>
    /// Object to transfer player information.
    /// </summary>
    internal class PlayerInformation
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int WorldSeed { get; set; }
        public float Daytime { get; set; }
        public float Xp { get; set; }
        public float Mana { get; set; }
        public float Health { get; set; }
        public Vector3 BlockPosition { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 TargetPosition { get; set; }
        public byte[] AbilityTreeArray { get; set; }
        public byte[] ToolbarArray { get; set; }
        public ClassDesign Class { get; set; }
        public float RandomFactor { get; set; }
        private Dictionary<int, Item> _items;

        public PlayerInformation()
        {
            this.Level = 1;
            this.Daytime = 12000;
            this.Health = 100;
            this.BlockPosition = GameSettings.SpawnPoint.ToVector3();
            this._items = new Dictionary<int, Item>();
            this.AbilityTreeArray = new byte[0];
            this.ToolbarArray = new byte[4];
        }

        public void AddItem(int Index, Item ItemSpecification)
        {
            _items.Add(Index, ItemSpecification);

        }

        public KeyValuePair<int, Item>[] Items {
            get => _items.ToArray();
            set => _items = value.FromArray();
        }

        public bool IsCorrupt => 
            BlockPosition.IsInvalid() || Rotation.IsInvalid() || TargetPosition.IsInvalid() ||
            Daytime.IsInvalid() || Xp.IsInvalid() || Mana.IsInvalid() || Health.IsInvalid();
    }
}
