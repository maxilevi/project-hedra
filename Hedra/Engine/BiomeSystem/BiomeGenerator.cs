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
		protected int OffsetX { get; }
        protected int OffsetZ { get; }
        protected Chunk Chunk { get; }
		protected int Seed { get; }
        protected Block[][][] Blocks { get; private set; }
        public Random RandomGen { get; set; }
		public bool BlocksSetted { get; set; }
		public bool StructuresPlaced { get; set; }

	    protected BiomeGenerator(Chunk RefChunk)
        {
			this.RandomGen = BiomeGenerator.GenerateRng(new Vector2(RefChunk.OffsetX, RefChunk.OffsetZ));
			this.OffsetX = RefChunk.OffsetX;
			this.OffsetZ = RefChunk.OffsetZ;
			this.Blocks = RefChunk.Voxels;
			this.Chunk = RefChunk;
			this.Seed = World.Seed;
		}
		
		public virtual void Regenerate()
        {
			this.RandomGen = new Random(World.Seed + this.Chunk.OffsetX + this.Chunk.OffsetZ);
			this.BuildArray();
			this.DefineBlocks();
		}

		public abstract void Generate();
		
		public abstract void DefineBlocks();
		
		public void BuildArray(){
			for(var x = 0; x < (int) (Chunk.Width / Chunk.BlockSize); x++)
            {
				Blocks[x] = new Block[Chunk.Height][];
				for(var y = 0; y <  Chunk.Height; y++)
                {
					Blocks[x][y] = new Block[(int) (Chunk.Width / Chunk.BlockSize)];
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

	    static int seed2(int _s)
	    {
	        var s = 192837463 ^ System.Math.Abs(_s);
	        var a = 1664525;
	        var c = 1013904223;
	        var m = 4294967296;
	        return (int)((s * a + c) % m);
	    }

	    static int GetSeedXY(int x, int y)
	    {
	        int sx = seed2(x * 1947);
	        int sy = seed2(y * 2904);
	        return seed2(sx ^ sy);
	    }

        public static Random GenerateRng(Vector2 Offset){
			return new Random( seed2(seed2((int) Offset.X * 1947) ^ seed2((int) Offset.Y * 2904)) );
        }
		
		public void Dispose(){
			try{
				if(BlocksSetted){
					lock(Blocks)
						Blocks = null;
				}
			}catch(Exception e){
				Log.WriteLine("Could not dispose the chunk properly.");
			}
		}
	}
}
 