using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Placers;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class GridPlacementDesigner : PlacementDesigner
    {
        private const float PathWidth = 16;
        private const float NoPathZone = .55f;
        private int VillageSize { get; }
        private readonly List<PlacementPoint> _pointsWithBuildings;
        private readonly List<PlacementPoint> _pointsWithStone;
        private MarketParameters _marketPoint;
        
        public GridPlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng) : base(Root, Config, Rng)
        {
            VillageSize = Config.Size;
            _pointsWithBuildings = new List<PlacementPoint>();
            _pointsWithStone = new List<PlacementPoint>();
        }
        
        public override PlacementDesign CreateDesign()
        {
            var design = new PlacementDesign
            {
                Markets = new List<MarketParameters>
                {
                    MarketPlacer.Place(new PlacementPoint())
                }
            };
            _marketPoint = design.Markets[0];
            var points = new List<PlacementPoint>();
            for (var x = 1; x < VillageSize; ++x)
            {
                for (var z = 1; z < VillageSize; ++z)
                {
                    if( (new Vector2(x,z) - new Vector2(VillageSize, VillageSize) * .5f).LengthSquared > VillageSize * .5f * .5f * VillageSize)
                        continue;
                    var size = _marketPoint.Size * 1.5f;
                    var offset = CalculateOffset();
                    var spacingX = 0f;
                    if (x % 2 == 0) spacingX += CalculateSpacing(x, 0) * .5f;
                    var position = new Vector2(AccumulativeSpacing(x), AccumulativeSpacing(z) + spacingX) - offset;
                    if ((position - _marketPoint.Position.Xz).LengthSquared > size * size)
                    {
                        points.Add(new PlacementPoint
                        {
                            Position = position.ToVector3(),
                            GridPosition = new Vector2(x, z),
                            Radius = 0
                        });
                    }
                }
            }

            for (var i = 0; i < points.Count; i++)
            {
                var successful = this.SelectPlacer(points[i], design);
                if(successful) _pointsWithBuildings.Add(points[i]);
            }
            return design;
        }

        private float CalculateSpacing(float X, float Z)
        {
            var distanceFromCenter = (new Vector2(X, Z) - new Vector2(VillageSize, VillageSize) * .5f).LengthFast /
                                     VillageSize;

            return VillageDesign.Spacing * (1.0f) //+ (float)Math.Pow(distanceFromCenter, 4.0f) * 4f)
                   + ((int) (X % 2) == 0 ? 0 : 0f);
        }

        private bool SelectPlacer(PlacementPoint Point, PlacementDesign Design)
        {
            var rng = Rng.NextFloat();
            var rotationY = Physics.DirectionToEuler(-(Vector3.Zero - Point.Position).NormalizedFast()).Y;
            var rotation = Vector3.UnitY * (float) (Math.Round(rotationY / 90f) * 90f);
            var distFromCenter = CalculateDistanceFromCenter(Point);

            if (distFromCenter < .2f)
            {
                if (rng < .5f && BlacksmithPlacer.SpecialRequirements(Point))
                    AddBlacksmith(Point, Design, rotation, distFromCenter);
                else if (rng < .95f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else
                    return false;
            }
            else if (distFromCenter < .75f)
            {
                if (rng < .15f && BlacksmithPlacer.SpecialRequirements(Point))
                    AddBlacksmith(Point, Design, rotation, distFromCenter);
                else if (rng < .85f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < .90f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation);
                else
                    return false;
            }
            else if (distFromCenter < .75f)
            {
                if (rng < .70f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < .90f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation);
                else
                    return false;
            }
            else if (distFromCenter < 1f && distFromCenter > .75f)
            {
                var chance = distFromCenter < .85f ? .4f : .5f;
                if (rng < chance && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation);
                else
                    return false;
            }

            return true;
        }

        private void AddBlacksmith(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation, float Distance)
        {
            var parameter = BlacksmithPlacer.Place(Point);
            parameter.Rotation = Rotation;
            Design.Blacksmith.Add(parameter);
            if(Distance > NoPathZone)
                _pointsWithStone.Add(Point);
        }

        private void AddFarm(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation)
        {
            Design.Farms.Add(FarmPlacer.Place(Point));
        }

        private void AddHouse(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation, float Distance)
        {
            var parameter = HousePlacer.Place(Point);
            parameter.Rotation = Rotation;
            Design.Houses.Add(parameter);
            if(Distance > NoPathZone)
                _pointsWithStone.Add(Point);
                
        }

        private float CalculateDistanceFromCenter(PlacementPoint Point)
        {
            return (Point.Position).LengthFast / (AccumulativeSpacing(VillageSize) * .5f);
        }
        
        private Vector2 CalculateOffset()
        {
            return AccumulativeSpacing(VillageSize) * Vector2.One * .5f;
        }

        public override void FinishPlacements(CollidableStructure Structure, PlacementDesign Design)
        {
            void AddGroundwork(Vector2 From, Vector2 To, float DistanceFromCenter)
            {
                var path = new LineGroundwork(From, To, SelectPathType(DistanceFromCenter))
                {
                    BonusHeight = -.25f * Math.Max(.75f - DistanceFromCenter, 0.0f),
                    Width = PathWidth
                };
                Structure.AddGroundwork(path);
            }

            for (var x = 1; x < VillageSize; ++x)
            {
                for (var z = 1; z < VillageSize; ++z)
                {
                    var spacingX = CalculateSpacing(x,0);
                    var spacingZ = CalculateSpacing(0,z);
                    var addX = 0f;
                    if (x % 2 != 0) addX -= CalculateSpacing(x, 0) * .5f;
                    var offset = CalculateOffset();
                    var startPosition = new Vector2(AccumulativeSpacing(x) + spacingX *.5f, AccumulativeSpacing(z) + spacingZ * .5f + addX) - offset;
                    var distFromCenter = 
                        startPosition.LengthFast / (AccumulativeSpacing(VillageSize) * .5f);
                    if(distFromCenter > NoPathZone) continue;
                    
                    AddGroundwork(
                        startPosition + Design.Position.Xz,
                        startPosition + new Vector2(0, spacingZ) + Design.Position.Xz,
                        distFromCenter
                    );

                    AddGroundwork(
                        startPosition + new Vector2(spacingX, 0) + Design.Position.Xz,
                        startPosition + Design.Position.Xz,
                        distFromCenter
                    );
                }
            }
            Structure.AddGroundwork(new RoundedGroundwork(Design.Position, _marketPoint.Size, BlockType.StonePath)
            {
                BonusHeight = .25f,
            });
            
            Structure.AddGroundwork(new RoundedGroundwork(Design.Position, _marketPoint.WellSize, BlockType.Path)
            {
                BonusHeight = .0f,
            });

            for (var i = 0; i < _pointsWithStone.Count; i++)
            {/*
                Structure.AddGroundwork(new RoundedGroundwork(_pointsWithStone[i].Position, _pointsWithStone[i].Radius, BlockType.StonePath)
                {
                    BonusHeight = .25f,
                });*/
            }
        }

        private BlockType SelectPathType(float DistanceFromCenter)
        {
            if (DistanceFromCenter < NoPathZone)
                return BlockType.StonePath;
            return BlockType.None;
        }

        private float AccumulativeSpacing(int I)
        {
            var accum = 0f;
            for(; I > 0; I--)
            {
                accum += CalculateSpacing(I, 0);
            }
            return accum;
        } 
    }
}