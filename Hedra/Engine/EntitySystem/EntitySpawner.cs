/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/05/2016
 * Time: 11:05 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using OpenTK;
using System.Threading;
using Hedra.Engine.Generation.ChunkSystem;
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
	    private Random Rng;
		
		public EntitySpawner(LocalPlayer Player)
		{
			this.Player = Player;
		    Rng = new Random();
            SpawnThread = new Thread(Update);
            SpawnThread.Start();
		}
		
		public void Update()
        {		
			while(Program.GameWindow.Exists){
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

			    if (World.Entities.Count >= MobCap || !(Utils.Rng.NextFloat() <= SpawnChance) ) continue;
			    Vector3 desiredPosition = new Vector3(Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width - GameSettings.ChunkLoaderRadius * Chunk.Width * .5f,
                    0,
                    Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width - GameSettings.ChunkLoaderRadius * Chunk.Width * .5f)
                    + Player.Position.Xz.ToVector3();
			    var underBlock = World.GetHighestBlockAt( (int) desiredPosition.X, (int) desiredPosition.Z);
			    if(underBlock.Type != BlockType.Air && underBlock.Type != BlockType.Water && underBlock.Type != BlockType.Seafloor){
			        int y = World.GetHighestY( (int) desiredPosition.X, (int) desiredPosition.Z);
			        if(y <= 0) 
			            goto START;
						
			        if((Player.Position.Xz - desiredPosition.Xz).LengthSquared < 32f*Chunk.BlockSize*32f*Chunk.BlockSize)
			            goto START;

			        try
			        {
			            for (int i = World.Entities.Count - 1; i > -1; i--)
			            {
			                if (World.Entities[i] == Player || World.Entities[i].IsStatic) continue;

			                if ((World.Entities[i].BlockPosition.Xz - desiredPosition.Xz).LengthSquared <
			                    80f * Chunk.BlockSize * 80f * Chunk.BlockSize)
			                    goto START;
			            }
			        }
			        catch (IndexOutOfRangeException e)
			        {
			            Log.WriteLine(e.Message);
                        goto START;
			        }
			        var newPosition = new Vector3(desiredPosition.X, Physics.HeightAtPosition(desiredPosition), desiredPosition.Z);
                    	
			        SpawnTemplate template = this.SelectMobTemplate(newPosition);
                    if(template == null) goto START;

			        int count = Utils.Rng.Next(template.MaxGroup, template.MaxGroup + 1);
			        if (!Program.GameWindow.Exists) break;
                    for (var i = 0; i < count; i++)
			        {
                        var offset = new Vector3(Utils.Rng.NextFloat() * 8f - 4f, 0, Utils.Rng.NextFloat() * 8f - 4f) * Chunk.BlockSize;
			            var newNearPosition = new Vector3(newPosition.X + offset.X,
                            Physics.HeightAtPosition(newPosition + offset),
                            newPosition.Z + offset.Z);
			            World.SpawnMob(template.Type, newNearPosition, Utils.Rng);
			        }
			    }
			}
		}
		
		public SpawnTemplate SelectMobTemplate(Vector3 NewPosition)
        {
		    var region = World.BiomePool.GetRegion(NewPosition);
            bool mountain = NewPosition.Y > 60 * Chunk.BlockSize;
		    bool shore = NewPosition.Y / Chunk.BlockSize > Chunk.BaseHeight && NewPosition.Y / Chunk.BlockSize < 2 + Chunk.BaseHeight;
		    bool forest = !shore && World.TreeGenerator.SpaceNoise(NewPosition.X, NewPosition.Z) > 0;
            bool plains = !forest && !shore && !mountain;


		    var templates = new[]
		    {
		        region.Mob.SpawnerSettings.Forest,
		        region.Mob.SpawnerSettings.Mountain,
		        region.Mob.SpawnerSettings.Plains,
		        region.Mob.SpawnerSettings.Shore
		    };

		    var conditions = new[]
		    {
		        forest,
		        mountain,
		        plains,
		        shore
		    };

		    for (var i = 0; i < templates.Length; i++)
		    {
		        if (conditions[i])
		        {
		            SpawnTemplate type = null;
		            while (type == null)
		            {
		                var template = templates[i][Rng.Next(0, templates[i].Length)];

		                if (Rng.NextFloat() < template.Chance)
		                    type = template;
		            }
		            return type;
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
        Gorilla,
        Beetle,
        Troll,
		TotalCount,
		None,
        Unknown
	}

}
