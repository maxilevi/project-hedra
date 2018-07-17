/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/12/2016
 * Time: 09:36 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.Drawing;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.PhysicsSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
	/// <summary>
	/// Description of VillageGenerator.
	/// </summary>
	internal class VillageGenerator : ISchemeGenerator
	{
		private Dictionary<int, List<CollisionShape>> ShelfShapes_Clones = new Dictionary<int, List<CollisionShape>>();
		private Dictionary<int, VertexData> ShelfModels_Clones = new Dictionary<int, VertexData>();
		private VertexData Market0_Clone = AssetManager.PlyLoader("Assets/Env/Village/MarketStand0.ply", Vector3.One);
		private VertexData Market1_Clone = AssetManager.PlyLoader("Assets/Env/Village/MarketStand1.ply", Vector3.One);
		private List<CollisionShape> MarketShapes_Clone = AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand0.ply", 6, Vector3.One);
		private List<CollisionShape> ExtraShelf_Clone = AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand1.ply", 1, Vector3.One);

        public VillageGenerator(){
            ShelfShapes_Clones.Add(2, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand2.ply", 1, Vector3.One));
			ShelfShapes_Clones.Add(3, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand3.ply", 1, Vector3.One));
			ShelfShapes_Clones.Add(4, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand4.ply", 1, Vector3.One));
			ShelfShapes_Clones.Add(5, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand5.ply", 1, Vector3.One));
			ShelfShapes_Clones.Add(6, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand6.ply", 1, Vector3.One));
			
			
			ShelfModels_Clones.Add(2, AssetManager.PlyLoader("Assets/Env/Village/MarketStand2.ply", Vector3.One));
			ShelfModels_Clones.Add(3, AssetManager.PlyLoader("Assets/Env/Village/MarketStand3.ply", Vector3.One));
			ShelfModels_Clones.Add(4, AssetManager.PlyLoader("Assets/Env/Village/MarketStand4.ply", Vector3.One));
			ShelfModels_Clones.Add(5, AssetManager.PlyLoader("Assets/Env/Village/MarketStand5.ply", Vector3.One));
			ShelfModels_Clones.Add(6, AssetManager.PlyLoader("Assets/Env/Village/MarketStand6.ply", Vector3.One));
        }
		
		public IEnumerator BuildMarket(object[] Params)
		{
		    var town = Params[0] as CollidableStructure;
			var rng = Params[1] as Random;
			var transMatrix = (Matrix4) Params[2];
			
			float marketDist = 1.75f + rng.NextFloat() * .75f + 1.2f;
			int marketCount = 6 + rng.Next(0, 4);
			
		    Vector3 centerPosition = transMatrix.ExtractTranslation();		    

            for (int i = 0; i < marketCount; i++)
			{
		    	//The chunk should be farther
		    	Vector3 marketPos = Vector3.TransformPosition(Vector3.UnitZ * marketDist * 3.0f * Chunk.BlockSize, Matrix4.CreateRotationY( 360 / marketCount * i * Mathf.Radian ));
		    	Chunk underChunk = World.GetChunkAt(transMatrix.ExtractTranslation() +  marketPos);
				int currentSeed = World.Seed;
		    	while( underChunk == null || !underChunk.BuildedWithStructures){
				if(World.Seed != currentSeed)
					yield break;
					underChunk = World.GetChunkAt(transMatrix.ExtractTranslation() +  marketPos);
					yield return null;
				}
				int k = i;
				TaskManager.Parallel(delegate
				{
					var originalPosition = transMatrix.ExtractTranslation();
					float heightAtPosition = Physics.HeightAtPosition(transMatrix.ExtractTranslation());
					transMatrix.Row3 = new Vector4(originalPosition.X, heightAtPosition, originalPosition.Z, transMatrix.Row3.W);

					if (k == 0)
						World.QuestManager.SpawnHumanoid(HumanType.Merchant, centerPosition - Vector3.UnitZ * 40f);
					else if (k == 1)
						World.QuestManager.SpawnHumanoid(HumanType.Merchant, centerPosition + Vector3.UnitZ * 40f);
					else if (k == 2)
						World.QuestManager.SpawnVillager(centerPosition - Vector3.UnitX * 40f, false);
					else if (k == 3)
						World.QuestManager.SpawnVillager(centerPosition + Vector3.UnitX * 40f, false);


					VertexData market0 = Market0_Clone.Clone();
					bool extraShelf = rng.Next(0, 4) != 0;
					if (extraShelf)
						market0 += Market1_Clone.Clone();
					market0.Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
					market0.Translate(Vector3.UnitZ * marketDist * Chunk.BlockSize);
					market0.Transform(Matrix4.CreateRotationY(360 / marketCount * k * Mathf.Radian));
					market0.Color(AssetManager.ColorCode1, MarketColor(rng));

					List<CollisionShape> marketShapes = MarketShapes_Clone.DeepClone();
					if (extraShelf) marketShapes.Add((CollisionShape) ExtraShelf_Clone[0].Clone());

					for (int j = 0; j < marketShapes.Count; j++)
					{
						marketShapes[j].Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
						marketShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
						marketShapes[j].Transform(Matrix4.CreateRotationY(360 / marketCount * k * Mathf.Radian));
						marketShapes[j].Transform(transMatrix);
					}

					int basketCount = rng.Next(0, 6);
					if (basketCount == 0) basketCount = 2;
					else if (basketCount == 1 || basketCount == 2) basketCount = 3;
					else if (basketCount == 3 || basketCount == 4 || basketCount == 5) basketCount = 4;

					List<CollisionShape> shelfShapes = ShelfShapes_Clones[basketCount].DeepClone();
					VertexData shelfModel = ShelfModels_Clones[basketCount].Clone();

					shelfModel.Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
					shelfModel.Translate(Vector3.UnitZ * marketDist * Chunk.BlockSize);
					shelfModel.Transform(Matrix4.CreateRotationY(360 / marketCount * k * Mathf.Radian));
					shelfModel.Color(AssetManager.ColorCode1, Colors.BerryColor(rng));

					for (int j = 0; j < shelfShapes.Count; j++)
					{
						shelfShapes[j].Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
						shelfShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
						shelfShapes[j].Transform(Matrix4.CreateRotationY(360 / marketCount * k * Mathf.Radian));
						shelfShapes[j].Transform(transMatrix);
					}

					market0 += shelfModel;
					marketShapes.AddRange(shelfShapes);

					if (extraShelf)
					{
						basketCount = rng.Next(0, 6);
						if (basketCount == 0) basketCount = -1;
						else if (basketCount == 1 || basketCount == 2) basketCount = 5;
						else if (basketCount == 3 || basketCount == 4 || basketCount == 5) basketCount = 6;

						if (basketCount != -1)
						{
							shelfShapes = ShelfShapes_Clones[basketCount].DeepClone();
							shelfModel = ShelfModels_Clones[basketCount].Clone();

							shelfModel.Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
							shelfModel.Translate(Vector3.UnitZ * marketDist * Chunk.BlockSize);
							shelfModel.Transform(Matrix4.CreateRotationY(360 / marketCount * k * Mathf.Radian));
							shelfModel.Color(AssetManager.ColorCode1, Colors.BerryColor(rng));

							for (int j = 0; j < shelfShapes.Count; j++)
							{
								shelfShapes[j].Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
								shelfShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
								shelfShapes[j].Transform(Matrix4.CreateRotationY(360 / marketCount * k * Mathf.Radian));
								shelfShapes[j].Transform(transMatrix);
							}
						}
						market0 += shelfModel;
						marketShapes.AddRange(shelfShapes);
					}
					market0.Transform(transMatrix);
					underChunk.Blocked = true;

					//underChunk.AddCollisionShape(marketShapes.ToArray());
					town.AddCollisionShape(marketShapes.ToArray());
					underChunk.AddStaticElement(market0);

				});
			}
		}
	}
}
