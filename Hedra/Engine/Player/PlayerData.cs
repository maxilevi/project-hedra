/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/06/2016
 * Time: 01:28 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of PlayerData.
	/// </summary>
	[Serializable]
	public class PlayerData
	{
		public string Name, QuestData = "";
		public int Level = 1, WorldSeed;
		public float RandomFactor = 1, Daytime = 12000, XP = 0, Mana = 0, Health = 100, Speed = 2, SpeedMultiplier = 1;
		public Vector3 BlockPosition = Constants.WORLD_OFFSET * .5f, Rotation, TargetPosition = Vector3.Zero;
		public Dictionary<Item.InventoryItem, int> Items = new Dictionary<Item.InventoryItem, int>();
		public Vector4 Color0 = new Vector4(.2f,.2f,.2f,1), Color1 = new Vector4(.2f,.2f,.2f,1);
		public byte[] SkillsData = new byte[]{};
		public byte[] SkillIDs = new byte[4];
		public byte[] Entities = new byte[]{};
		public Class ClassType;
		public float AddonHealth;
		
		public PlayerData Clone(){
			 using (var ms = new MemoryStream())
			 {
			   var formatter = new BinaryFormatter();
			   formatter.Serialize(ms, this);
			   ms.Position = 0;
			
			   return (PlayerData) formatter.Deserialize(ms);
			 }
		}
	}
}
