/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 31/01/2017
 * Time: 11:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Item;
using Hedra.Engine.Player;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of Packet0x5.
	/// </summary>
	[Serializable]
	public class Packet0x5
	{
		public static float PrevHealth, PrevMaxHealth;
		public static int PrevLevel;
		public static InventoryItem PrevWeaponItem;
		
		public float Health, MaxHealth;
		public int Level;
		public InventoryItem WeaponItem;
		
		public static Packet0x5 FromHuman(Humanoid Human){
			Packet0x5 Packet = new Packet0x5();
			Packet.Health = Human.Health;
			Packet.MaxHealth = Human.MaxHealth;
			Packet.Level = Human.Level;
			Packet.WeaponItem = Human.MainWeapon;
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x5 Packet){
			//Human.MaxHealth = Packet.MaxHealth;
			Human.Health = Packet.Health;
			if(Packet.WeaponItem != Human.MainWeapon)
				Human.MainWeapon = Packet.WeaponItem;
			if(Packet.WeaponItem != null)
			ThreadManager.ExecuteOnMainThread( () => Human.Model.SetWeapon(Human.MainWeapon.Weapon) );
			Human.Level = Packet.Level;
		}
	}
}
