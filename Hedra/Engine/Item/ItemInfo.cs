/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 06/05/2016
 * Time: 12:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Item
{
	/// <summary>
	/// Description of ItemInfo.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2 )]
	public struct ItemInfo
	{
		[FieldOffset(0)]
		public Material MaterialType;
		[FieldOffset(4)]
		public float Damage;
		[FieldOffset(8)]
		public int ModelSeed;
		
		public ItemInfo(Material MaterialType, float Damage){
			this.MaterialType = MaterialType;
			MaterialInfo Info = ItemPool.MaterialInfo(MaterialType);
			this.Damage = Damage;
			this.ModelSeed = Utils.Rng.Next(-99999999,99999999);
		}
		
		public float CritMultiplier{
			get{
				Random Rng = new Random(ModelSeed + 737);
				return Rng.NextFloat() * 1.4f - .7f;
			}
		}
		
		private static Random Rng = new Random();
		public static ItemInfo Random(ItemType Type){
			if(Type == ItemType.Food)
				return ItemInfo.Berry( Rng.Next(1,4) );
			
			else if(Type == ItemType.Coin)
				return ItemInfo.Gold( Rng.Next(1, 10) );
			
			return Random(Type, Rng);
		}
		
		public static ItemInfo Random(ItemType Type, Random R){
			if(Type == ItemType.Stackable){
				return new ItemInfo((Material) R.Next( (int) Material.MaxItems, (int)Material.MaxItems+4), 1);
			}
            if (Type == ItemType.Food){
				return new ItemInfo(Material.Berry, 1 + R.Next(0, 5));
			}
            if (Type == ItemType.Coin){
				return new ItemInfo(Material.Iron, 1 + R.Next(0, 5));
			}
			return new ItemInfo((Material) R.Next( 2, (int)Material.MaxItems), R.NextFloat() * 2 + 5);
		}

		
		public static ItemInfo LowValue(Random Rng){
			int N = Rng.Next(0, 4);
			return new ItemInfo( ((N==0) ? Material.Copper : (N==1) ? Material.Amber : (N==2) ? Material.Quartz : Material.Andesine),
			                    6 + Rng.NextFloat() * 6);
		}
		
		public static ItemInfo Berry(float Amount){
			return new ItemInfo(Material.Berry, Amount);//Amount of berries
		}
		
		public static ItemInfo Gold(float Amount){
			return new ItemInfo(Material.Iron, Amount);//Amount of berries
		}
	}
	
	#region For Backwards compatibility
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
	public struct OldItemInfo
	{
		[FieldOffset(0)]
		public Material MaterialType;
		[FieldOffset(4)]
		public float Damage;
		
		public OldItemInfo(Material MaterialType, float Damage){
			this.MaterialType = MaterialType;
			MaterialInfo Info = ItemPool.MaterialInfo(MaterialType);
			this.Damage = Damage + Damage * (Info.AttackPower*0.01f);
		}
		
		static Random R = new Random();
		public static ItemInfo Random{
			get{ 
				return new ItemInfo((Material) R.Next(1, (int)Material.MaxItems), (float) R.NextDouble() * 15 + 5); 
			}
		}
	}
	#endregion
}
