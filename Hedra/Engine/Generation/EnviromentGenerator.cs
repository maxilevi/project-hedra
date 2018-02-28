/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/11/2016
 * Time: 09:24 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of HerbGenerator.
	/// </summary>
	public class EnviromentGenerator
	{
		
		public void GeneratePlant(int x, int y, int z, Chunk UnderChunk, PlantType Type){

			var rng = new Random(World.Seed + 1324 + UnderChunk.OffsetX + UnderChunk.OffsetZ + x + y);
			var position = new Vector3(UnderChunk.OffsetX + x * Chunk.BlockSize, y, UnderChunk.OffsetZ + z * Chunk.BlockSize);
			
			if(!UnderChunk.Initialized) return;
			
			#region GRASS & WHEAT
			if(Type == PlantType.GRASS || Type == PlantType.WHEAT || Type == PlantType.ALGAE){
				Vector3 Addon = new Vector3(Utils.Rng.NextFloat() * 8f, 0, Utils.Rng.NextFloat() * 8f);
				
				//Clamp it so there is no problem with the neighbour chunks
				if(x + Addon.X / Chunk.BlockSize > 15) Addon.X = 0;
				if(z + Addon.Z / Chunk.BlockSize > 15) Addon.Z = 0;

			    float Height = Physics.HeightAtPosition(position + Addon);
			    Block topBlock = World.GetHighestBlockAt( (int)(position.X + Addon.X), (int)(position.Z + Addon.Z));
                if(topBlock.Noise3D) return;
                for (int _x = -1; _x < 1; _x++)
			    {
			        if (_x == 0) continue;
                    float blockDens = Physics.HeightAtPosition(new Vector3((x + _x) * Chunk.BlockSize + UnderChunk.OffsetX, 0, (z + 0) * Chunk.BlockSize + UnderChunk.OffsetZ));
			        float difference = Math.Abs(blockDens - Height);
			        if (difference > 6)
			            return;
                }

			    for (int _z = -1; _z < 1; _z++)
			    {
                    if(_z == 0) continue;
			        float blockDens = Physics.HeightAtPosition(new Vector3((x + 0) * Chunk.BlockSize + UnderChunk.OffsetX, 0, (z + _z) * Chunk.BlockSize + UnderChunk.OffsetZ));
			        float difference = Math.Abs(blockDens - Height);
			        if (difference > 6)
			            return;
			    }
			    var region = World.BiomePool.GetRegion(position + Addon);
                Matrix4 RotationMat4 = Matrix4.CreateRotationY(360 * Utils.Rng.NextFloat());
				Vector4 NewColor = new Vector4((region.Colors.GrassColor * 1.25f).Xyz,1);
				
				Matrix4 TransMatrix = Matrix4.CreateScale(6.0f + Utils.Rng.NextFloat() * .5f);
				TransMatrix *= RotationMat4;
				TransMatrix *= Matrix4.CreateTranslation( new Vector3(position.X, Height, position.Z) + Addon );
	
				if(Type == PlantType.GRASS){
					VertexData Model = CacheManager.GetModel(CacheItem.Grass).Clone();
					
					//Do CPU intensive stuff manually to optimize it

					Model.ReColor(NewColor);
				    Vector3 Highest = CacheManager.GetModel(CacheItem.Grass).SupportPoint(Vector3.UnitY);
				    Vector3 Lowest = CacheManager.GetModel(CacheItem.Grass).SupportPoint(-Vector3.UnitY);
				    float dot = Vector3.Dot(Highest - Lowest, Vector3.UnitY);

                    for (int i = 0; i < Model.Vertices.Count; i++)
				    {
				        float Shade = Vector3.Dot(Model.Vertices[i] - Lowest, Vector3.UnitY) / dot;
				        Model.Colors[i] += new Vector4(.3f, .3f, .3f, 0) * Shade;
				    }
                    //End CPU Intensive

				    var data = new InstanceData
				    {
				        MeshCache = CacheManager.GetModel(CacheItem.Grass),
				        Colors = Model.Colors.Clone(),
				        ExtraData = Model.ExtraData.Clone(),
				        TransMatrix = TransMatrix
				    };
				    CacheManager.Check(data);
					
					UnderChunk.Mesh.AddInstance(data);
					
					Model.Dispose();
					
				}
				
				else if(Type == PlantType.WHEAT){
					VertexData Model = CacheManager.GetModel(CacheItem.Wheat).Clone();
					Model.Color(AssetManager.ColorCode1, WheatColor(rng));
					Model.Color(AssetManager.ColorCode0, NewColor);

				    Vector3 Highest = CacheManager.GetModel(CacheItem.Wheat).SupportPoint(Vector3.UnitY);
				    Vector3 Lowest = CacheManager.GetModel(CacheItem.Wheat).SupportPoint(-Vector3.UnitY);
				    float dot = Vector3.Dot(Highest - Lowest, Vector3.UnitY);
				    for (int i = 0; i < Model.Vertices.Count; i++)
				    {
				        float Shade = Vector3.Dot(Model.Vertices[i] - Lowest, Vector3.UnitY) / dot;
				        Model.Colors[i] += new Vector4(.3f, .3f, .3f, 0) * Shade;
				    }

                    InstanceData Data = new InstanceData();
					Data.MeshCache = CacheManager.GetModel(CacheItem.Wheat);// Pointer to the model
					Data.Colors = Model.Colors.Clone();
					Data.ExtraData = Model.ExtraData.Clone();
					Data.TransMatrix = TransMatrix;
					CacheManager.Check(Data);
					
					UnderChunk.StaticBuffer.AddInstance(Data);
					
					Model.Dispose();
				}
			}
			#endregion
			
			#region BUSH & FERN & ROCK
			else if (Type == PlantType.WHEAT || Type == PlantType.FERN || Type == PlantType.ROCK || Type == PlantType.BUSH){
				Vector3 Addon = new Vector3(rng.NextFloat() * 4f, 0, rng.NextFloat() * 4f);
				
				if(x + Addon.X / Chunk.BlockSize > 15) Addon.X = 0;
				if(z + Addon.Z / Chunk.BlockSize > 15) Addon.Z = 0;
				
				float Height = Physics.HeightAtPosition(position);
				
				//Spanw the bush in a plane
				for(int _x = -3; _x < 3; _x++){
					for(int _z = -3; _z < 3; _z++){
						float BDens = Physics.HeightAtPosition(new Vector3( (x+_x) * Chunk.BlockSize + UnderChunk.OffsetX, 0, (z+_z) * Chunk.BlockSize + UnderChunk.OffsetZ));
						float Difference = Math.Abs(BDens - Height);
						if(Difference > 5f)
							return;
					}
				}
				
				VertexData Model;
				VertexData OriginalModel = null;
				if(Type == PlantType.BUSH)
					OriginalModel = CacheManager.GetModel(CacheItem.Bushes);
                else if(Type == PlantType.FERN)
					OriginalModel = CacheManager.GetModel(CacheItem.Ferns);
                else if(Type == PlantType.ROCK)
					OriginalModel = CacheManager.GetModel(CacheItem.Rock);

                Model = OriginalModel.Clone();
				
				Matrix4 RotY = Matrix4.CreateRotationY(360 * Utils.Rng.NextFloat());
				Vector4 NewColor = Vector4.One;
				if(Type == PlantType.BUSH || Type == PlantType.FERN)
					NewColor = UnderChunk.Biome.Colors.GrassColor * ((Type == PlantType.FERN) ? .7f : .8f);
				
				else if(Type == PlantType.ROCK)
					NewColor = RockColor(rng);
					
				Model.ReColor(NewColor);
				
				Model.GraduateColor(Vector3.UnitY);
				float Scale = 1;
				if(Type == PlantType.FERN)
					Scale = 3.75f + rng.NextFloat() * .75f;
				else if(Type == PlantType.BUSH)
					Scale = 1.75f + rng.NextFloat() * .75f;
				else
					Scale = 2.75f + rng.NextFloat() * .75f;
				
				
				Matrix4 TransMatrix =  Matrix4.CreateScale( Scale );
				TransMatrix *= RotY;
				Model.ExtraData.AddRange(Model.GenerateWindValues());
				if(Type == PlantType.ROCK){
					for(int i = 0; i < Model.ExtraData.Count; i++)
						Model.ExtraData[i] = 0.001f;//Rocks shouldn't be affected by wind
				}
				TransMatrix *= Matrix4.CreateTranslation(new Vector3(position.X, Height, position.Z) );
				
				InstanceData Data = new InstanceData();
				
				Data.MeshCache = OriginalModel;
				Data.Colors = Model.Colors.Clone();
				Data.ExtraData = Model.ExtraData.Clone();
				Data.TransMatrix = TransMatrix;
				CacheManager.Check(Data);

				UnderChunk.StaticBuffer.AddInstance(Data);
					
				Model.Dispose();
				
				if(PlantType.ROCK == Type){
					List<CollisionShape> NewShapes = CacheManager.GetShape(CacheManager.GetModel(CacheItem.Rock));
					NewShapes.ForEach( shape => shape.Transform(TransMatrix));

					UnderChunk.AddCollisionShape(NewShapes.ToArray());
				}
			}
			#endregion
			
			#region CLOUDS
			if(Type == PlantType.CLOUD){
				VertexData CloudData = CacheManager.GetModel(CacheItem.Cloud);
				
				Vector3 CloudPosition = new Vector3(position.X, 800 + Utils.Rng.NextFloat() * 16f - 8f, position.Z);
				
				Matrix4 TransMatrix = Matrix4.CreateScale( rng.NextFloat() * 6.0f + 40f );
				TransMatrix *= Matrix4.CreateRotationY(360f * Utils.Rng.NextFloat() );
				TransMatrix *= Matrix4.CreateTranslation(CloudPosition);
				
				InstanceData Data = new InstanceData();
				Data.MeshCache = CloudData;
				Data.Colors = CloudData.Colors.Clone();
				Data.ExtraData = CloudData.ExtraData.Clone();
				Data.TransMatrix = TransMatrix;
				CacheManager.Check(Data);
					
				
				UnderChunk.StaticBuffer.AddInstance(Data);
			}
			#endregion
			
		}

		public void GenerateBerryBush(int x, int y, int z, Chunk UnderChunk){
			//If they are in the same position they can be explotable
			Random Rng = UnderChunk.Landscape.RandomGen;//new Random(World.Seed + 52342 + UnderChunk.OffsetX + UnderChunk.OffsetZ + x + y);
			
			Vector3 Addon = new Vector3(Rng.NextFloat() * 4f, 0, Rng.NextFloat() * 4f);
			Vector3 Position = new Vector3(UnderChunk.OffsetX + x * Chunk.BlockSize, y, UnderChunk.OffsetZ + z * Chunk.BlockSize);
			
			float Height = Physics.HeightAtPosition(Position + Addon);
			
			for(int _x = -3; _x < 3; _x++){
				for(int _z = -3; _z < 3; _z++){
					float BDens = Physics.HeightAtPosition(new Vector3( (x+_x) * Chunk.BlockSize + UnderChunk.OffsetX, 0, (z+_z) * Chunk.BlockSize + UnderChunk.OffsetZ));
					float Difference = Math.Abs(BDens - Height);
					if(Difference > 5)
						return;
				}
			}
			
			VertexData Model = CacheManager.GetModel(CacheItem.BerryBush).Clone();
			
			Matrix4 RotY = Matrix4.CreateRotationY(360 * Utils.Rng.NextFloat());
			Vector4 NewColor = Utils.VariateColor(UnderChunk.Biome.Colors.GrassColor, 15, Rng);
			Vector4 BerriesColor = Utils.VariateColor(World.EnviromentGenerator.BerryColor(Rng), 15, Rng);
			
			Model.Color(AssetManager.ColorCode0, NewColor);
			Model.GraduateColor(Vector3.UnitY);
			Model = Model + CacheManager.GetModel(CacheItem.Berries).Clone();
			Model.Color(AssetManager.ColorCode1, BerriesColor);
			float Scale = 1.5f + Rng.NextFloat() * .75f;
			Model.Transform( Matrix4.CreateScale( Scale ) );
			Model.Transform(RotY);
			//Model.Transform(new Vector3(Position.X, Height, Position.Z) );
						
			ThreadManager.ExecuteOnMainThread( delegate{
				var BerryBush = new Entity();
				BerryBush.Physics.HasCollision = false;
				BerryBush.Physics.UsePhysics = false;
				BerryBush.Physics.HitboxSize = 0;
				//BerryBush.Physics.CanCollide = false;
				BerryBush.Model = new StaticModel(BerryBush, Model);
				BerryBush.Model.Position = new Vector3(Position.X, Height, Position.Z);
				
				DamageComponent Damage = new DamageComponent(BerryBush);
				Damage.Immune = true;
				BerryBush.AddComponent( Damage );
				
				BerryBushComponent Berries = new BerryBushComponent(BerryBush);
				Berries.UnderChunk = UnderChunk;
				BerryBush.AddComponent( Berries );

			    World.AddEntity(BerryBush);
			});
			//Model.Transform(new Vector3(Position.X, Height, Position.Z) );
			//UnderChunk.AddStaticElement(Model);
		}
		
		public Vector4 BerryColor(Random Rng){
			int N = Rng.Next(0, 6);
			switch(N){
				case 0: return Colors.FromHtml("#BF4B42");
				case 1: return Colors.FromHtml("#FF6380");
				case 2: return Colors.FromHtml("#AA3D98");
				case 3: return Colors.FromHtml("#FF65F2");
				case 4: return Colors.FromHtml("#379B95");
				case 5: return Colors.FromHtml("#FFAD5A");
			}
			return Colors.FromHtml("#ff0000");
		}
		
		public Vector4 WheatColor(Random Rng){
			int N = Rng.Next(0, 5);
			switch(N){
				case 0: return Colors.FromHtml("#f4deb3");
			}
			return BerryColor(Rng);
		}
		
		public Vector4 RockColor(Random Rng){
			int N = Rng.Next(0, 2);
			switch(N){
				case 0: return Colors.FromHtml("#976548");
				case 1: return Colors.Gray;
			}
			return Colors.Red;
		}
		
		public void Clear(){}
		
	}
	
	public enum PlantType{
		GRASS,
		WHEAT,
		BUSH,
		FERN,
		ROCK,
		ALGAE,
		CLOUD,
		MAX_ITEMS
	}
}
