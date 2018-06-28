﻿/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 05:26 p.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
	/// <inheritdoc />
	/// <summary>
	/// Description of MountainGenerator.
	/// </summary>
	internal class LandscapeGenerator : BiomeGenerator
	{
		public LandscapeGenerator(Chunk Chunk) : base (Chunk){}
		public override void Generate(){

			this.CheckForNearbyStructures();
			
			this.BuildArray();
			this.DefineBlocks();
			try
            {
				this.PlaceStructures();
			}
            catch (Exception e)
            {
				Log.WriteLine(e.ToString());
				World.RemoveChunk(Chunk);
				return;
			}
			StructuresPlaced = true;
		}
		
		public override void DefineBlocks()
        {
			var rng = new Random(World.Seed + 1234123);
			lock(Blocks)
            {
				try
                {
                    var heightCache = new Dictionary<Vector2, float[]>();

					const int noiseScale = 4;
				    var width = (int) (Chunk.Width / Chunk.BlockSize);
				    var depth = (int) (Chunk.Width / Chunk.BlockSize);

				    var noise3D = new float[0];//Chunk.ChunkHeight / noiseScale];
 
                    Plateau[] plateauPositions;

				    lock (World.QuestManager.Plateaus)
				        plateauPositions = World.QuestManager.Plateaus.ToArray();

				    CollidableStructure[] structs;

				    lock (World.StructureGenerator.Items)
				        structs = World.StructureGenerator.Items.ToArray();

				    var biomeGen = Chunk.Biome.Generation;
				    var hasRiver = biomeGen.HasRivers ? 1f : 0f;
				    var hasPath = biomeGen.HasPaths ? 1f : 0f;

				    for (var x = 0; x < width; x++)
					{
					    const float narrow = 0.42f;
					    const float border = 0.02f;
					    const float scale = 1f;
					    const float riverDepth = 8;
					    const float riverMult = 165f;
					    const float pathDepth = 2.25f;

					    for(var z = 0; z < depth; z++)
                        {
							
							var position = new Vector2(x * Chunk.BlockSize + OffsetX, z* Chunk.BlockSize + OffsetZ);
					        float height = Chunk.Biome.Generation.GetHeight(position.X, position.Y, heightCache, out BlockType type);
                            float dist, final;

                            #region Structure stuff
                            var town = false;
					        var townClamped = false;
					        CollidableStructure nearCollidableStructure = null;
							float townHeight = 0;

					        foreach (CollidableStructure item in structs)
					        {
					            if (!(item.Design is VillageDesign) ) continue;

                                float villageRadius = item.Mountain.Radius;
					            if (!((position - item.Position.Xz).LengthSquared < villageRadius * villageRadius)) continue;

					            town = true;
					            nearCollidableStructure = item;
					            break;
					        }
					        
                            var giantTree = false;

					        CollidableStructure nearGiantTree = (from item in structs
                                    let radius = item.Mountain?.Radius ?? float.MaxValue
                                 where (position - item.Position.Xz).LengthSquared < radius * radius && item.Design is GiantTreeDesign
                                    select item).FirstOrDefault();

						    var inPlateau = false;

					        Plateau biggestPlateau = null;
					        foreach (Plateau plateau in plateauPositions)
					        {
					            var currentDist = 1 - Math.Min((plateau.Position.Xz - position).LengthSquared / (plateau.Radius * plateau.Radius), 1);
                                if(currentDist <= 0) continue;

					            if (plateau.Radius > (biggestPlateau?.Radius ?? 0))
					            {
					                biggestPlateau = plateau;
					            }
					        }
					        
					        foreach (Plateau plateau in plateauPositions)
					        {

					            dist = (plateau.Position.Xz - position).LengthSquared;
					            final = Math.Max(1 - Math.Min(dist / (plateau.Radius * plateau.Radius), 1), 0);
					            float addonHeight = plateau.Height * Math.Max(final, 0f);

					            height += addonHeight;
					            height = Mathf.Lerp(height - addonHeight,
					                Math.Min((plateau.Radius < 64 ? biggestPlateau ?? plateau : plateau).MaxHeight + SmallFrequency(position.X, position.Y),
					                    height),
					                Math.Min(1.0f, final * 1.5f));


					            if (final > 0 && nearGiantTree != null && plateau == nearGiantTree.Mountain) giantTree = true;

					            if (final > 0) inPlateau = true;
					        }
					        
					        if(townClamped && townHeight != height)
								townClamped = false;

					        var pathClamped = false;
					        float path = hasPath * BiomeGenerator.PathFormula(Chunk.BlockSize * x + Chunk.OffsetX, Chunk.BlockSize * z + Chunk.OffsetZ);
					        path = Mathf.Clamp(path * 100f, 0, pathDepth);

					        float river = hasRiver * (float) Math.Max(0, 0.5 - Math.Abs(OpenSimplexNoise.Evaluate((x * Chunk.BlockSize + Chunk.OffsetX) * 0.0011f, (Chunk.BlockSize * z + Chunk.OffsetZ) *  0.0011f) - 0.2) - narrow) * scale;
					        float riverBorders = hasRiver * (float) Math.Max(0, 0.5 - Math.Abs(OpenSimplexNoise.Evaluate((x * Chunk.BlockSize + Chunk.OffsetX) * 0.0011f, (Chunk.BlockSize * z + Chunk.OffsetZ) *  0.0011f) - 0.2) - narrow+border) * scale;
						    river = Mathf.Clamp(river * riverMult, 0, riverDepth);
					        float amplifiedRiverBorders = Mathf.Clamp(riverBorders * riverMult, 0, riverDepth);

                            height = Math.Max(0, height + Chunk.BaseHeight );
					        path = Mathf.Lerp(path, 0, river / riverDepth);

                            
                            foreach (Plateau plateau in plateauPositions)
							{
							    float plateauDist = ( plateau.Position.Xz - new Vector2(OffsetX + x*Chunk.BlockSize, OffsetZ + z*Chunk.BlockSize) ).LengthFast;
							    float plateauFinal = Math.Max(1-plateauDist / plateau.Radius, 0);
								
							    //Dont do this near trees
							    if(nearGiantTree == null){
							        river = Mathf.Clamp(Mathf.Lerp(0, river, 1-Math.Min( 1, plateauFinal * 3f)), 0, river);
							        riverBorders = Mathf.Clamp(Mathf.Lerp(0, riverBorders, 1-Math.Min( 1, plateauFinal * 3f)), 0, riverBorders);
							    }
								
							    if(path > 0 && nearCollidableStructure != null && plateau == nearCollidableStructure.Mountain)
							        pathClamped = true;
								
							    path = Mathf.Clamp(Mathf.Lerp(0, path, 1-Math.Min( 1, plateauFinal * 3f)), 0, path);
							}
	
					        for (var i = 0; i < noise3D.Length; i++)
					        {
					            if (World.MenuSeed != World.Seed)
					                noise3D[i] = biomeGen.GetDensity(OffsetX + x * Chunk.BlockSize, i * noiseScale * Chunk.BlockSize,
					                    OffsetZ + z * Chunk.BlockSize, heightCache);


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
                                    
					            }
					            if(nearGiantTree == null){
									river = Mathf.Clamp(Mathf.Lerp(0, river, 1-Math.Abs(noise3D[i])), 0, river);
									riverBorders = Mathf.Clamp(Mathf.Lerp(0, riverBorders, 1-Math.Abs(noise3D[i])), 0, riverBorders);
								}
					            path = Mathf.Lerp(path, 0, noise3D[i]);
							}

					        float riverLerp = amplifiedRiverBorders / riverDepth;
                            
					        if ( (riverLerp > 0 || path > 0) && !inPlateau)
					        {
                                
					            if (heightCache.ContainsKey(new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ)))
					            {
					                if (path > 0)
					                {
					                    path = Mathf.Lerp(path, 0, Mathf.Clamp(heightCache[
					                                                               new Vector2(x * Chunk.BlockSize + OffsetX,
					                                                                   z * Chunk.BlockSize + OffsetZ)][0] / 32.0f, 0,
					                        1.0f));
					                }
					                if (riverLerp > 0)
					                {
					                    float minus = Mathf.Lerp(0,
					                        heightCache[
					                            new Vector2(x * Chunk.BlockSize + OffsetX,
					                                z * Chunk.BlockSize + OffsetZ)][0], riverLerp);
					                    height -= minus;
					                }
					            }
					        }
                            

					        height -= river;
							height -= path;
					        var dirtNoise = OpenSimplexNoise.Evaluate((x * Chunk.BlockSize + OffsetX) * 0.0175f,
					            (z * Chunk.BlockSize + OffsetZ) * 0.0175f) > .35f;


					        bool makeDirt = biomeGen.HasDirt && dirtNoise;

					        var villagePath = false;
					        lock (World.QuestManager.VillagePositions)
					        {
					            foreach (var pair in World.QuestManager.VillagePositions)
					            {
					                var villageRadius = pair.Value;
					                if ((position - pair.Key.Xz).LengthSquared < villageRadius * villageRadius)
					                {
					                    villagePath = true;
					                }
					            }
					        }
                            #endregion

                            for (var y = 0; y < Chunk.Height-1; y++)
                            {

                                float noise = 0;/*Mathf.Lerp(
                                    noise3D[ (int) Math.Floor(y / (float) noiseScale) ],
                                    noise3D[ (int) Math.Min(noise3D.Length-1, Math.Floor(y / (float) noiseScale) + 1) ],
								    (y / (float) noiseScale) - (int) Math.Floor(y / (float)noiseScale));*/

                                type = biomeGen.GetHeightSubtype(position.X, y, position.Y, height, type, heightCache);

								if(noise != 0 && townClamped)
									townClamped = false;
								
								Blocks[x][y][z].Type = BlockType.Air;
								
								Blocks[x][y][z].Density = (Half) (1-(y - height) + noise);
								
								if(y < 2)
									Blocks[x][y][z].Density = (Half)(0.95f + rng.NextFloat()*0.75f);
								
								if( Blocks[x][y][z].Density > 0){
									
									Blocks[x][y][z].Type = type;

								    if (height - y > 3.0f)
								    {
								        Blocks[x][y][z].Type = BlockType.Stone;
								    }
                                    
                                    if (y < 2)
										Blocks[x][y][z].Type = BlockType.Seafloor;
									
									if(noise != 0)
										Blocks[x][y][z].Noise3D = true;
									
								}
								
								if(Blocks[x][y][z].Type == BlockType.Grass)
								    if( (World.Seed == World.MenuSeed || true) && makeDirt )
								        Blocks[x][y][z].Type = BlockType.Dirt;

                                if (y < height + river)
                                {
                                    if (Blocks[x][y][z].Type == BlockType.Air && river > 0)
                                    {
                                        Blocks[x][y][z].Type = BlockType.Water;
                                        Chunk.AddWaterDensity(new Vector3(x,y,z), (Half) (height + river));
                                    }
                                    else if (Mathf.Clamp(riverBorders * 100f, 0, riverDepth) > 2 &&
                                             Blocks[x][y][z].Type != BlockType.Air)
                                    {
                                        Blocks[x][y][z].Type = BlockType.Seafloor;
                                        for (var i = 0; i < y; i++) Blocks[x][i][z].Type = BlockType.Seafloor;
                                    }
                                }

                                if(y <= BiomePool.SeaLevel && Blocks[x][y][z].Type == BlockType.Air && y >= 1 && Blocks[x][y-1][z].Type != BlockType.Seafloor && Blocks[x][y-1][z].Type != BlockType.Air && Blocks[x][y-1][z].Type != BlockType.Water )
                                {
                                    if (y < BiomePool.SeaLevel)
                                    { 
                                        Blocks[x][y][z].Type = BlockType.Seafloor;
                                        for (var i = 0; i < y; i++)
                                        {
                                            Blocks[x][i][z].Type = BlockType.Seafloor;
                                        }
                                    }
                                }

                                var isOcean = y > 0 && y <= BiomePool.SeaLevel &&
                                              (Blocks[x][y - 1][z].Type == BlockType.Seafloor ||
                                               Blocks[x][y - 1][z].Type == BlockType.Water) &&
                                              Blocks[x][y][z].Type == BlockType.Air &&
                                              Blocks[x][y + 1][z].Type == BlockType.Air;
                                if (isOcean)
                                {
                                    Blocks[x][y][z].Type = BlockType.Water;
                                    Chunk.AddWaterDensity(new Vector3(x,y,z), (Half) BiomePool.SeaLevel);
                                }

                                if(villagePath || path == pathDepth || town){
									if( Blocks[x][y][z].Type != BlockType.Air && Blocks[x][y][z].Type != BlockType.Water && Blocks[x][y][z].Type != BlockType.Seafloor){
										if(path > 0 || pathClamped || villagePath)
											Blocks[x][y][z].Type = BlockType.Path;
										if(town && townClamped)
										    if(Blocks[x][y][z].Type == BlockType.Stone)
										        Blocks[x][y][z].Type = BlockType.Grass;
									}
								}
							}
						}
					}
					
				}
                catch (NullReferenceException e)
                {
					Log.WriteLine(e.ToString());
					
					World.RemoveChunk(Chunk);
				}
			}
		}

	    public override void PlaceStructures()
	    {
	        this.StructuresPlaced = true;

	        CollidableStructure[] structs;
	        lock (World.StructureGenerator.Items)
	            structs = World.StructureGenerator.Items.ToArray();

	        for (var x = 0; x < this.Chunk.BoundsX; x++)
	        {
	            for (var z = 0; z < this.Chunk.BoundsZ; z++)
	            {
	                int y = Chunk.GetHighestY(x, z);
	                var position = new Vector3(Chunk.OffsetX + x * Chunk.BlockSize, y-1, Chunk.OffsetZ + z * Chunk.BlockSize);

	                if (Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air
	                    && Blocks[x][y - 1][z].Type != BlockType.Water &&
	                    Blocks[x][y - 1][z].Type != BlockType.Air) Blocks[x][y][z].Type = BlockType.Air;

	                var block = Chunk.GetBlockAt(x, y, z);
	                Region region = World.BiomePool.GetRegion(position);
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
	                float radius = structPosition.Mountain.Radius;
	                if ((structPosition.Mountain.Position.Xz - possiblePosition.Xz).LengthSquared <
	                    radius * .5f * radius * .5f)
	                    InMerchant = true;
	            }
	            if (structPosition.Generated) continue;

	            if ((possiblePosition.Xz - structPosition.Position.Xz).LengthSquared < 2 * 2)
	            {
	                World.StructureGenerator.Build(possiblePosition, structPosition);
	                structPosition.Generated = true;
	            }
	        }
        }

	    public bool NoTreeZone(int X, int Z){
			return Utils.Rng.Next(0, 4) == 1;
		}
		
		public void CheckForNearbyStructures(){
			World.StructureGenerator.CheckStructures( new Vector2(Chunk.OffsetX, Chunk.OffsetZ) );
		}
	}
}