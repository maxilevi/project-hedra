  /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.StructureSystem
{
	/// <summary>
	/// Description of StructureGenerator.
	/// </summary>
	internal class StructureGenerator
	{
	    public readonly List<CollidableStructure> Items;
		public Vector3 MerchantPosition { get; set; }
        public bool MerchantSpawned { get; set; }
        public Voronoi SeedGenerator { get; }

	    public StructureGenerator()
	    {
	        Items = new List<CollidableStructure>();
	        SeedGenerator = new Voronoi();
        }

	    public void CheckStructures(Vector2 ChunkOffset)
	    {
            if(!World.IsChunkOffset(ChunkOffset))
                throw new ArgumentException("Provided paramater does not represent a valid offset");

	        var underChunk = World.GetChunkAt(ChunkOffset.ToVector3());
	        var region = underChunk != null 
                ? underChunk.Biome 
                : World.BiomePool.GetRegion(ChunkOffset.ToVector3());
	        var designs = region.Structures.Designs;
	        for (var i = 0; i < designs.Length; i++)
	        {
                if(designs[i].MeetsRequirements(ChunkOffset))
                    designs[i].CheckFor(ChunkOffset, region);
	        }
	    }

	    public void Build(Vector3 Position, CollidableStructure Struct)
	    {
	        Position = new Vector3(Position.X, Physics.HeightAtPosition(Position), Position.Z);
	        Struct.Generated = true;
            Struct.Design.Build(Position, Struct);
	    }
		
		public void Dispose(){
			this.MerchantPosition = Vector3.Zero;
		    this.MerchantSpawned = false;
            for (int i = Items.Count-1; i > -1; i--)
		    {
		        Items.RemoveAt(i);
		    }
            Items.Clear();
		}
	}
}
