/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 05:26 p.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
	/// <inheritdoc />
	/// <summary>
	/// Description of MountainGenerator.
	/// </summary>
	public class LandscapeGenerator : BiomeGenerator
	{
		private const float narrow = 0.42f;
		private const float border = 0.02f;
		private const float scale = 1f;
		private const float riverDepth = 8;
		private const float riverMult = 165f;
		private const float pathDepth = 2.25f;
		
        public LandscapeGenerator(Chunk Chunk) : base(Chunk)
	    {
	    }

		public override void Generate(Block[][][] Blocks, RegionCache Cache)
        {
			this.CheckForNearbyStructures();
			this.BuildArray(Blocks);
            this.DefineBlocks(Blocks, Cache);
            this.BlocksSetted = true;
            this.PlaceStructures(Blocks, Cache);
			this.StructuresPlaced = true;
		}

	    public override void DefineBlocks(Block[][][] Blocks, RegionCache Cache)
	    {
	        var rng = new Random(World.Seed + 1234123);

	        var heightCache = new Dictionary<Vector2, float[]>();

	        const int noiseScale = 4;
	        var width = (int) (Chunk.Width / Chunk.BlockSize);
	        var depth = (int) (Chunk.Width / Chunk.BlockSize);

	        var noise3D = new float[0]; //Chunk.ChunkHeight / noiseScale];

	        var plateaus = World.WorldBuilding.Plateaus;
	        var groundworks = World.WorldBuilding.Groundworks.ToList();

	        var structs = World.StructureGenerator.Structures;

	        var biomeGen = Chunk.Biome.Generation;
	        var hasRiver = biomeGen.HasRivers ? 1f : 0f;
	        var hasPath = biomeGen.HasPaths ? 1f : 0f;
		    
	        for (var x = 0; x < width; x++)
	        {

	            for (var z = 0; z < depth; z++)
	            {

	                var position = new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ);
	                float height = Chunk.Biome.Generation.GetHeight(position.X, position.Y, heightCache, out BlockType type);

		            this.HandleStructures(x, z, position, groundworks, plateaus, structs, heightCache, noise3D, biomeGen,
			            hasPath, hasRiver, noiseScale, ref height, out var town, out var townClamped, out var makeDirt,
			            out var pathClamped, out var river, out var path, out var riverBorders, out var blockGroundworksModifier,
			            out var blockGroundworks);

		            var hasSubType = biomeGen.HasHeightSubtype(position.X, position.Y, heightCache);

	                for (var y = 0; y < Chunk.Height - 1; y++)
	                {
		                var currentBlock = Blocks[x][y][z];
	                    float noise = 0; /*Mathf.Lerp(
                                    noise3D[ (int) Math.Floor(y / (float) noiseScale) ],
                                    noise3D[ (int) Math.Min(noise3D.Length-1, Math.Floor(y / (float) noiseScale) + 1) ],
								    (y / (float) noiseScale) - (int) Math.Floor(y / (float)noiseScale));*/

	                    type = hasSubType ? biomeGen.GetHeightSubtype(position.X, y, position.Y, height, type, heightCache) : type;

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
	                        if ((World.Seed == World.MenuSeed || true) && makeDirt)
	                            currentBlock.Type = BlockType.Dirt;

	                    if (y < height + river)
	                    {
	                        if (currentBlock.Type == BlockType.Air && river > 0)
	                        {
	                            currentBlock.Type = BlockType.Water;
	                            Chunk.AddWaterDensity(new Vector3(x, y, z), (Half) (height + river));
	                        }
	                        else if (Mathf.Clamp(riverBorders * 100f, 0, riverDepth) > 2 &&
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
		                
		                this.HandleGroundworks(Blocks, x, y, z, path, pathDepth, pathClamped, town, townClamped,
			                blockGroundworks, blockGroundworksModifier);
	                }
	            }
	        }
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
			path = Mathf.Clamp(path * 100f, 0, pathDepth);

			river = hasRiver * River(x,z, narrow, scale);
			riverBorders = hasRiver * River(x,z, narrow, scale, border);
			float amplifiedRiverBorders = Mathf.Clamp(riverBorders * riverMult, 0, riverDepth);

			river = Mathf.Clamp(river * riverMult, 0, riverDepth);
			path = Mathf.Lerp(path, 0, river / riverDepth);

			blockGroundworksModifier = 1.0f;
			this.HandlePlateaus(x, z, smallFrequency, ref height, plateaus, nearGiantTree, nearCollidableStructure,
				ref river, ref riverBorders, ref path, ref blockGroundworksModifier, ref pathClamped, out var inPlateau);

			height = Math.Max(0, height + Chunk.BaseHeight);
			
			for (var i = 0; i < noise3D.Length; i++)
			{
				if (World.MenuSeed != World.Seed)
					noise3D[i] = biomeGen.GetDensity(OffsetX + x * Chunk.BlockSize,
						i * noiseScale * Chunk.BlockSize,
						OffsetZ + z * Chunk.BlockSize, heightCache);

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
					river = Mathf.Clamp(Mathf.Lerp(0, river, 1 - Math.Abs(noise3D[i])), 0, river);
					riverBorders = Mathf.Clamp(Mathf.Lerp(0, riverBorders, 1 - Math.Abs(noise3D[i])), 0,
						riverBorders);
				}
				path = Mathf.Lerp(path, 0, noise3D[i]);
			}

			float riverLerp = amplifiedRiverBorders / riverDepth;
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
		
		private Plateau GetBiggestPlateauForPosition(Vector2 Position, Plateau[] Plateaux)
		{
			Plateau biggestPlateau = null;
			foreach (Plateau plateau in Plateaux)
			{
				var currentDist = 1 - Math.Min((plateau.Position.Xz - Position).LengthSquared / (plateau.Radius * plateau.Radius), 1);
				if (currentDist <= 0) continue;

				if (plateau.Radius > (biggestPlateau?.Radius ?? 0))
				{
					biggestPlateau = plateau;
				}
			}

			return biggestPlateau;
		}

	    private bool IsBlockChangeable(BlockType Type)
	    {
	        return Type != BlockType.Air && Type != BlockType.Water && Type != BlockType.Seafloor;
	    }

	    public override void PlaceStructures(Block[][][] Blocks, RegionCache Cache)
	    {
		    var structs = World.StructureGenerator.Structures;
	        for (var x = 0; x < this.Chunk.BoundsX; x++)
	        {
	            for (var z = 0; z < this.Chunk.BoundsZ; z++)
	            {
	                int y = Chunk.GetHighestY(x, z);
                    var position = new Vector3(Chunk.OffsetX + x * Chunk.BlockSize, y-1, Chunk.OffsetZ + z * Chunk.BlockSize);
	                var block = Blocks[x][y][z];

                    if(block.Type == BlockType.Seafloor) continue;
	                
	                Region region = Cache.GetRegion(position);
	                this.LoopStructures(x, z, structs, out bool noWeedZone, out bool noTreesZone, out bool inMerchant);
	                this.DoEnviromentPlacements(position, noWeedZone, region);

	                if (block.Type != BlockType.Grass) continue;
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

	                if(noTreesZone) continue;

	                float noise = (float) OpenSimplexNoise.Evaluate(position.X * 0.005f, position.Z * 0.005f);
	                World.TreeGenerator.GenerateTree(position, region, region.Trees.GetDesign( (int) (noise * 10000) ));
	            }
	        }
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