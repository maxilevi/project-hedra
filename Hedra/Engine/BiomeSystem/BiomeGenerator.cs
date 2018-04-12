/*
 * Author: Zaphyk
 * Date: 15/02/2016
 * Time: 09:19 p.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.BiomeSystem
{
	/// <summary>
	/// Description of BiomeGenerator.
	/// </summary>
	public abstract class BiomeGenerator : IDisposable
	{
		protected int OffsetX, OffsetZ;
		protected Chunk Chunk;
		protected int Seed;
		protected Block[][][] _blocks;
		public Random RandomGen;
		public bool BlocksSetted {get; set;}
		public bool StructuresPlaced {get; set;}

	    protected BiomeGenerator(Chunk RefChunk){
			this.RandomGen = BiomeGenerator.GenerateRng(new Vector2(RefChunk.OffsetX, RefChunk.OffsetZ));
			this.OffsetX = RefChunk.OffsetX;
			this.OffsetZ = RefChunk.OffsetZ;
			this._blocks = RefChunk.Voxels;
			this.Chunk = RefChunk;
			this.Seed = World.Seed;
		}
		
		public virtual void Regenerate(){
			this.RandomGen = new Random(World.Seed + this.Chunk.OffsetX + this.Chunk.OffsetZ);
			this.BuildArray();
			this.DefineBlocks();
		}

		public abstract void Generate();
		
		public abstract void DefineBlocks();
		
		public void BuildArray(){
			for(var x = 0; x < (int) (Chunk.Width / Chunk.BlockSize); x++){
				_blocks[x] = new Block[Chunk.Height][];
				for(var y = 0; y <  Chunk.Height; y++){
					_blocks[x][y] = new Block[(int) (Chunk.Width / Chunk.BlockSize)];
				}
			}
			BlocksSetted = true;
		    this.Chunk.CalculateBounds();
        }
		
		public static float PathFormula(float x, float z){
			return (float) Math.Max(0, (0.5 - Math.Abs(OpenSimplexNoise.Evaluate(x * 0.0009f, z *  0.0009f) - 0.2)) - 0.425f) * 1f;
		}
		
		public static float SmallFrequency(float x, float z){
			return (float) (OpenSimplexNoise.Evaluate(x * 0.2, z * 0.2) * -0.15f * OpenSimplexNoise.Evaluate(x * 0.035, z * 0.035) * 2.0f);
		}
		
		public virtual void PlaceStructures(){
			this.StructuresPlaced = true;
		}
		
		public static Random GenerateRng(Vector2 Offset){
			return new Random(World.Seed + (int) (Offset.X / 11.0 *  (Offset.Y / 13.0)) );
        }
		
		public void Dispose(){
			try{
				if(BlocksSetted){
					lock(_blocks)
						_blocks = null;
				}
			}catch(Exception e){
				Log.WriteLine("Could not dispose the chunk properly.");
			}
		}
	}
}
 