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
using Hedra.Engine.Player;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of VillageGenerator.
	/// </summary>
	public static class VillageGenerator
	{
		private static VertexData[] HouseModels = new VertexData[2];
        private static VertexData[] BlacksmithModels = new VertexData[2];
	    private static List<CollisionShape>[] BlacksmithShapes = new List<CollisionShape>[2];
        private static List<CollisionShape>[] HouseShapes_Clones = new List<CollisionShape>[2];
		private static Dictionary<int, List<CollisionShape>> ShelfShapes_Clones = new Dictionary<int, List<CollisionShape>>();
		private static Dictionary<int, VertexData> ShelfModels_Clones = new Dictionary<int, VertexData>();
		private static VertexData Market0_Clone = AssetManager.PlyLoader("Assets/Env/Village/MarketStand0.ply", Vector3.One);
		private static VertexData Market1_Clone = AssetManager.PlyLoader("Assets/Env/Village/MarketStand1.ply", Vector3.One);
		private static List<CollisionShape> MarketShapes_Clone = AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand0.ply", 6, Vector3.One);
		private static List<CollisionShape> ExtraShelf_Clone = AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand1.ply", 1, Vector3.One);
		private static VertexData Stable0_Clone = AssetManager.PlyLoader("Assets/Env/Village/Stable0.ply", Vector3.One);
	    private static List<CollisionShape> Stable0Shapes_Clone;
		private static VertexData Cementery0_Clone = AssetManager.PlyLoader("Assets/Env/Mausoleum.ply", Vector3.One* 2f);

        public static void Init(){
			HouseModels[0] = AssetManager.PlyLoader("Assets/Env/Village/House"+0+".ply", Vector3.One);
			HouseModels[1] = AssetManager.PlyLoader("Assets/Env/Village/House"+1+".ply", Vector3.One);
			
			HouseShapes_Clones[0] = AssetManager.LoadCollisionShapes("Assets/Env/Village/House"+0+".ply", 3, Vector3.One);
			HouseShapes_Clones[1] = AssetManager.LoadCollisionShapes("Assets/Env/Village/House"+1+".ply", 3, Vector3.One);

            BlacksmithModels[0] = AssetManager.PlyLoader("Assets/Env/Village/Blacksmith0.ply", Vector3.One);
            BlacksmithModels[1] = AssetManager.PlyLoader("Assets/Env/Village/Blacksmith1.ply", Vector3.One);

            BlacksmithShapes[0] = AssetManager.LoadCollisionShapes("Assets/Env/Village/Blacksmith0.ply", 21, Vector3.One);
            BlacksmithShapes[1] = AssetManager.LoadCollisionShapes("Assets/Env/Village/Blacksmith1.ply", 19, Vector3.One);


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

            Stable0Shapes_Clone = AssetManager.LoadCollisionShapes("Assets/Env/Village/Stable0.ply", 24, Vector3.One);
        }
		
		public static string GenerateVillageName(int Seed){
			var rng = new Random(Seed + 2341);
			return NameGenerator.Generate(rng.Next(0,99999999));
		}
		
		public static EntityMesh GenerateWindmillBlades(Vector3 Position, Matrix4 TransMatrix){
			var scale = 1f;
			var rng = new Random(World.Seed + 7654531 + (int) (Position.X+Position.Z) );
			Matrix4 rotationMatrix = Matrix4.CreateRotationY( rng.NextFloat() * Mathf.Radian * 360 );
			VertexData data = AssetManager.PlyLoader("Assets/Env/Village/Windmill0_Blades.ply", new Vector3(scale, scale, scale));
			data.Transform(rotationMatrix);
			data.Transform( Matrix4.CreateScale(TransMatrix.ExtractScale()) );

		    Vector4 woodColor = Utils.UniformVariateColor(Colors.FromHtml("#2D262A"), 25, rng);
		    data.Color(AssetManager.ColorCode3, woodColor);
		    

		    EntityMesh mesh = EntityMesh.FromVertexData(data);
			mesh.Position = Position + TransMatrix.ExtractTranslation();
			return mesh;
		}
		
		public static EntityMesh GenerateStableIcon(Vector3 Scale, Vector3 Position){
			var rng = new Random(World.Seed + 312412 + (int) (Position.X+Position.Z) );
		    VertexData model = Stable0_Clone.Clone();
            model.Scale( Scale );

			Vector4 woodColor = Utils.UniformVariateColor(Colors.FromHtml("#2D262A"), 25, rng);
			model.Color(AssetManager.ColorCode3, Utils.VariateColor(WallColor(rng), 5, rng) * new Vector4(.7f, .7f, .7f, 1f) );
			model.Color(AssetManager.ColorCode0, Utils.VariateColor(FloorColor(rng), 5, rng));
			
			ROOF_COLOR:
			Vector4 roofC = Utils.UniformVariateColor(RoofColor(rng), 25, rng) * new Vector4(.8f, .8f, .8f, 1f);
			if(roofC == Colors.FromHtml("#506CCC") )
				goto ROOF_COLOR;
			
			model.Color(new Vector4(.8f, .8f, .8f, 1f), roofC);
			model.Color(AssetManager.ColorCode2, woodColor);
			model.Color(AssetManager.ColorCode1, woodColor);
			model.GraduateColor(Vector3.UnitY);
			
			return EntityMesh.FromVertexData(model);
		}

	    public static EntityMesh GenerateCementeryIcon(Vector3 Scale, Vector3 Position)
	    {
	        VertexData model = Cementery0_Clone.Clone();
	        model.Scale(Scale);

	        model.GraduateColor(Vector3.UnitY);
	        return EntityMesh.FromVertexData(model);
	    }

        public static EntityMesh GenerateVillageHouseIcon(Vector3 Scale){
			var rng = new Random(World.Seed + 1231);
			VertexData houseModel = AssetManager.PlyLoader("Assets/Env/Village/House1.ply", Scale);

			return EntityMesh.FromVertexData(houseModel);
		}
		
		public static VertexData GenerateLogHouse(Vector3 Position, List<CollisionShape> Shapes, Chunk UnderChunk){
			var rng = new Random(World.Seed +412412);
		    var model = new VertexData();

		    var scale = 3.5f;
			Matrix4 transMatrix = Matrix4.CreateScale(scale);
			transMatrix *= Matrix4.CreateRotationY( 360 * Mathf.Radian * rng.NextFloat() );
			transMatrix *= Matrix4.CreateTranslation( Position + -Vector3.UnitY * .4f );
			
			model += AssetManager.PlyLoader("Assets/Env/LogHouse0.ply", Vector3.One);
			model.Transform(transMatrix);
			
			List<CollisionShape> newShapes = AssetManager.LoadCollisionShapes("Assets/Env/LogHouse0.ply", 9, Vector3.One);
			for(int i = 0; i < newShapes.Count; i++){
				newShapes[i].Transform(transMatrix);
			}
			Shapes.AddRange(newShapes.ToArray());
			return model;
		}

        #region Coroutines

        public static IEnumerator GenerateWindmill(object[] Params)
        {
            var position = (Vector3)Params[0];
            var rng = new Random(World.Seed + 7654531 + (int)(position.X + position.Z));//Params[1] as Random;
            var village = (bool)Params[2];
            Matrix4 transMatrix = Matrix4.Identity;
            if (village) transMatrix = (Matrix4)Params[3];

            Chunk underChunk = World.GetChunkAt(position + transMatrix.ExtractTranslation());
            while (underChunk == null || !underChunk.BuildedWithStructures)
            {
                underChunk = World.GetChunkAt(position + transMatrix.ExtractTranslation());
                yield return null;
            }

            var scale = 1f;

            Matrix4 rotationMatrix = Matrix4.CreateRotationY(rng.NextFloat() * Mathf.Radian * 360);
            VertexData model = AssetManager.PlyLoader("Assets/Env/Village/Windmill0.ply", new Vector3(scale, scale, scale));
            //Model.Transform(RotationMatrix);
            model.Transform(transMatrix);
            model.Transform(position);

            //Changecolors
            Vector4 woodColor = Utils.UniformVariateColor(underChunk.Biome.Colors.WoodColor, 25, rng);
            model.Color(AssetManager.ColorCode2, Utils.VariateColor(WallColor(rng), 5, rng) * new Vector4(.7f, .7f, .7f, 1f));

            ROOF_COLOR:
            Vector4 roofC = Utils.UniformVariateColor(RoofColor(rng), 25, rng) * new Vector4(.8f, .8f, .8f, 1f);
            if (roofC == Colors.FromHtml("#506CCC"))
                goto ROOF_COLOR;

            model.Color(AssetManager.ColorCode3, woodColor);
            model.Color(AssetManager.ColorCode1, roofC);
            model.GraduateColor(Vector3.UnitY);

            List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Assets/Env/Village/Windmill0.ply", 6, new Vector3(scale, scale, scale));
            for (int i = 0; i < shapes.Count; i++)
            {
                //Shapes[i].Transform(RotationMatrix);
                shapes[i].Transform(transMatrix);
                shapes[i].Transform(position);
            }

            underChunk.AddStaticElement(model);
            underChunk.AddCollisionShape(shapes.ToArray());
        }

        public static IEnumerator GenerateStable(object[] Params)
        {
            var town = (CollidableStructure) Params[0];
            var position = (Vector3)Params[1];
            var rng = new Random(World.Seed + 312412 + (int)(position.X + position.Z));
            var village = (bool)Params[3];
            Matrix4 transMatrix = Matrix4.Identity;
            if (village) transMatrix = (Matrix4)Params[4];

            Vector3 stablePosition = position + transMatrix.ExtractTranslation();
            Chunk underChunk = World.GetChunkAt(stablePosition);
            int currentSeed = World.Seed;
            while (underChunk == null || !underChunk.BuildedWithStructures)
            {
                if (World.Seed != currentSeed)
                    yield break;
                underChunk = World.GetChunkAt(stablePosition);
                yield return null;
            }

            float heightAtPosition = Physics.HeightAtPosition(stablePosition);

            if (Math.Abs(heightAtPosition - stablePosition.Y) > 2)
                yield break;

            position = new Vector3(position.X, heightAtPosition, position.Z);

            var scale = 3.0f;

            VertexData model = Stable0_Clone.Clone();
            model.Transform(transMatrix);
            model.Transform(position);

            //model.GraduateColor(Vector3.UnitY);

            List<CollisionShape> shapes = Stable0Shapes_Clone.DeepClone();
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                shapes[i].Transform(position);

                town.AddCollisionShape(shapes[i]);
            }

            underChunk.AddStaticElement(model);

            underChunk.Blocked = true;
            World.AddChunkToQueue(underChunk, true);
            
        }

        public static IEnumerator BuildBlacksmith(object[] Params)
        {
            var town = (CollidableStructure) Params[0];
			var transMatrix = (Matrix4) Params[1];
			var blacksmithPosition = (Vector3) Params[2];

            Vector3 position = transMatrix.ExtractTranslation() + blacksmithPosition;
			Chunk underChunk = World.GetChunkAt(position);
			int currentSeed = World.Seed;
			while( (underChunk == null || !underChunk.BuildedWithStructures) && World.Seed == currentSeed){
				underChunk = World.GetChunkAt(position);
				yield return null;
			}

            float heightAtPosition = Physics.HeightAtPosition(position);

            if (Math.Abs(heightAtPosition - position.Y) > 2)
                yield break;

            int type = 1;

            if (type == 1)
                transMatrix = transMatrix * Matrix4.CreateScale(.75f);

            Vector3 blacksmithGuyOffset = Vector3.Zero;
            if (type == 1)
                blacksmithGuyOffset = Vector3.UnitX * 10f + Vector3.UnitZ * 24;

            TaskManager.Parallel( delegate{

                var bigPost =
                    new LampPost(blacksmithPosition + transMatrix.ExtractTranslation() + Vector3.UnitZ * 10f - Vector3.UnitX * 16f)
                    {
                        Radius = 32,
                        LightColor = new Vector3(1f, .5f, 0f)
                    };
                World.AddStructure(bigPost);

                List<CollisionShape> blacksmithShapes = BlacksmithShapes[type].Clone();
				VertexData blacksmith = BlacksmithModels[type].Clone();
                blacksmith.Transform(transMatrix);
				blacksmith.Transform(Matrix4.CreateTranslation(blacksmithPosition + Vector3.UnitY * -.5f));
                blacksmith.GraduateColor(Vector3.UnitY);
				underChunk.AddStaticElement(blacksmith);
				
				for(int i = 0; i < blacksmithShapes.Count; i++){
					blacksmithShapes[i].Transform(transMatrix);
					blacksmithShapes[i].Transform(Matrix4.CreateTranslation(blacksmithPosition));
				}
				town.AddCollisionShape(blacksmithShapes.ToArray());
				underChunk.Blocked = true;
				World.AddChunkToQueue(underChunk, true);
			                          });

                World.QuestManager.SpawnHumanoid(HumanType.Blacksmith, blacksmithGuyOffset + blacksmithPosition + transMatrix.ExtractTranslation());
        }
		
		public static IEnumerator BuildSingleHouse(object[] Params){
			Vector3 position = (Vector3)Params[0];
			var transMatrix = (Matrix4) Params[1];
			var rng = Params[2] as Random;

		    Vector3 housePosition = transMatrix.ExtractTranslation() + position;
            Chunk underChunk = World.GetChunkAt(housePosition);

			int currentSeed = World.Seed;
			while( underChunk == null || !underChunk.BuildedWithStructures){
				if(World.Seed != currentSeed)
					yield break;
				underChunk = World.GetChunkAt(housePosition);
				yield return null;
			}


            float heightAtPosition = Physics.HeightAtPosition(housePosition);

		    if (Math.Abs(heightAtPosition - housePosition.Y) > 4)
		        yield break;

            TaskManager.Parallel( delegate{
                //var post = new LampPost(transMatrix.ExtractTranslation() + position + Vector3.UnitY * 24f);
                //post.Radius = 384;
                //World.AddStructure(post);

				int houseType = rng.Next(0,2);
			
				VertexData houseModel = HouseModels[houseType].Clone();
				Matrix4 houseMatrix = Matrix4.Identity;
				if(houseType == 0)
					houseMatrix =  Matrix4.CreateScale( new Vector3(1.45f,1.45f,1.45f) );
				else
					houseMatrix =  Matrix4.CreateScale( new Vector3(1.5f,1.5f,1.5f) );
					
                
				houseModel.Transform(houseMatrix);
				houseModel.Transform(transMatrix);
				houseModel.Transform(position);
				//houseModel.GraduateColor(Vector3.UnitY);
                List<CollisionShape> houseShapes = HouseShapes_Clones[houseType].DeepClone();

				
				for(int i = 0; i < houseShapes.Count; i++){
					houseShapes[i].Transform(houseMatrix);
					houseShapes[i].Transform(transMatrix);
					houseShapes[i].Transform(position);
				}
				underChunk.AddStaticElement(houseModel);
				underChunk.AddCollisionShape(houseShapes.ToArray());
				underChunk.Blocked = true;
				World.AddChunkToQueue(underChunk, true);
			                          });
		}
		
		public static IEnumerator BuildFarms(object[] Params)
		{
		    var town = (CollidableStructure) Params[0];
		    var rng = Params[1] as Random;
		    var transMatrix = (Matrix4) Params[2];
			var farmPosition = (Vector3) Params[3];
			
			Matrix4 mat4 = Matrix4.CreateScale(new Vector3(.85f,.85f,.85f));
			mat4 *= Matrix4.CreateTranslation( Vector3.UnitY * .1f);
			
			Matrix4 rotY = Matrix4.CreateRotationY( rng.NextFloat() * Mathf.Radian * 360);
			
			float modifier = rng.NextFloat();
			VertexData model = null;
			var farmCount = 4;
			int k = 0, j = 0;
			for(int i = 0; i < farmCount; i++){
				Vector3 offset =  -Vector3.UnitX * (1f+modifier*.5f) * Chunk.BlockSize * 1f 
				                    + Vector3.UnitZ * 4f * Chunk.BlockSize * 0.375f 
				                    + Vector3.UnitX * (2f+modifier) * j * Chunk.BlockSize * 1f
				                    - Vector3.UnitZ * k * 2f * Chunk.BlockSize * 1.5f;

			    Vector3 position = transMatrix.ExtractTranslation() + farmPosition.Xz.ToVector3() + offset;
                Chunk underChunk = World.GetChunkAt(position);
				int currentSeed = World.Seed;
				while( underChunk == null || !underChunk.BuildedWithStructures){
				if(World.Seed != currentSeed)
					yield break;
					underChunk = World.GetChunkAt(position);
					yield return null;
				}

			    
			    float heightAtPosition = Physics.HeightAtPosition(position);

                if( Math.Abs(heightAtPosition - farmPosition.Y) > 3 )
                    continue;

			    //Vector3 terrainNormal = Physics.NormalAtPosition(position.X, position.Z);
			    //var lookAt = new Matrix4(new Matrix3(Mathf.RotationAlign(Vector3.UnitY, terrainNormal)));
				VertexData farmModel = CacheManager.GetModel(CacheItem.Farm).Clone();
				farmModel.Color(AssetManager.ColorCode1, Utils.VariateColor( FenceColor(new Random(World.Seed + 2412)), 15, rng) );
				farmModel.Color(AssetManager.ColorCode2, Utils.VariateColor(underChunk.Biome.Colors.GrassColor , 15, rng) );
                //farmModel.Transform(lookAt);
				farmModel.Transform(mat4);
				farmModel.Transform(offset);
				farmModel.Transform(transMatrix);
				farmModel.Transform(farmPosition);
				model += farmModel;
				j++;
				if(j == 2){
					j = 0;
					k++;
				}
				
				underChunk.AddStaticElement(farmModel);
				underChunk.Blocked = true;
				World.AddChunkToQueue(underChunk, true);
			}
			k = 0; 
			j = 0;
			
			for(int i = 0; i < farmCount; i++){
				List<CollisionShape> farmShapes = AssetManager.LoadCollisionShapes("Assets/Env/Village/Farm0.ply",5, Vector3.One);
				//FarmShapes.AddRange( AssetManager.LoadCollisionShapes("Assets/Env/Village/Fence0.ply",11, Vector3.One) );
				Vector3 offset =  -Vector3.UnitX * (1f+modifier*.5f) * Chunk.BlockSize * 1f 
				                    + Vector3.UnitZ * 4f * Chunk.BlockSize * 0.375f 
				                    + Vector3.UnitX * (2f+modifier) * j * Chunk.BlockSize * 1f
				                    - Vector3.UnitZ * k * 2f * Chunk.BlockSize * 1.5f;
				
				Chunk underChunk = World.GetChunkAt(transMatrix.ExtractTranslation() +  farmPosition + offset);
				int currentSeed = World.Seed;
		    	while( underChunk == null || !underChunk.BuildedWithStructures){
				if(World.Seed != currentSeed)
					yield break;
					underChunk = World.GetChunkAt(transMatrix.ExtractTranslation() +  farmPosition + offset);
					yield return null;
				}

			    float heightAtPosition = Physics.HeightAtPosition(transMatrix.ExtractTranslation() + farmPosition + offset);

			    if (Math.Abs(heightAtPosition - farmPosition.Y) > 3)
			        continue;

                for (int l = 0; l < farmShapes.Count; l++){

					farmShapes[l].Transform(mat4);
					farmShapes[l].Transform(offset);
					farmShapes[l].Transform(transMatrix);
					farmShapes[l].Transform(farmPosition);
                    farmShapes[l].UseBroadphase = true;
                    town.AddCollisionShape(farmShapes[l]);
                }
				j++;
				if(j == 2){
					j = 0;
					k++;
				}
			}
		}

	    public static IEnumerator BuildCenter(object[] Params)
	    {

	        var rng = Params[0] as Random;
	        var shapes = Params[1] as List<CollisionShape>;
	        var transMatrix = (Matrix4) Params[2];

	        Chunk underChunk = World.GetChunkAt(transMatrix.ExtractTranslation());
	        int currentSeed = World.Seed;
	        while (underChunk == null || !underChunk.BuildedWithStructures)
	        {
	            if (World.Seed != currentSeed)
	                yield break;
	            underChunk = World.GetChunkAt(transMatrix.ExtractTranslation());
	            yield return null;
	        }

            Vector3 position = transMatrix.ExtractTranslation();


	        var bigPost =
	            new LampPost(Vector3.UnitY * 8f + position)
	            {
	                Radius = 386,
	                LightColor = new Vector3(.25f, .25f, .25f)
	            };
	        World.AddStructure(bigPost);

            float heightAtPosition = Physics.HeightAtPosition(position);

	        if (Math.Abs(heightAtPosition - position.Y) > 2)
	            yield break;

	        float wellScale = rng.NextFloat() * .25f + .85f;
	        VertexData model = AssetManager.PlyLoader("Assets/Env/Village/Well0.ply", Vector3.One);
	        model.Transform(Matrix4.CreateScale(wellScale));
	        model.Transform(Matrix4.CreateTranslation(Vector3.UnitY * .3f));
	        model.Transform(transMatrix);

	        List<CollisionShape> wellShapes =
	            AssetManager.LoadCollisionShapes("Assets/Env/Village/Well0.ply", 6, Vector3.One);
	        for (int i = 0; i < wellShapes.Count; i++)
	        {
	            wellShapes[i].Transform(Matrix4.CreateScale(wellScale));
	            wellShapes[i].Transform(transMatrix);
	        }

	        underChunk.AddCollisionShape(wellShapes.ToArray());
            if (shapes != null)
	            underChunk.AddCollisionShape(shapes.ToArray());
	        

	        underChunk.AddStaticElement(model);
			underChunk.Blocked = true;
			World.AddChunkToQueue(underChunk, true);
		}
		
		public static IEnumerator BuildMarket(object[] Params)
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

			    Vector3 position = transMatrix.ExtractTranslation() + marketPos;

                float heightAtPosition = Physics.HeightAtPosition(position);

                if (Math.Abs(heightAtPosition - position.Y) > 2)
			        continue;

                if(i == 0)
			        ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnHumanoid(HumanType.Merchant, centerPosition - Vector3.UnitZ * 40f));
                else if (i == 1)
                    ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnHumanoid(HumanType.Merchant, centerPosition + Vector3.UnitZ * 40f));
                else if (i == 2)
                    ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnVillager(centerPosition - Vector3.UnitX * 40f, false));
                else if (i == 3)
                    ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnVillager(centerPosition + Vector3.UnitX * 40f, false));

                int k = i;
		    	TaskManager.Parallel( delegate{
			    	VertexData market0 = Market0_Clone.Clone();
					bool extraShelf = rng.Next(0, 4) != 0;
					if(extraShelf)
						market0 += Market1_Clone.Clone();
					market0.Transform( Matrix4.CreateRotationY( 90 * Mathf.Radian ) );
					market0.Transform(Vector3.UnitZ * marketDist * Chunk.BlockSize);
					market0.Transform( Matrix4.CreateRotationY( 360 / marketCount * k * Mathf.Radian ) );
					market0.Color(AssetManager.ColorCode1, MarketColor(rng));
					
					List<CollisionShape> marketShapes = MarketShapes_Clone.DeepClone();
					if(extraShelf) marketShapes.Add((CollisionShape) ExtraShelf_Clone[0].Clone());
					
					for(int j = 0; j < marketShapes.Count; j++){
						marketShapes[j].Transform( Matrix4.CreateRotationY( 90 * Mathf.Radian ) );
						marketShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
						marketShapes[j].Transform( Matrix4.CreateRotationY( 360 / marketCount * k * Mathf.Radian ) );
						marketShapes[j].Transform(transMatrix);
					}
					
					int basketCount = rng.Next(0, 6);
					if(basketCount == 0) basketCount = 2;
					else if(basketCount == 1 || basketCount == 2) basketCount = 3;
					else if(basketCount == 3 || basketCount == 4 || basketCount == 5) basketCount = 4;
					
					List<CollisionShape> shelfShapes = ShelfShapes_Clones[basketCount].DeepClone();
					VertexData shelfModel = ShelfModels_Clones[basketCount].Clone();
					
					shelfModel.Transform( Matrix4.CreateRotationY( 90 * Mathf.Radian ) );
					shelfModel.Transform(Vector3.UnitZ * marketDist * Chunk.BlockSize);
					shelfModel.Transform( Matrix4.CreateRotationY( 360 / marketCount * k * Mathf.Radian ) );
					shelfModel.Color(AssetManager.ColorCode1, World.EnviromentGenerator.BerryColor(rng) );
					
					for(int j = 0; j < shelfShapes.Count; j++){
						shelfShapes[j].Transform( Matrix4.CreateRotationY( 90 * Mathf.Radian ) );
						shelfShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
						shelfShapes[j].Transform( Matrix4.CreateRotationY( 360 / marketCount * k * Mathf.Radian ) );
						shelfShapes[j].Transform(transMatrix);
					}
	
					market0 += shelfModel;
					marketShapes.AddRange(shelfShapes);
					
					if(extraShelf){
						basketCount = rng.Next(0, 6);
						if(basketCount == 0) basketCount = -1;
						else if(basketCount == 1 || basketCount == 2) basketCount = 5;
						else if(basketCount == 3 || basketCount == 4 || basketCount == 5) basketCount = 6;
						
						if(basketCount != -1){
							shelfShapes = ShelfShapes_Clones[basketCount].DeepClone();
							shelfModel = ShelfModels_Clones[basketCount].Clone();
							
							shelfModel.Transform( Matrix4.CreateRotationY( 90 * Mathf.Radian ) );
							shelfModel.Transform(Vector3.UnitZ * marketDist * Chunk.BlockSize);
							shelfModel.Transform( Matrix4.CreateRotationY( 360 / marketCount * k * Mathf.Radian ) );
							shelfModel.Color(AssetManager.ColorCode1, World.EnviromentGenerator.BerryColor(rng) );
							
							for(int j = 0; j < shelfShapes.Count; j++){
								shelfShapes[j].Transform( Matrix4.CreateRotationY( 90 * Mathf.Radian ) );
								shelfShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
								shelfShapes[j].Transform( Matrix4.CreateRotationY( 360 / marketCount * k * Mathf.Radian ) );
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
					World.AddChunkToQueue(underChunk, true);
		    	                          });
			}
		}

#endregion

        #region Colors

        private static Vector4 FenceColor(Random Rng){
			int n = Rng.Next(0, 2);
			switch(n){
				case 0: return Colors.FromHtml("#8F6951");
				case 1: return Colors.FromHtml("#8F6951") * new Vector4(.6f,.6f,.6f,1f);
				//case 1: return Colors.FromHtml("#FFFFFF");
			}
			return Colors.Red; 
		}
		
		private static Vector4 MarketColor(Random Rng){
			int n = Rng.Next(0, 6);
			switch(n){
				case 0: return Colors.FromHtml("#BF4B42");
				case 1: return Colors.FromHtml("#FF6380");
				case 2: return Colors.FromHtml("#AA3D98");
				//case 3: return Colors.FromHtml("#FF65F2");
				case 4: return Colors.FromHtml("#379B95");
				case 5: return Colors.FromHtml("#FFAD5A");
			}
			return Colors.Red;
		}

	    private static Vector4 DoorColor(Random Rng)
	    {
	        int n = Rng.Next(0, 5);
	        switch (n)
	        {
	            case 0: return Colors.FromHtml("#c6965d");
	            case 1: return Colors.FromHtml("#7f613d");
	            case 2: return Colors.FromHtml("#73491c");
	            case 3: return Colors.FromHtml("#523f21");
	            case 4: return Colors.FromHtml("#6E5945");
	            default: return new Vector4(0, 0, 0, 1);
	        }
	    }

	    private static Vector4 WallColor(Random Rng)
	    {
	        int n = Rng.Next(0, 2);
	        switch (n)
	        {
	            case 0: return Colors.FromHtml("#d7d3cc");
	            case 1: return Colors.FromHtml("#CCCCCC");
	            default: return new Vector4(0, 0, 0, 1);
	        }
	    }

	    private static Vector4 FloorColor(Random Rng)
	    {
	        int n = Rng.Next(0, 1);
	        switch (n)
	        {
	            case 0: return Colors.FromHtml("#A88E76");
	            case 1: return Colors.FromHtml("#352e28");
	            case 2: return Colors.FromHtml("#d7d3cc");
	            case 3: return Colors.FromHtml("#6d6d57");
	            default: return new Vector4(0, 0, 0, 1);
	        }
	    }

	    private static Vector4 RoofColor(Random Rng)
	    {
	        int n = Rng.Next(0, 5);
	        switch (n)
	        {
	            case 0: return Colors.FromHtml("#506CCC");
	            case 1: return Colors.FromHtml("#e9734e");
	            case 2: return Colors.FromHtml("#A8524E");
	            case 3: return Colors.FromHtml("#71B3BE");
	            case 4: return Colors.FromHtml("#C93E6C");
	            default: return new Vector4(0, 0, 0, 1);
	        }
	    }

#endregion

        private static VertexData BuildPath(Matrix4 TransMatrix, Vector3 Origin, Vector3 End){
			int pathCount = (int) ( (Origin - End).Length / (5.35f * Chunk.BlockSize) ) + 1;
			Vector3 dir = (Origin - End).Normalized();
			Vector3 euler = Physics.DirectionToEuler( dir.Xz.ToVector3() );
			Matrix4 directionMat = Matrix4.CreateRotationY(euler.Y * Mathf.Radian);
			
			VertexData model = new VertexData();
			VertexData pathToBlacksmith = AssetManager.PlyLoader("Assets/Env/Village/Path0.ply", Vector3.One);
			List<VertexData> paths = new List<VertexData>();
			for(int i = 0; i < pathCount; i++){
				paths.Add( pathToBlacksmith.Clone() );
				//Paths[Paths.Count-1].Transform( DirectionMat );
				paths[paths.Count-1].Transform(TransMatrix);
				paths[paths.Count-1].Transform( dir * 5.35f * Chunk.BlockSize * i);
				model += paths[paths.Count-1];
			}
			return model;
		}
	}
}
