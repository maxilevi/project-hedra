/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/07/2016
 * Time: 12:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Localization;

namespace Hedra.Engine.WorldBuilding
{
    public class WorldBuilding : IWorldBuilding
    {
        private readonly List<IGroundwork> _groundwork;
        private readonly object _groundworkLock = new ();
        private readonly object _plateauLock = new ();
        private readonly object _landformsLock = new ();
        private readonly List<BasePlateau> _plateaus;
        private readonly List<Landform> _landforms;

        public WorldBuilding()
        {
            _groundwork = new List<IGroundwork>();
            _plateaus = new List<BasePlateau>();
            _landforms = new List<Landform>();
        }

        public Chest SpawnChest(Vector3 Position, Item Item)
        {
            return new Chest(Position, Item);
        }

        public string GenerateName()
        {
            var rng = new Random(World.Seed);
            var types = new[]
            {
                "mountains_of", "lands_of", "territory_of"
            };
            return Translations.Get(types[rng.Next(0, types.Length)], NameGenerator.Generate(World.Seed));
        }

        public float ApplyMultiple(Vector2 Position, float MaxHeight, params BasePlateau[] Against)
        {
            var plateaus = Against.OrderByDescending(P => P.MaxHeight).ToList();
            for (var i = 0; i < plateaus.Count; i++) MaxHeight = plateaus[i].Apply(Position, MaxHeight, out _);
            return MaxHeight;
        }

        public float ApplyMultiple(Vector2 Position, float MaxHeight)
        {
            lock (_plateauLock)
            {
                return ApplyMultiple(Position, MaxHeight, _plateaus.ToArray());
            }
        }

        public IEnumerable<Landform> GetLandformsFor(Vector2 Position)
        {
            lock (_landformsLock)
            {
                var chunkSpace = World.ToChunkSpace(Position);
                return _landforms.Where(L =>
                {
                    return L.HasPoint(chunkSpace, out _)
                           || L.HasPoint(chunkSpace + new Vector2(Chunk.Width, 0), out _)
                           || L.HasPoint(chunkSpace + new Vector2(0, Chunk.Width), out _)
                           || L.HasPoint(chunkSpace + new Vector2(Chunk.Width, Chunk.Width), out _);
                }).ToList();
            }
        }
        
        public List<Landform> Landforms
        {
            get
            {
                lock (_landformsLock)
                    return _landforms.ToList();
            }
        }

        public BasePlateau[] GetPlateausFor(Vector2 Position)
        {
            lock (_plateauLock)
            {
                var chunkSpace = World.ToChunkSpace(Position);
                var list = new List<BasePlateau>();
                for (var i = 0; i < _plateaus.Count; ++i)
                {
                    var squared = _plateaus[i].ToBoundingBox();
                    if (
                        squared.Collides(chunkSpace)
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, 0))
                        || squared.Collides(chunkSpace + new Vector2(0, Chunk.Width))
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, Chunk.Width))
                        || World.ToChunkSpace(squared.BackCorner) == chunkSpace
                        || World.ToChunkSpace(squared.FrontCorner) == chunkSpace
                        || World.ToChunkSpace(squared.RightCorner) == chunkSpace
                        || World.ToChunkSpace(squared.LeftCorner) == chunkSpace
                    )
                        list.Add(_plateaus[i]);
                }

                return list.ToArray();
            }
        }

        public IGroundwork[] GetGroundworksFor(Vector2 Position)
        {
            lock (_groundworkLock)
            {
                var chunkSpace = World.ToChunkSpace(Position);
                var list = new List<IGroundwork>();
                for (var i = 0; i < _groundwork.Count; ++i)
                {
                    var squared = _groundwork[i].ToBoundingBox();
                    if (
                        squared.Collides(chunkSpace)
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, 0))
                        || squared.Collides(chunkSpace + new Vector2(0, Chunk.Width))
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, Chunk.Width))
                        || World.ToChunkSpace(squared.BackCorner) == chunkSpace
                        || World.ToChunkSpace(squared.FrontCorner) == chunkSpace
                        || World.ToChunkSpace(squared.RightCorner) == chunkSpace
                        || World.ToChunkSpace(squared.LeftCorner) == chunkSpace
                    )
                        list.Add(_groundwork[i]);
                }

                return list.ToArray();
            }
        }

        public BasePlateau[] Plateaux
        {
            get
            {
                lock (_plateauLock)
                {
                    return _plateaus.ToArray();
                }
            }
        }

        public IGroundwork[] Groundworks
        {
            get
            {
                lock (_groundworkLock)
                {
                    return _groundwork.ToArray();
                }
            }
        }

        public void SetupStructure(CollidableStructure Structure)
        {
            LoopStructure(Structure, AddPlateau, AddGroundwork, false);
            Structure.Reposition();
        }

        public void DisposeStructure(CollidableStructure Structure)
        {
            LoopStructure(Structure, RemovePlateau, RemoveGroundwork, true);
        }

        public void RemoveLandform(Landform Land)
        {
            lock (_landformsLock)
            {
                _landforms.Remove(Land);
            }
        }

        public void AddLandform(Landform Land)
        {
            lock (_landformsLock)
            {
                /* This is here so that houses get correctly positioned on mountains */
                _landforms.Add(Land);
            }
        }


        private void RemovePlateau(BasePlateau Mount)
        {
            lock (_plateauLock)
            {
                _plateaus.Remove(Mount);
            }
        }

        private void RemoveGroundwork(IGroundwork Work)
        {
            lock (_groundworkLock)
            {
                _groundwork.Remove(Work);
            }
        }

        private void AddPlateau(BasePlateau Mount)
        {
            lock (_plateauLock)
            {
                /* This is here so that houses get correctly positioned on mountains */
                Mount.MaxHeight = ApplyMultiple(Mount.Position, Mount.MaxHeight);
                _plateaus.Add(Mount);
                AvoidFloatingStructures(Mount);
            }
        }

        private void AddGroundwork(IGroundwork Work)
        {
            lock (_groundworkLock)
            {
                _groundwork.Add(Work);
            }
        }

        private static void LoopStructure(CollidableStructure Structure, Action<BasePlateau> PlateauDo,
            Action<IGroundwork> GroundworkDo, bool IsRemoving)
        {
            //if(World.StructureHandler.Has(Structure) && !IsRemoving)
            //    throw new ArgumentOutOfRangeException("A structure first needs to be added via the structure handler.");

            if (Structure.Mountain != null)
                PlateauDo(Structure.Mountain);

            var plateaus = Structure.Plateaux.OrderByDescending(P => P.MaxHeight).ToArray();
            for (var i = 0; i < plateaus.Length; i++)
                PlateauDo(plateaus[i]);

            var groundworks = Structure.Groundworks;
            for (var i = 0; i < groundworks.Length; i++)
                GroundworkDo(groundworks[i]);
        }

        private void AvoidFloatingStructures(params BasePlateau[] Plateaus)
        {
            lock (_plateauLock)
            {
                for (var i = 0; i < _plateaus.Count; ++i)
                for (var j = 0; j < Plateaus.Length; ++j)
                    if (Plateaus[j] is RoundedPlateau rounded0 && _plateaus[i] is RoundedPlateau rounded1)
                        if (Math.Abs(rounded0.MaxHeight - rounded1.MaxHeight) > 0.005f && rounded0.Collides(rounded1))
                            /* rounded0 is the newest plateau, we always clamp to the existing one to avoid issues */
                            rounded0.MaxHeight = rounded1.MaxHeight;
            }
        }
    }
}