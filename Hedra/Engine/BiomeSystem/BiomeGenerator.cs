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
			this._blocks = RefChunk.Blocks;
			this.Chunk = RefChunk;
			this.Seed = World.Seed;
		}
		
		public virtual void Regenerate(){
			this.RandomGen = new Random(World.Seed + this.Chunk.OffsetX + this.Chunk.OffsetZ);
			this.SetupBlocks();
			this.DefineBlocks();
		}
		
		
		public static float GetDensity(float x, float y, float z, Dictionary<Vector2, float[]> HeightCache)
		{
			double Density = 0;
			//if(World.MenuSeed != World.Seed){
				
				if(HeightCache.ContainsKey(new Vector2(x,z))){
					double Mult = HeightCache[new Vector2(x,z)][0];
					
					//if(Mult > 0.0)
					//	Density -=  Mathf.Clamp( 0.1 * Mult * OpenSimplexNoise.Evaluate(x * 0.0025, y * 0.0025,  z * 0.0025) * 32.0, 0, Mult);

				}
				
				return (float) Density;
		}

	    public static BlockType GetHeightSubtype(float x, float y, float z, float currentHeight, BlockType type, Dictionary<Vector2, float[]> HeightCache)
	    {
	        if (HeightCache.ContainsKey(new Vector2(x, z)))
	        {
	            double height = HeightCache[new Vector2(x, z)][1];
	            double realHeight = (currentHeight - height - Chunk.BaseHeight) / HeightCache[new Vector2(x, z)][2];
                
	            if (Math.Abs(realHeight - 32.0) < 0.5f)
	            {
	                if (y > 28.0)
	                   return BlockType.Grass;
	            }
	            return type;
	        }
	        return type;
	    }

	    public static float GetHeight(float x, float z, Dictionary<Vector2, float[]> HeightCache, out BlockType blocktype){
			
			double Height = 0;

			//if(World.MenuSeed != World.Seed){
				//START BASE
				double BaseHeight = OpenSimplexNoise.Evaluate(x * 0.0004,  z *0.0004) * 48.0;
				double GrassHeight = (OpenSimplexNoise.Evaluate(x * 0.004,  z *0.004) + .25) * 3.0;
				blocktype = BlockType.Grass;
				
				Height += BaseHeight + GrassHeight;
            //END BASE

            //START MOUNTAIN

		    double GrassMountHeight = Math.Max(0, OpenSimplexNoise.Evaluate(x * 0.0008, z * 0.0008) * 80.0);
		    if (GrassMountHeight != 0)
		    {
		       blocktype = BlockType.Grass;
            }
		    Height += GrassMountHeight;
			//END MOUNTAIN
				
			//BIG MOUNTAINS
	        double SmallMountainHeight = 0;//Math.Max(0, OpenSimplexNoise.Evaluate(x * 0.01,  z *0.01) - .75f) * 128.0;
				
			if(SmallMountainHeight != 0 && BaseHeight > 0){
					
				Height += SmallMountainHeight;
			}

	        double LakeHeight = Mathf.Clamp( (OpenSimplexNoise.Evaluate(x * 0.001,  z *0.001) - .4f) * 48.0, 0, 32);
	        // Height -= LakeHeight * 4;	

            Height += Mathf.Clamp(OpenSimplexNoise.Evaluate(x * 0.001,  z *0.001) * 64.0, 0, float.MaxValue);
			Height += OpenSimplexNoise.Evaluate(x * 0.01,  z *0.01) * 2.0;
				
			Height += OpenSimplexNoise.Evaluate(x * 0.02,  z *0.02) * 1.5;
			//Small Frequency
			Height += SmallFrequency(x,z);

	        double MountHeight = Mathf.Clamp((OpenSimplexNoise.Evaluate(x * 0.004, z * 0.004) -.6f) * 2048.0, 0.0, 32.0);

            if (MountHeight > 0)
	        {
                blocktype = BlockType.Stone;

	            double Mod = (World.MenuSeed == World.Seed) ? 0 : .4f;//Mathf.Clamp(OpenSimplexNoise.Evaluate(x * 0.005f, z * 0.005f) * .5f - .1f, 0.0, 2.0);

	            MountHeight *= Mod;
	            var Mult = 1;//Mathf.Clamp(OpenSimplexNoise.Evaluate(x * 0.005, z * 0.005) * 4.0, 0, 1);
	            MountHeight *= Mult;

                if(MountHeight > 0)
	                HeightCache?.Add(new Vector2(x, z), new[] { (float) MountHeight, (float) Height, (float) (Mult * Mod) });
	            Height += MountHeight;
            }

            if(MountHeight <= 1.0)
                blocktype = BlockType.Grass;

            if(SmallMountainHeight > 0)
                blocktype = BlockType.Stone;

            return (float) Height;

		}
		
		public abstract void Generate();
		
		public abstract void DefineBlocks();
		
		public virtual void SetupBlocks(){
			for(int x = 0; x < (int) (Chunk.ChunkWidth / Chunk.BlockSize); x++){
				_blocks[x] = new Block[Chunk.ChunkHeight][];
				for(int y = 0; y <  Chunk.ChunkHeight; y++){
					_blocks[x][y] = new Block[(int) (Chunk.ChunkWidth / Chunk.BlockSize)];
				}
			}
			Chunk.BoundsX = _blocks.Length;
			Chunk.BoundsY = _blocks[0].Length;
			Chunk.BoundsZ = _blocks[0][0].Length;
			BlocksSetted = true;
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
 