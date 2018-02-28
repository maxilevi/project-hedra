/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/10/2016
 * Time: 09:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using System.Collections.Generic;
using Hedra.Engine.Management;

namespace Hedra.Engine.Item
{
	/// <summary>
	/// Description of ItemGenerator.
	/// </summary>
	public static class ItemGenerator
	{
		private const int BladeCount = 12;
		private const int HandleCount = 4;
		private const int HolderCount = 3;
		
		private const int RingCount = 6;
		private const int GemCount = 3;
		
		private const int BowHandlesCount = 5;
		private const int BowStructuresCount = 5;
		
		private const int AxeHandlesCount = 4;
		
		private const int HammerHandlesCount = 3;
		
		private const int ClawHandlesCount = 1;
		
		private const int KatarHandlesCount = 1;
		
		private static List<VertexData> Blades = new List<VertexData>();
		private static List<VertexData> Holders = new List<VertexData>();
		private static List<VertexData> Handles = new List<VertexData>();
		
		private static List<VertexData> Rings = new List<VertexData>();
		private static List<VertexData> Gems = new List<VertexData>();
		
		private static List<VertexData> BowHandles = new List<VertexData>();
		private static List<VertexData> BowStructures = new List<VertexData>();
		
		private static List<VertexData> AxeHandles = new List<VertexData>();
		
		private static List<VertexData> HammerHandles = new List<VertexData>();
		
		private static List<VertexData> ClawHandles = new List<VertexData>();
		
		private static List<VertexData> KatarHandles = new List<VertexData>();
		
		public static void Load(){
			for(int i = 0; i < BladeCount; i++){
				Blades.Add(AssetManager.PlyLoader("Assets/Items/Blade"+(i+1)+".ply", Vector3.One * 2));
			}
			
			for(int i = 0; i < HandleCount; i++){
				Handles.Add(AssetManager.PlyLoader("Assets/Items/Handle"+(i+1)+".ply", Vector3.One * 2));
			}
			
			for(int i = 0; i < HolderCount; i++){
				Holders.Add(AssetManager.PlyLoader("Assets/Items/Holder"+(i+1)+".ply", Vector3.One * 2));
			}
			
			for(int i = 0; i < RingCount; i++){
				Rings.Add(AssetManager.PlyLoader("Assets/Items/Ring"+(i+1)+".ply", Vector3.One * 2.9f));
			}
			
			for(int i = 0; i < GemCount; i++){
				Gems.Add(AssetManager.PlyLoader("Assets/Items/Gem"+(i+1)+".ply", Vector3.One * 3.65f));
			}
			
			for(int i = 0; i < BowHandlesCount; i++){
				BowHandles.Add(AssetManager.PlyLoader("Assets/Items/BowHandle"+(i)+".ply", Vector3.One * 2.0f));
			}
			
			for(int i = 0; i < BowStructuresCount; i++){
				BowStructures.Add(AssetManager.PlyLoader("Assets/Items/BowStructure"+(i)+".ply", Vector3.One * 2.0f));
			}
			
			for(int i = 0; i < AxeHandlesCount; i++){
				AxeHandles.Add(AssetManager.PlyLoader("Assets/Items/AxeHandle"+(i)+".ply", Vector3.One * 2.0f));
			}
			
			for(int i = 0; i < HammerHandlesCount; i++){
				HammerHandles.Add(AssetManager.PlyLoader("Assets/Items/HammerHandle"+(i)+".ply", Vector3.One * 2.0f));
			}
			
			for(int i = 0; i < ClawHandlesCount; i++){
				ClawHandles.Add(AssetManager.PlyLoader("Assets/Items/Claw"+(i)+".ply", Vector3.One * 2.5f));
			}
			
			for(int i = 0; i < KatarHandlesCount; i++){
				KatarHandles.Add(AssetManager.PlyLoader("Assets/Items/Katar"+(i)+".ply", Vector3.One * 2.5f));
			}
		}
		
		private static Dictionary<int, VertexData> SwordCache = new Dictionary<int, VertexData>();
		private static Dictionary<int, VertexData> RingCache = new Dictionary<int, VertexData>();
		private static Dictionary<int, VertexData> BowCache = new Dictionary<int, VertexData>();
		private static Dictionary<int, VertexData> HammerCache = new Dictionary<int, VertexData>();
		private static Dictionary<int, VertexData> AxeCache = new Dictionary<int, VertexData>();
		private static Dictionary<int, VertexData> ClawCache = new Dictionary<int, VertexData>();
		private static Dictionary<int, VertexData> KatarCache = new Dictionary<int, VertexData>();
		public static VertexData GenerateItemModel(int Seed, ItemType Type){
			
			if(Type == ItemType.Sword || Type == ItemType.ThrowableDagger || Type == ItemType.Knife || Type == ItemType.DoubleBlades){
				if(SwordCache.ContainsKey(Seed))
					return SwordCache[Seed];
				
				
				VertexData Data = null;
				Random Rng = new Random(Seed);
				Vector4 HandleC = Utils.VariateColor(HandleColor(Rng), 15, Rng);
				Vector4 HolderC = Utils.VariateColor(HolderColor(Rng), 15, Rng);
				
				int a = Rng.Next(0, Handles.Count);
				Data += Blades[Rng.Next(0, Blades.Count)];
				Data += Handles[a];
				Data += Holders[Rng.Next(0, Holders.Count)];
				
				Data.Color(AssetManager.ColorCode0, HandleC * 1.2f);
				Data.Color(AssetManager.ColorCode1, HolderC * 1.1f);
				
				SwordCache.Add(Seed, Data.Clone());	
				
				return Data;
			}
			else if(Type == ItemType.Ring){
				if(RingCache.ContainsKey(Seed))
					return RingCache[Seed];
				
				VertexData Data = null;
				Random Rng = new Random(Seed);
				Vector4 RingC = Utils.VariateColor(RingColor(Rng), 15, Rng);
				Vector4 GemC = Utils.VariateColor(GemColor(Rng), 15, Rng);
				
				Data += Gems[Rng.Next(0, Gems.Count)];
				Data.Transform(-Vector3.UnitY * .2f);
				Data += Rings[Rng.Next(0, Rings.Count)];
				
				Data.Color(AssetManager.ColorCode0, GemC * 1.2f);
				Data.Color(AssetManager.ColorCode1, RingC * 1.1f);
				Data.Color(AssetManager.ColorCode2, RingC * 1.3f);
				
				Data.Transform(new Vector3(0,0.5f,0)); // Center it
				
				RingCache.Add(Seed, Data.Clone());
				
				return Data;
			}
			else if(Type == ItemType.Bow){
				if(BowCache.ContainsKey(Seed))
					return BowCache[Seed];
				
				VertexData Data = null;
				Random Rng = new Random(Seed);
				Vector4 WoodColor = Utils.UniformVariateColor(BowColor(Rng), 10, Rng);
				
				Data += BowHandles[Rng.Next(0, BowHandles.Count)];
				Data += BowStructures[Rng.Next(0, BowStructures.Count)];
				
				Data.Color(AssetManager.ColorCode1, WoodColor);
				
				//Data.Transform(new Vector3(0,0.5f,0)); // Center it
				
				BowCache.Add(Seed, Data.Clone());
				
				return Data;
			}
			else if(Type == ItemType.Axe){
				if(AxeCache.ContainsKey(Seed))
					return AxeCache[Seed];
				
				VertexData Data = null;
				Random Rng = new Random(Seed);
				Vector4 WoodColor = Utils.UniformVariateColor(BowColor(Rng), 10, Rng);
				
				Data += AxeHandles[Rng.Next(0, AxeHandles.Count)];
				
				Data.Color(AssetManager.ColorCode0, WoodColor);
				
				AxeCache.Add(Seed, Data.Clone());
				
				return Data;
			}
			else if(Type == ItemType.Hammer){
				if(HammerCache.ContainsKey(Seed))
					return HammerCache[Seed];
				
				VertexData Data = null;
				Random Rng = new Random(Seed);
				Vector4 WoodColor = Utils.UniformVariateColor(BowColor(Rng), 10, Rng);
				
				Data += HammerHandles[Rng.Next(0, HammerHandles.Count)];
				
				Data.Color(AssetManager.ColorCode0, WoodColor);
				
				HammerCache.Add(Seed, Data.Clone());
				
				return Data;
			}else if(Type == ItemType.Katar){
				if(KatarCache.ContainsKey(Seed))
					return KatarCache[Seed];
				
				VertexData Data = null;
				Random Rng = new Random(Seed);
				Vector4 SilverColor = Utils.UniformVariateColor(new Vector4(.4f,.4f,.4f,1), 10, Rng);
				
				Data += KatarHandles[Rng.Next(0, KatarHandles.Count)];
				
				Data.Color(AssetManager.ColorCode0, SilverColor);
				
				Data.Transform(new Vector3(0,-0.25f,0));
				
				KatarCache.Add(Seed, Data.Clone());
				
				return Data;
			}else if(Type == ItemType.Claw){
				if(ClawCache.ContainsKey(Seed))
					return ClawCache[Seed];
				
				VertexData Data = null;
				Random Rng = new Random(Seed);
				Vector4 SilverColor = Utils.UniformVariateColor(new Vector4(.4f,.4f,.4f,1), 10, Rng);
				
				Data += ClawHandles[Rng.Next(0, ClawHandles.Count)];
				
				Data.Color(AssetManager.ColorCode0, SilverColor);
				
				Data.Transform(new Vector3(0,-0.25f,0));
				
				ClawCache.Add(Seed, Data.Clone());
				
				return Data;
			}
			return null;
		}
		
		
		private static Vector4 BowColor(Random Rng){
			int R = 0;//Rng.Next(0, 3);
			switch(R){
				case 0: return Colors.FromHtml("#735545");
				case 1: return Colors.FromHtml("#2B1E16");
				case 2: return Colors.FromHtml("#776F6A");
			}
			return Vector4.One;
		}
		
		private static Vector4 GemColor(Random Rng){
			int R = Rng.Next(0, 7);
			switch(R){
				case 0: return new Vector4(0.988f, 0.643f, 0.447f, 1.000f);
				case 1: return new Vector4(0.043f, 0.647f, 0.537f, 1.000f);
				case 2: return new Vector4(0.969f, 0.176f, 0.278f, 1.000f);
				case 3: return new Vector4(0.471f, 0.761f, 0.804f, 1.000f);
				case 4: return new Vector4(0.369f, 0.698f, 0.122f, 1.000f);
				case 5: return new Vector4(0.416f, 0.416f, 0.416f, 1.000f);
				case 6: return new Vector4(0.856f, 0.856f, 0.856f, 1.000f);				
			}
			return Vector4.One;
		}
		
		private static Vector4 RingColor(Random Rng){
			int R = Rng.Next(0, 4);
			switch(R){
				case 0: return new Vector4(0.765f, 0.514f, 0.404f, 1.000f);
				case 1: return new Vector4(0.894f, 0.757f, 0.357f, 1.000f);
				case 2: return new Vector4(0.925f, 0.863f, 0.722f, 1.000f);
				case 3: return new Vector4(0.863f, 0.627f, 0.416f, 1.000f);
				case 4: return new Vector4(0.918f, 0.867f, 0.804f, 1.000f);
			}
			return Vector4.One;
		}
		
		private static Vector4 HandleColor(Random Rng){
			int R = Rng.Next(0, 4);
			switch(R){
				case 0: return new Vector4(0.384f, 0.369f, 0.325f, 1.000f);
				case 1: return new Vector4(0.241f, 0.249f, 0.237f, 1.000f);
				case 2: return new Vector4(0.424f, 0.310f, 0.286f, 1.000f);
				case 3: return new Vector4(0.545f, 0.231f, 0.157f, 1.000f);
			}
			return Vector4.One;
		}
		
		private static Vector4 HolderColor(Random Rng){
			int R = Rng.Next(0, 5);
			switch(R){
				case 0: return new Vector4(0.682f, 0.753f, 0.769f, 1.000f);
				case 1: return new Vector4(0.208f, 0.216f, 0.212f, 1.000f);
				case 2: return new Vector4(0.298f, 0.322f, 0.322f, 1.000f);
				case 3: return new Vector4(0.498f, 0.435f, 0.306f, 1.000f);
				case 4: return new Vector4(0.667f, 0.518f, 0.286f, 1.000f);
				case 5: return new Vector4(0.878f, 0.776f, 0.557f, 1.000f);
			}
			return Vector4.One;
		}
	}
}
