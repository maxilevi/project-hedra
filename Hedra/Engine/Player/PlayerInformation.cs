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
    public class PlayerInformation
    {
        public string Name;
        public int Level;
        public int WorldSeed;
        public float Daytime;
        public float Xp;
        public float Mana;
        public float Health;
        public Vector3 BlockPosition;
        public Vector3 Rotation;
        public Vector3 TargetPosition;
        public byte[] AbilityTreeArray;
        public byte[] ToolbarArray;
        public ClassDesign Class;
        public float RandomFactor;
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
    }
}
