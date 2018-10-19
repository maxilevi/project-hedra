﻿/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 05:26 p.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem;
using OpenTK;
using SimplexNoise;

namespace Hedra.Engine.BiomeSystem
{
	/// <inheritdoc />
	/// <summary>
	/// Description of MountainGenerator.
	/// </summary>
	public class LandscapeGenerator : BiomeGenerator
	{
		private const float Narrow = 0.42f;
		private const float Border = 0.02f;
		private const float Scale = 1f;
		private const float RiverDepth = 8;
		private const float RiverMult = 165f;
		private const float PathDepth = 2.25f;
		private bool _firstGeneration = true;
		
        public LandscapeGenerator(Chunk Chunk) : base(Chunk)
	    {
	    }

		public override void Generate(Block[][][] Blocks, RegionCache Cache)
		{
			if (FullyGenerated) throw new ArgumentException($"Cannot generate a chunk multiple times");
	        if (_firstGeneration)
	        {
		        this.CheckForNearbyStructures();
		        this.BuildArray(Blocks);
	            this.BlocksSetted = true;
            }

			var lod = Chunk.Lod;
	        if (_firstGeneration || lod < GeneratedLod)
	        {
		        bool BorderFilter(int X, int Z) => X < 2 || Z < 2 || X > Chunk.BoundsX - 3 || Z > Chunk.BoundsZ - 3;
		        if (_firstGeneration)
		        {
			        /*
			         * Generate borders for the LOD borders and the blocks needed for this LOD
			         */
			        this.DefineBlocks(Blocks, Cache, 1, BorderFilter);
			        this.DefineBlocks(Blocks, Cache, lod, (X, Z) => !BorderFilter(X, Z));
                }
                else
		        {
			        /*
			         * If the chunk was partially generated, that means some data already exists. So we
			         * generate everything that wasnt generated by the previous lod.
			         */
		            bool Filter(int X, int Z) => !BorderFilter(X, Z) && (X % GeneratedLod != 0 || Z % GeneratedLod != 0);
                    this.DefineBlocks(Blocks, Cache, lod, Filter);
                }
	        }
		    GeneratedLod = lod;
            if (_firstGeneration)
		    {
			    this.DoTreeAndStructurePlacements(Blocks, Cache, lod);
            }
			if (lod == 1)
			{
				this.PlaceEnviroment(Blocks, Cache);
			}
			this.StructuresPlaced = true;
	        this._firstGeneration = false;
	        this.FullyGenerated = lod == 1;
        }

	    public override void DefineBlocks(Block[][][] Blocks, RegionCache Cache, int Lod, Func<int, int, bool> Filter)
	    {
	        var rng = new Random(World.Seed + 1234123);

	        var heightCache = new Dictionary<Vector2, float[]>();

	        const int noiseScale = 1;
	        var width = (int) (Chunk.Width / Chunk.BlockSize);
	        var depth = (int) (Chunk.Width / Chunk.BlockSize);

	        var noise3D = new float[0/*Chunk.Height / noiseScale*/];

	        var plateaus = World.WorldBuilding.Plateaus;
	        var groundworks = World.WorldBuilding.Groundworks.ToList();

	        var structs = World.StructureGenerator.Structures;

	        var biomeGen = Chunk.Biome.Generation;
	        var hasRiver = biomeGen.HasRivers ? 1f : 0f;
	        var hasPath = biomeGen.HasPaths ? 1f : 0f;
		    
	        for (var x = 0; x < width; x+=Lod)
	        {

	            for (var z = 0; z < depth; z+=Lod)
	            {
                    if(!Filter(x,z)) continue;
	                var position = new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ);
	                float height = Chunk.Biome.Generation.GetHeight(position.X, position.Y, heightCache, out BlockType type);

		            this.HandleStructures(x, z, position, groundworks, plateaus, structs, heightCache, noise3D, biomeGen,
			            hasPath, hasRiver, noiseScale, ref height, out var town, out var townClamped, out var makeDirt,
			            out var pathClamped, out var river, out var path, out var riverBorders, out var blockGroundworksModifier,
			            out var blockGroundworks);

		            var hasSubType = biomeGen.HasHeightSubtype(position.X, position.Y, heightCache);

	                for (var y = 0; y < Chunk.Height - 1; y++)
	                {
		                type = hasSubType 
			                ? biomeGen.GetHeightSubtype(position.X, y, position.Y, height, type, heightCache) 
			                : type;
		                
		                this.GenerateBlock(Blocks, type, x, y, z, height, river, makeDirt, riverBorders,
			                ref townClamped, rng, noise3D, noiseScale);
		                			
		                this.HandleGroundworks(Blocks, x, y, z, path, PathDepth, pathClamped, town, townClamped,
			                blockGroundworks, blockGroundworksModifier);
	                }
	            }
	        }
	    }
		
		private void GenerateBlock(Block[][][] Blocks, BlockType type, int x, int y, int z, float height, float river,
			bool makeDirt, float riverBorders, ref bool townClamped, Random rng, float[] noise3D, int noiseScale)
		{
			var currentBlock = Blocks[x][y][z];
		    var noise = 0;//noise3D[y];
	
			if (noise != 0 && townClamped)
				townClamped = false;
	
			currentBlock.Type = BlockType.Air;
	
			currentBlock.Density = new Half(1 - (y - height) + noise);
	
			if (y < 2)
				currentBlock.Density = new Half(0.95f + rng.NextFloat() * 0.75f);
	
			if (currentBlock.Density > 0)
			{
	
				currentBlock.Type = type;
	
				if (height - y > 3.0f)
				{
					currentBlock.Type = BlockType.Stone;
				}
	
				if (y < 2)
					currentBlock.Type = BlockType.Seafloor;
	
				if (noise != 0)
					currentBlock.Noise3D = true;
	
			}

		    if (currentBlock.Type == BlockType.Grass)
		    {
		        if (makeDirt) currentBlock.Type = BlockType.Dirt;
		    }

		    if (y < height + river)
			{
				if (currentBlock.Type == BlockType.Air && river > 0)
				{
					currentBlock.Type = BlockType.Water;
					Chunk.AddWaterDensity(new Vector3(x, y, z), (Half) (height + river));
				}
				else if (Mathf.Clamp(riverBorders * 100f, 0, RiverDepth) > 2 &&
						 currentBlock.Type != BlockType.Air)
				{
					currentBlock.Type = BlockType.Seafloor;
					for (var i = 0; i < y; i++) Blocks[x][i][z].Type = BlockType.Seafloor;
				}
			}
	
			if (y <= BiomePool.SeaLevel && currentBlock.Type == BlockType.Air && y >= 1 &&
				Blocks[x][y - 1][z].Type != BlockType.Seafloor && Blocks[x][y - 1][z].Type != BlockType.Air &&
				Blocks[x][y - 1][z].Type != BlockType.Water)
			{
				if (y < BiomePool.SeaLevel)
				{
					currentBlock.Type = BlockType.Seafloor;
					for (var i = 0; i < y; i++)
					{
						Blocks[x][i][z].Type = BlockType.Seafloor;
					}
				}
			}
	
			var isOcean = y > 0 && y <= BiomePool.SeaLevel &&
						  (Blocks[x][y - 1][z].Type == BlockType.Seafloor ||
						   Blocks[x][y - 1][z].Type == BlockType.Water) &&
						  currentBlock.Type == BlockType.Air &&
						  Blocks[x][y + 1][z].Type == BlockType.Air;
			if (isOcean)
			{
				currentBlock.Type = BlockType.Water;
				Chunk.AddWaterDensity(new Vector3(x, y, z), (Half) BiomePool.SeaLevel);
			}
	
			Blocks[x][y][z] = currentBlock;
		}

		private void HandleStructures(int x, int z, Vector2 position, List<IGroundwork> groundworks, Plateau[] plateaus,
			CollidableStructure[] structs, Dictionary<Vector2, float[]> heightCache, float[] noise3D, RegionGeneration biomeGen,
			float hasPath, float hasRiver, float noiseScale, ref float height, out bool town, out bool townClamped,
			out bool makeDirt, out bool pathClamped, out float river, out float path, out float riverBorders,
			out float blockGroundworksModifier, out IGroundwork[] blockGroundworks)
		{
			town = false;
			townClamped = false;
			CollidableStructure nearCollidableStructure = null;
			float townHeight = 0;

			for(var i = 0; i < structs.Length; i++)
			{
				var item = structs[i];
				if (!(item.Design is VillageDesign)) continue;

				float villageRadius = item.Mountain.Radius;
				if (!((position - item.Position.Xz).LengthSquared < villageRadius * villageRadius)) continue;

				town = true;
				nearCollidableStructure = item;
				break;
			}     
			var smallFrequency = SmallFrequency(position.X, position.Y);
			var nearGiantTree = FindNearestGiantTree(position, structs);
			
			blockGroundworks = groundworks.Where(G => G.Affects(position)).ToArray();

			if (townClamped && townHeight != height)
				townClamped = false;

			pathClamped = false;
			path = hasPath * PathFormula(Chunk.BlockSize * x + Chunk.OffsetX, Chunk.BlockSize * z + Chunk.OffsetZ);
			path = Mathf.Clamp(path * 100f, 0, PathDepth);

			river = hasRiver * River(x,z, Narrow, Scale);
			riverBorders = hasRiver * River(x,z, Narrow, Scale, Border);
			float amplifiedRiverBorders = Mathf.Clamp(riverBorders * RiverMult, 0, RiverDepth);

			river = Mathf.Clamp(river * RiverMult, 0, RiverDepth);
			path = Mathf.Lerp(path, 0, river / RiverDepth);

			blockGroundworksModifier = 1.0f;
			this.HandlePlateaus(x, z, smallFrequency, ref height, plateaus, nearGiantTree, nearCollidableStructure,
				ref river, ref riverBorders, ref path, ref blockGroundworksModifier, ref pathClamped, out var inPlateau);

			height = Math.Max(0, height);
			
			for (var i = 0; i < noise3D.Length; i++)
			{
				if (World.MenuSeed != World.Seed)
				{
					noise3D[i] = biomeGen.GetDensity(OffsetX + x * Chunk.BlockSize,
						i * noiseScale * Chunk.BlockSize,
						OffsetZ + z * Chunk.BlockSize, heightCache);
				}

				/*
				if (inPlateau)
				{
					foreach (Plateau plateau in plateauPositions)
					{
						float plateauDist =
						(plateau.Position.Xz -
						 new Vector2(OffsetX + x * Chunk.BlockSize, OffsetZ + z * Chunk.BlockSize)).LengthFast;
						float plateauFinal = Math.Max(1 - plateauDist / plateau.Radius, 0);
						noise3D[i] = Mathf.Lerp(0, noise3D[i], 1 - Math.Min(1, plateauFinal * 3f));
					}
				}*/

				if (nearGiantTree == null)
				{
				    const int lerpFactor = 80;
				    noise3D[i] = Mathf.Lerp(noise3D[i], 0f, (float)Mathf.Clamp(riverBorders * lerpFactor, 0, 1));

				    /*noise3D[i] = Mathf.Clamp(
				        Mathf.Lerp(noise3D[i], 0f, path),
				        0, noise3D[i]);*/
                }
			}

			float riverLerp = amplifiedRiverBorders / RiverDepth;
			var pathGroundwork = blockGroundworks.FirstOrDefault(P => P.IsPath);
			var groundworkDensity = pathGroundwork?.Density(position) ?? 0;
			if ((riverLerp > 0 || path > 0) && !inPlateau)
			{
				if (heightCache.ContainsKey(new Vector2(x * Chunk.BlockSize + OffsetX,
					z * Chunk.BlockSize + OffsetZ)))
				{
					var cache = heightCache[new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ)][0];
					if (path > 0)
					{
						path = Mathf.Lerp(path, 0, Mathf.Clamp(cache / 32.0f, 0, 1.0f));
					}
					if (riverLerp > 0)
					{
						height -= Mathf.Lerp(0, cache, riverLerp);
					}
				}
			}
			if (blockGroundworks.Length > 0)
			{
				var bonusHeight = blockGroundworks[blockGroundworks.Length - 1].BonusHeight * blockGroundworks[blockGroundworks.Length - 1].Density(position);
				height += bonusHeight * blockGroundworksModifier;
			}
			if (pathGroundwork != null)
			{
				river = Mathf.Lerp(river, 0, groundworkDensity);
			}
			height -= river;
			height -= path;
			var dirtNoise = OpenSimplexNoise.Evaluate((x * Chunk.BlockSize + OffsetX) * 0.0175f,
								(z * Chunk.BlockSize + OffsetZ) * 0.0175f) > .35f;
			makeDirt = biomeGen.HasDirt && dirtNoise;
	                
		}

		private void HandlePlateaus(int x, int z, float smallFrequency, ref float height, Plateau[] plateaus, CollidableStructure nearGiantTree,
			CollidableStructure nearCollidableStructure, ref float river, ref float riverBorders, ref float path,
			ref float blockGroundworksModifier, ref bool pathClamped, out bool inPlateau)
		{
			inPlateau = false;
			for (var i = 0; i < plateaus.Length; i++)
			{
				var point = new Vector2(OffsetX + x * Chunk.BlockSize, OffsetZ + z * Chunk.BlockSize);
				var radius = plateaus[i].Radius;
				var dist = (plateaus[i].Position.Xz - point).LengthSquared;
				var final = Math.Max(1 - Math.Min(dist / (radius * radius), 1), 0);
				var addonHeight = plateaus[i].MaxHeight * Math.Max(final, 0f);

				height += addonHeight;
				height = Mathf.Lerp(height - addonHeight,
					Math.Min(plateaus[i].MaxHeight + smallFrequency, height),
					Math.Min(1.0f, final * 1.5f));
				
				if (nearGiantTree == null)
				{
					river = Mathf.Clamp(Mathf.Lerp(0, river, 1 - Math.Min(1, final * 3f)), 0, river);
					riverBorders = Mathf.Clamp(Mathf.Lerp(0, riverBorders, 1 - Math.Min(1, final * 3f)), 0,
						riverBorders);
				}

				if (path > 0 && nearCollidableStructure != null && plateaus[i] == nearCollidableStructure.Mountain)
					pathClamped = true;

				blockGroundworksModifier = Mathf.Clamp(Mathf.Lerp(0, blockGroundworksModifier, 1 - Math.Min(1, final * 3f)),
					0, blockGroundworksModifier);
				path = Mathf.Clamp(Mathf.Lerp(0, path, 1 - Math.Min(1, final * 3f)), 0, path);
				
				if (final < 0.005f)
				{
					inPlateau = true;
				}
			}
		}

		private static CollidableStructure FindNearestGiantTree(Vector2 position, CollidableStructure[] structs)
		{
			for(var i = 0; i < structs.Length; i++)
			{
				float radius = structs[i].Mountain?.Radius ?? float.MaxValue;
				if ((position - structs[i].Position.Xz).LengthSquared < radius * radius && structs[i].Design is GiantTreeDesign) return structs[i];
			}
			return null;
		}
		
		private void HandleGroundworks(Block[][][] Blocks, int X, int Y, int Z, float path, float pathDepth,
			bool pathClamped, bool town, bool townClamped, IGroundwork[] BlockGroundworks, float GroundworksModifier)
		{
			if (BlockGroundworks.Length > 0 || path == pathDepth || town)
			{
				if (this.IsBlockChangeable(Blocks[X][Y][Z].Type))
				{
					if (path > 0 && !town || pathClamped && !town)
					{
						Blocks[X][Y][Z].Type = BlockType.Path;
					}

					if (BlockGroundworks.Length > 0)
					{
						var groundwork = BlockGroundworks[BlockGroundworks.Length - 1];
						Blocks[X][Y][Z].Type = groundwork.Type;
					}

					if (town && townClamped)
					{
						if (Blocks[X][Y][Z].Type == BlockType.Stone)
						{
							Blocks[X][Y][Z].Type = BlockType.Grass;
						}
					}
				}
			}
		}
		
		private float River(int X, int Z, float Narrow, float Scale, float Border = 0)
		{
			return (float) Math.Max(0,
				       0.5 - Math.Abs(
					       OpenSimplexNoise.Evaluate((X * Chunk.BlockSize + Chunk.OffsetX) * 0.0011f,
						       (Chunk.BlockSize * Z + Chunk.OffsetZ) * 0.0011f) - 0.2) - Narrow +
				       Border) * Scale;
		}

	    private bool IsBlockChangeable(BlockType Type)
	    {
	        return Type != BlockType.Air && Type != BlockType.Water && Type != BlockType.Seafloor;
	    }

	    protected override void PlaceEnviroment(Block[][][] Blocks, RegionCache Cache)
	    {
		    var structs = World.StructureGenerator.Structures;
	        for (var x = 0; x < this.Chunk.BoundsX; x++)
	        {
	            for (var z = 0; z < this.Chunk.BoundsZ; z++)
	            {
                    int y = Chunk.GetHighestY(x, z);
                    var position = new Vector3(Chunk.OffsetX + x * Chunk.BlockSize, y-1, Chunk.OffsetZ + z * Chunk.BlockSize);

                    if(y < BiomePool.SeaLevel - Chunk.BlockSize) continue;

	                Region region = Cache.GetRegion(position);
	                this.LoopStructures(x, z, structs, out bool noWeedZone, out _, out _);
	                this.DoEnviromentPlacements(position, noWeedZone, region);
	            }
	        }
	    }

		private void DoTreeAndStructurePlacements(Block[][][] Blocks, RegionCache Cache, int Lod)
		{
			var structs = World.StructureGenerator.Structures;
			var plateaus = World.WorldBuilding.Plateaus.Where(P => P.NoTrees).ToArray();
			for (var _x = 0; _x < this.Chunk.BoundsX; _x++)
			{
				for (var _z = 0; _z < this.Chunk.BoundsZ; _z++)
				{
					var coordinates = GetNearest(_x, _z, Lod);
					var x = (int) coordinates.X;
					var z = (int) coordinates.Y;
					var y = Chunk.GetHighestY(x, z);
					var block = Blocks[x][y][z];

					if(block.Type != BlockType.Grass) continue;
					this.LoopStructures(_x, _z, structs, out _, out var noTreesZone, out _);
					this.LoopPlateaus(_x, _z, plateaus, out var noTreesPlateau);
					
					if (World.Seed == World.MenuSeed)
					{
						//Is menu, force a platform
						if ((Scenes.MenuBackground.DefaultPosition.Xz - new Vector2(
							     Chunk.OffsetX + x * Chunk.BlockSize,
							     Chunk.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 32 * 32) continue;
						if ((Scenes.MenuBackground.CreatorPosition.Xz - new Vector2(
							     Chunk.OffsetX + x * Chunk.BlockSize,
							     Chunk.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 32 * 32) continue;
						if ((Scenes.MenuBackground.CampfirePosition.Xz - new Vector2(
							     Chunk.OffsetX + x * Chunk.BlockSize,
							     Chunk.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 48 * 48) continue;
					}

					if(noTreesZone || noTreesPlateau) continue;

					var realPosition = new Vector3(Chunk.OffsetX + _x * Chunk.BlockSize, y-1, Chunk.OffsetZ + _z * Chunk.BlockSize);
					var samplingPosition = new Vector3(Chunk.OffsetX + x * Chunk.BlockSize, y-1, Chunk.OffsetZ + z * Chunk.BlockSize);
					
					var region = Cache.GetRegion(realPosition);
					var noise = (float) OpenSimplexNoise.Evaluate(realPosition.X * 0.005f, realPosition.Z * 0.005f);
					var placementObject = World.TreeGenerator.CanGenerateTree(samplingPosition, region, Lod);
					if (!placementObject.Placed) continue;
					placementObject.Position += -samplingPosition.Xz.ToVector3() + realPosition.Xz.ToVector3();
					World.TreeGenerator.GenerateTree(placementObject, region, region.Trees.GetDesign( (int) (noise * 10000) ));
				}
			}
		}

		private void LoopPlateaus(int X, int Z, Plateau[] Plateaus, out bool NoTrees)
		{
			for (var i = 0; i < Plateaus.Length; i++)
			{
				if (Plateaus[i].Collides(new Vector2(X * Chunk.BlockSize + OffsetX, Z * Chunk.BlockSize + OffsetZ)))
				{
					NoTrees = true;
					return;
				}
			}
			NoTrees = false;
		}
		
		private Vector2 GetNearest(int X, int Z, int Lod)
		{
			var directionX = X == 0 ? +(X % Lod) : X == Chunk.BoundsX - Lod ? -(X % Lod) : 0; 
			var directionZ = Z == 0 ? +(Z % Lod) : Z == Chunk.BoundsZ - Lod ? -(Z % Lod) : 0;
			return new Vector2(X % Lod == 0 ? X : X + directionX, Z % Lod == 0 ? Z : Z + directionZ);
		}
		

	    private void DoEnviromentPlacements(Vector3 Position, bool HideEnviroment, Region Biome)
	    {
	        var designs = Biome.Enviroment.Designs;
	        for (var i = 0; i < designs.Length; i++)
	        {
	            if(designs[i].CanBeHidden && HideEnviroment) continue;
	            if (designs[i].ShouldPlace(Position, this.Chunk))
	            {
	                var design = designs[i].GetDesign(Position, this.Chunk);
                    World.EnviromentGenerator.GeneratePlant(Position, Biome, design);
	            }
	        }
	    }

	    private void LoopStructures(int X, int Z, CollidableStructure[] Structs,
            out bool NoGrassZone, out bool NoTreesZone, out bool InMerchant)
	    {
	        NoGrassZone = false;
	        NoTreesZone = false;
	        InMerchant = false;
	        foreach (CollidableStructure structPosition in Structs)
	        {
	            var possiblePosition = new Vector3(Chunk.OffsetX + X * Chunk.BlockSize, 0,
	                Chunk.OffsetZ + Z * Chunk.BlockSize);

	            if (structPosition.Design is GiantTreeDesign)
	            {
	                float radius = structPosition.Mountain.Radius;
	                if ((structPosition.Mountain.Position.Xz - possiblePosition.Xz).LengthSquared <
	                    radius * .3f * radius * .3f)
	                {
	                    NoGrassZone = true;
	                }

	                if (!NoTreesZone && (structPosition.Mountain.Position.Xz - possiblePosition.Xz).LengthSquared <
	                    radius * 0.5f * radius * 0.5f)
	                    NoTreesZone = true;

	            }

	            if (structPosition.Design is TravellingMerchantDesign)
	            {
	                var radius = structPosition.Mountain.Radius;
	                if ((structPosition.Mountain.Position.Xz - possiblePosition.Xz).LengthSquared <
	                    radius * .5f * radius * .5f)
	                    InMerchant = true;
	            }
	        }
        }
		
		public void CheckForNearbyStructures()
        {
			StructureGenerator.CheckStructures( new Vector2(Chunk.OffsetX, Chunk.OffsetZ) );
		}
	}
}