﻿using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.LavaBiome
{
    public class LavaBiomeGenerationDesign :  BiomeGenerationDesign
    {
        public override bool HasRivers { get; set; } = true;
        public override bool HasPaths { get; set; } = true;
        public override bool HasDirt { get; set; } = true;

        public override float GetDensity(float X, float Y, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            double Density = 0;
            //if(World.MenuSeed != World.Seed){

            if (HeightCache.ContainsKey(new Vector2(X, Z)))
            {
                double Mult = HeightCache[new Vector2(X, Z)][0];

                //if(Mult > 0.0)
                //	Density -=  Mathf.Clamp( 0.1 * Mult * OpenSimplexNoise.Evaluate(x * 0.0025, y * 0.0025,  z * 0.0025) * 32.0, 0, Mult);

            }

            return (float)Density;
        }

        public override BlockType GetHeightSubtype(float X, float Y, float Z, float CurrentHeight, BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            if (HeightCache.ContainsKey(new Vector2(X, Z)))
            {
                double height = HeightCache[new Vector2(X, Z)][1];
                double realHeight = (CurrentHeight - height - Chunk.BaseHeight) / HeightCache[new Vector2(X, Z)][2];

                if (Math.Abs(realHeight - 32.0) < 0.5f)
                {
                    if (Y > 28.0)
                        return BlockType.Grass;
                }
                return Type;
            }
            return Type;
        }

        public override float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache, out BlockType Blocktype)
        {

            double Height = 0;

            //if(World.MenuSeed != World.Seed){
            //START BASE
            double BaseHeight = OpenSimplexNoise.Evaluate(X * 0.0004, Z * 0.0004) * 48.0;
            double GrassHeight = (OpenSimplexNoise.Evaluate(X * 0.004, Z * 0.004) + .25) * 3.0;
            Blocktype = BlockType.Grass;

            Height += BaseHeight + GrassHeight;
            //END BASE

            //START MOUNTAIN

            double GrassMountHeight = Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.0008, Z * 0.0008) * 80.0);
            if (GrassMountHeight != 0)
            {
                Blocktype = BlockType.Grass;
            }
            Height += GrassMountHeight;
            //END MOUNTAIN

            //BIG MOUNTAINS
            double SmallMountainHeight = 0;//Math.Max(0, OpenSimplexNoise.Evaluate(x * 0.01,  z *0.01) - .75f) * 128.0;

            if (SmallMountainHeight != 0 && BaseHeight > 0)
            {

                Height += SmallMountainHeight;
            }

            double LakeHeight = Mathf.Clamp((OpenSimplexNoise.Evaluate(X * 0.001, Z * 0.001) - .4f) * 48.0, 0, 32);
            // Height -= LakeHeight * 4;	

            Height += Mathf.Clamp(OpenSimplexNoise.Evaluate(X * 0.001, Z * 0.001) * 64.0, 0, float.MaxValue);
            Height += OpenSimplexNoise.Evaluate(X * 0.01, Z * 0.01) * 2.0;

            Height += OpenSimplexNoise.Evaluate(X * 0.02, Z * 0.02) * 1.5;
            //Small Frequency
            Height += BiomeGenerator.SmallFrequency(X, Z);

            double MountHeight = Mathf.Clamp((OpenSimplexNoise.Evaluate(X * 0.004, Z * 0.004) - .6f) * 2048.0, 0.0, 32.0);

            if (MountHeight > 0)
            {
                Blocktype = BlockType.Stone;

                double Mod = (World.MenuSeed == World.Seed) ? 0 : .4f;//Mathf.Clamp(OpenSimplexNoise.Evaluate(x * 0.005f, z * 0.005f) * .5f - .1f, 0.0, 2.0);

                MountHeight *= Mod;
                var Mult = 1;//Mathf.Clamp(OpenSimplexNoise.Evaluate(x * 0.005, z * 0.005) * 4.0, 0, 1);
                MountHeight *= Mult;

                if (MountHeight > 0)
                    HeightCache?.Add(new Vector2(X, Z), new[] { (float)MountHeight, (float)Height, (float)(Mult * Mod) });
                Height += MountHeight;
            }

            if (MountHeight <= 1.0)
                Blocktype = BlockType.Grass;

            if (SmallMountainHeight > 0)
                Blocktype = BlockType.Stone;

            return (float)Height;

        }
    }
}
