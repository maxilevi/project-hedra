/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 26/04/2016
 * Time: 10:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using System.Drawing;
using System.Text;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK;

namespace Hedra.Engine.Item
{
	/// <summary>
	/// Description of ItemPool.
	/// </summary>
	public static class ItemPool
	{
		
		public static VertexData GetItem(ItemType Type, ItemInfo Info){
			
			VertexData data;
			
			if(Type == ItemType.Glider){
				data = AssetManager.PlyLoader("Assets/Items/Glider.ply", Vector3.One * .5f);
				return data;	
			}
			
			if(Type == ItemType.Food){
				if(Info.MaterialType == Material.Berry){
					data = AssetManager.PlyLoader("Assets/Items/Berry.ply", Vector3.One * 1.4f);
					return data;
				}else{
					//Defualt make it a berry
					data = AssetManager.PlyLoader("Assets/Items/Berry.ply", Vector3.One * 1.4f);
					return data;
				}
			}
			
			if(Type == ItemType.Stackable){
				switch (Info.MaterialType)
				{
				    case Material.BoarTusk:
				        data = AssetManager.PlyLoader("Assets/Items/BoarTusk.ply", Vector3.One * 4.5f);
				        return data;

				    case Material.TurtleShell:
				        data = AssetManager.PlyLoader("Assets/Items/TurtleShell.ply", Vector3.One * 1.4f);
				        return data;

				    case Material.SpiderEye:
				        data = AssetManager.PlyLoader("Assets/Items/SpiderEyes.ply", Vector3.One * 1.4f);
				        return data;

				    case Material.RatTail:
				        data = AssetManager.PlyLoader("Assets/Items/RatTail.ply", Vector3.One * 4.5f);
				        return data;
                         
				    case Material.GoatHorn:
				        data = AssetManager.PlyLoader("Assets/Items/GoatHorn.ply", Vector3.One * 4.5f);
				        return data;
                }
			}
			
			if(Type == ItemType.Mount){
				data = null;
				if(Info.MaterialType == Material.HorseMount){
					string fileContents = Encoding.ASCII.GetString(AssetManager.ReadBinary("Assets/Chr/HorseIdle.dae", AssetManager.DataFile3));
					data = ColladaLoader.LoadColladaModel(fileContents, GeneralSettings.MaxWeights).Mesh.ToVertexData();
					data.Scale(Vector3.One * .65f);
					data.Transform(-Vector3.UnitY * .5f);
				}
				if(Info.MaterialType == Material.WolfMount){
					string fileContents = Encoding.ASCII.GetString(AssetManager.ReadBinary("Assets/Chr/WolfIdle.dae", AssetManager.DataFile3));
					data = data = ColladaLoader.LoadColladaModel(fileContents, GeneralSettings.MaxWeights).Mesh.ToVertexData();
					data.Scale(Vector3.One * .65f);
					data.Transform(-Vector3.UnitY * .5f);
				}
				return data;
			}
			
			if(Type == ItemType.Coin){
				data = null;
				if(Info.MaterialType == Material.Iron){
					data = AssetManager.PlyLoader("Assets/Items/Coin.ply", Vector3.One * 1.5f);
					data.Transform( Matrix4.CreateRotationX(-90 * Mathf.Radian) );
					data.Transform( Vector3.UnitY * .8f );
					return data;
				}
				return data;
			}

			data = ItemGenerator.GenerateItemModel(Info.ModelSeed ,Type);
			
			if(Type == ItemType.ThrowableDagger || Type == ItemType.Knife){
				data = data.Clone();
				data.Scale( new Vector3(.5f,.5f,.5f) );
			}
			if(Type == ItemType.DoubleBlades){
				data = data.Clone();
				data.Scale( new Vector3(.75f,.75f,.75f) );
			}
			if(Type == ItemType.Bow)
				data.Color(AssetManager.ColorCode2, MaterialColor(Info.MaterialType) * 0.8f );
			else if(Type == ItemType.Claw)
				data.Color(AssetManager.ColorCode1, MaterialColor(Info.MaterialType) * 1.4f );
			else
				data.Color(AssetManager.ColorCode2, MaterialColor(Info.MaterialType) * 1.4f );
			//Data.GraduateColor(Vector3.UnitY, .3f);
			return data;
		}
		
		#region Materials
  		public static Vector4 MaterialColor(Material Material){
			switch(Material){
				case Material.Sapphire:
					return Colors.FromArgb(255, 102, 114, 216);
					
				case Material.Arsenic: 
					return Colors.FromArgb(255, 186, 48, 2);
					
				case Material.Calcite:
					return Colors.FromArgb(255, 196, 191, 162);
					
				case Material.Quartz:
					return Colors.FromArgb(255, 254, 247, 239);
					
				case Material.Gold:
					return Colors.FromArgb(255, 255, 200, 82);
					
				case Material.Iron:
					return Colors.FromArgb(255, 139, 144, 137);
					
				case Material.Fluorite:
					return Colors.FromArgb(255, 255, 102, 101);
					
				case Material.Beryl:
					return Colors.FromArgb(255, 165, 229, 80);
					
				case Material.Andesine:
					return Colors.FromArgb(255, 219, 28, 36);
					
				case Material.Amber:
					return Colors.FromArgb(255, 254, 145, 3);
					
				case Material.Copper:
					return Colors.FromArgb(255, 200, 117, 51);
					
				default:
					return Colors.Transparent;
				
			}
		}
		
		public static MaterialInfo MaterialInfo(Material Material){
			switch(Material){
				case Material.Sapphire:
					return new MaterialInfo(11, 115, -.05f, EffectType.Freeze);
					
				case Material.Arsenic:
					return new MaterialInfo(14, 90, .05f, EffectType.Fire);
					
				case Material.Calcite:
					return new MaterialInfo(13, 95, 0, EffectType.Slow);
					
				case Material.Quartz:
					return new MaterialInfo(9, 110, .2f, EffectType.None);
					
				case Material.Gold:
					return new MaterialInfo(8, 120, .1f, EffectType.None);
					
				case Material.Iron:
					return new MaterialInfo(9, 100, -.2f, EffectType.None);
					
				case Material.Fluorite:
					return new MaterialInfo(12, 110, -.10f, EffectType.Fire);
					
				case Material.Beryl:
					return new MaterialInfo(13, 105, -.075f, EffectType.Poison);
					
				case Material.Andesine:
					return new MaterialInfo(12, 95, .15f, EffectType.Freeze);
					
				case Material.Amber:
					return new MaterialInfo(9, 97.5f, .10f, EffectType.Bleed);
					
				case Material.Copper:
					return new MaterialInfo(8, 100f, 0f, EffectType.None);
					
				case Material.Berry:
					return new MaterialInfo(150, 0, 0, EffectType.None);
					
				default:
					return new MaterialInfo(0,0,0, EffectType.None);
			}
		}
		#endregion
	}

	
	public enum ItemType{
		Sword,
		Ring,
		Glider,
		Food,
		Stackable,
		Bow,
		Mount,
		ThrowableDagger,
		Axe,
		Hammer,
		Knife,
		Katar,
		Claw,
		DoubleBlades,
		Coin,
		MaxItems,
		Random
	}
	/* Atributes:
	 * Attack Power :
	 * Attack Speed :
	 * Movement Speed :
	 * Special Effect :
	 * */
	
	
	public enum Material{
		None,
		Berry,
		Sapphire,
		Arsenic,
		Calcite,
		Quartz,
		Gold,
		Iron,
		Fluorite,
		Beryl,
		Andesine,
		Amber,
		MaxItems,
		BoarTusk,
		TurtleShell,
		SpiderEye,
		RatTail,
		Copper,
		HorseMount,
		WolfMount,
        Mount,
        GoatHorn
	}
	
	public enum EffectType{
		None,
		Bleed,
		Fire,
		Poison,
		Freeze,
		Speed,
		Slow,
		MaxItems
	}
	
}
