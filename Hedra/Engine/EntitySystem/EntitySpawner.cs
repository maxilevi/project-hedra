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
		public float SpawnChance { get; set; } = .8f;
		public int MinSpawn { get; set; }= 1;
		public int MaxSpawn { get; set; }= 3;
	    private readonly IPlayer _player;
	    private readonly Random _rng;
	    public bool Enabled { get; set; }
		
		public EntitySpawner(IPlayer Player)
		{
			this._player = Player;
		    _rng = new Random();
            var spawnThread = new Thread(delegate()
            {
	            while (GameManager.Exists)
	            {
		            this.Update();
		            Thread.Sleep(15);
	            }
            });
            spawnThread.Start();
		}
		
		public virtual void Update()
        {
			#if !DEBUG
			if(MobCap == 0)
			{
				throw new ArgumentException("Mob cap cannot be 0");
			}
			#endif
			
			if(!Enabled) return;

			if (World.Entities.Count >= MobCap || !(Utils.Rng.NextFloat() <= SpawnChance) ) return;
	        var desiredPosition = this.PlacementPosition();
			var underBlock = World.GetHighestBlockAt( (int) desiredPosition.X, (int) desiredPosition.Z);
	        if (underBlock.Type == BlockType.Air || underBlock.Type == BlockType.Water ||
	            underBlock.Type == BlockType.Seafloor) return;
	        
	        var y = World.GetHighestY( (int) desiredPosition.X, (int) desiredPosition.Z);
	        if (y <= 0) return;

	        if ((_player.Position.Xz - desiredPosition.Xz).LengthSquared <
	            32f * Chunk.BlockSize * 32f * Chunk.BlockSize)
		        return;

	        if (this.Conflicts(desiredPosition)) return;
	        
	        var newPosition = new Vector3(desiredPosition.X, Physics.HeightAtPosition(desiredPosition), desiredPosition.Z);					
	        var template = this.SelectMobTemplate(newPosition);
	        if (template == null) return;

	        var count = Utils.Rng.Next(template.MinGroup, template.MaxGroup + 1);

	        for (var i = 0; i < count; i++)
	        {
		        var offset = new Vector3(Utils.Rng.NextFloat() * 8f - 4f, 0, Utils.Rng.NextFloat() * 8f - 4f) * Chunk.BlockSize;
		        var newNearPosition = new Vector3(newPosition.X + offset.X,
			        Physics.HeightAtPosition(newPosition + offset),
			        newPosition.Z + offset.Z);
		        World.SpawnMob(template.Type, newNearPosition, Utils.Rng);
	        }
        }
		
		public virtual SpawnTemplate SelectMobTemplate(Vector3 NewPosition)
        {
		    var region = World.BiomePool.GetRegion(NewPosition);
            var mountain = NewPosition.Y > 60 * Chunk.BlockSize;
		    var shore = NewPosition.Y / Chunk.BlockSize > Chunk.BaseHeight && NewPosition.Y / Chunk.BlockSize < 2 + Chunk.BaseHeight;
		    var forest = !shore && World.TreeGenerator.SpaceNoise(NewPosition.X, NewPosition.Z) > 0;
            var plains = !forest && !shore && !mountain;

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
		                var template = templates[i][_rng.Next(0, templates[i].Length)];

		                if (_rng.NextFloat() < template.Chance)
		                    type = template;
		            }
		            return type;
		        }
		    }
		    return null;
		}

	    private bool Conflicts(Vector3 Position)
	    {
		    try
		    {
			    for (var i = World.Entities.Count - 1; i > -1; i--)
			    {
				    if (World.Entities[i] == _player || World.Entities[i].IsStatic) continue;

				    if ((World.Entities[i].BlockPosition.Xz - Position.Xz).LengthSquared <
				        80f * Chunk.BlockSize * 80f * Chunk.BlockSize) return true;
			    }
		    }
		    catch (IndexOutOfRangeException e)
		    {
			    Log.WriteLine(e.Message);
			    return true;
		    }

		    return false;
	    }
	    
	    private Vector3 PlacementPosition()
	    {
		    return new Vector3(Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width - GameSettings.ChunkLoaderRadius * Chunk.Width * .5f,
				    0,
				    Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width - GameSettings.ChunkLoaderRadius * Chunk.Width * .5f)
			    + _player.Position.Xz.ToVector3();
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
