  /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.StructureSystem
{
	/// <summary>
	/// Description of StructureGenerator.
	/// </summary>
	public class StructureGenerator
	{
	    public readonly List<CollidableStructure> Items;
		public Vector3 MerchantPosition { get; set; }
        public bool MerchantSpawned { get; set; }

	    public StructureGenerator()
	    {
	        Items = new List<CollidableStructure>();
	    }

	    public void CheckStructures(Vector2 ChunkOffset)
	    {
	        var underChunk = World.GetChunkAt(ChunkOffset.ToVector3());
	        var designs = underChunk != null 
                ? underChunk.Biome.Structures.Designs 
                : World.BiomePool.GetRegion(ChunkOffset.ToVector3()).Structures.Designs;
	        for (var i = 0; i < designs.Length; i++)
	        {
                if(designs[i].MeetsRequirements(ChunkOffset))
                    designs[i].CheckFor(ChunkOffset);
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
