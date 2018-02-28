/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/05/2016
 * Time: 11:05 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using OpenTK;
using System.Threading;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    /// Description of EntitySpawner.
    /// </summary>
    public class EntitySpawner
	{
		public static int MobCap = int.MaxValue;
		public float SpawnChance = .8f;
		public int MinSpawn = 1;
		public int MaxSpawn = 3;
		public LocalPlayer Player;
		public bool Enabled;
        public Thread SpawnThread;
		
		public EntitySpawner(LocalPlayer Player)
		{
			this.Player = Player;
			SpawnThread = new Thread(Update);
            SpawnThread.Start();
		}
		
		public void Update(){
			
			while(true){
                START:
                if(!Program.GameWindow.Exists) break;
                Thread.Sleep(25);
				#if !DEBUG
				if(MobCap == 0){
					throw new ArgumentException("Mob cap cannot be 0");
				}
				#endif
				
				if(!Enabled) 
					goto START;
				Entity[] mobs = World.Entities.ToArray();


			    if (mobs.Length >= MobCap || !(Utils.Rng.NextFloat() <= SpawnChance) ) continue;
			    Vector3 desiredPosition = new Vector3(Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.ChunkWidth - GameSettings.ChunkLoaderRadius * Chunk.ChunkWidth * .5f,
                    0,
                    Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.ChunkWidth - GameSettings.ChunkLoaderRadius * Chunk.ChunkWidth * .5f)
                    + Player.Position.Xz.ToVector3();
			    Block underBlock = World.GetHighestBlockAt( (int) desiredPosition.X, (int) desiredPosition.Z);
			    if(underBlock.Type != BlockType.Air && underBlock.Type != BlockType.Water && underBlock.Type != BlockType.Seafloor){
			        int y = World.GetHighestY( (int) desiredPosition.X, (int) desiredPosition.Z);
			        if(y <= 0) 
			            goto START;
						
			        if((Player.Position.Xz - desiredPosition.Xz).LengthSquared < 24f*Chunk.BlockSize*24f*Chunk.BlockSize)
			            goto START;

			        for(int i = mobs.Length- 1; i > -1; i--)
			        {
			            if (mobs[i] == Player || mobs[i].IsStatic) continue;

			            if((mobs[i].BlockPosition.Xz - desiredPosition.Xz).LengthSquared < 48f * Chunk.BlockSize * 48f * Chunk.BlockSize)
			                goto START;
			        }
						
			        Vector3 newPosition = new Vector3(desiredPosition.X, Physics.HeightAtPosition(desiredPosition), desiredPosition.Z);
						
			        string type = this.SelectMobType(newPosition);
                    if(type == null) goto START;

                    World.SpawnMob(type, newPosition, Utils.Rng);
			    }
			}
		}
		
		public string SelectMobType(Vector3 NewPosition){
			bool mountain = NewPosition.Y > 60 * Chunk.BlockSize;
		    bool shore = NewPosition.Y / Chunk.BlockSize > Chunk.BaseHeight && NewPosition.Y / Chunk.BlockSize < 8 + Chunk.BaseHeight;
		    bool forest = !shore && World.TreeGenerator.SpaceNoise(NewPosition.X, NewPosition.Z) > 0;
            bool plains = !forest && !shore && !mountain;

		    var templates = new[]
		    {
		        World.SpawnerSettings.Forest,
                World.SpawnerSettings.Mountain,
                World.SpawnerSettings.Plains,
		        World.SpawnerSettings.Shore
		    };

		    var conditions = new[]
		    {
		        forest,
		        mountain,
		        plains,
		        shore
		    };

            var rng = Utils.Rng;
		    for (var i = 0; i < templates.Length; i++)
		    {
		        var incremental = 0f;
		        foreach (SpawnTemplate template in templates[i])
		        {
		            float val = rng.NextFloat();

		            if (conditions[i] && val > incremental && val < incremental + template.Chance)
                        return template.Type;
		            
		            incremental += template.Chance;
		        }
		    }
		    return null;
		}
	}

	public enum MobType{
		Sheep,
		Pig,
		Boar,
		Fox,
		Turtle,
		Spider,
		Mosquitos,
		Bee,
		Wasp,
		Rat,
		Mask,
		Human,
		Horse,
		Wolf,
		Goat,
		TotalCount,
		None,
        Unknown
	}

}
