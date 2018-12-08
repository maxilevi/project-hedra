using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class GridPlacementDesigner : PlacementDesigner
    {
        private const float PathWidth = 16;
        private const float NoPathZone = .55f;
        private const float SparseZone = .60f;
        private int VillageSize { get; }
        private MarketParameters _marketPoint;
        
        public GridPlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng) : base(Root, Config, Rng)
        {
            VillageSize = Config.Size;
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
                    if (x % 2 == 0) spacingX += VillageDesign.Spacing * .5f;
                    var position = new Vector2(VillageDesign.Spacing * x, VillageDesign.Spacing * z + spacingX) - offset;

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
            }
            return design;
        }
        
        private bool SelectPlacer(PlacementPoint Point, PlacementDesign Design)
        {
            var rng = Rng.NextFloat();
            var rotationY = Physics.DirectionToEuler(-(Vector3.Zero - Point.Position).NormalizedFast()).Y;
            var rotation = Vector3.UnitY * (float) (Math.Round(rotationY / 90f) * 90f);
            var distFromCenter = CalculateDistanceFromCenter(Point.Position);
            if (distFromCenter > SparseZone)
            {
                var x = Point.GridPosition.X;
                var z = Point.GridPosition.Y;
                var spacingX = 0f;
                var offset = CalculateOffset();
                if (x % 2 == 0) spacingX += VillageDesign.Spacing * .5f;
                var mod = (1.0f + Math.Max(0, distFromCenter - SparseZone) * 2f) * VillageDesign.Spacing * (Point.Position.NormalizedFast());
                Point.Position = (new Vector2(VillageDesign.Spacing * x, VillageDesign.Spacing * z + spacingX) - offset + mod.Xz).ToVector3();
            }

            if (distFromCenter < .2f)
            {
                if (rng < .5f && BlacksmithPlacer.SpecialRequirements(Point))
                    AddBlacksmith(Point, Design, rotation);
                else if (rng < .95f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else
                    return false;
            }
            else if (distFromCenter < NoPathZone)
            {
                if (rng < .15f && BlacksmithPlacer.SpecialRequirements(Point))
                    AddBlacksmith(Point, Design, rotation);
                else if (rng < .90f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < .95f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation);
                else
                    return false;
            }
            else if (distFromCenter < NoPathZone + .15f)
            {
                if (rng < .30f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < .50f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation);
                else
                    return false;
            }
            else if (distFromCenter < 1f)
            {
                if (rng < .05f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < 0.45f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation);
                else
                    return false;
            }

            return true;
        }

        private void AddBlacksmith(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation)
        {
            var parameter = BlacksmithPlacer.Place(Point);
            parameter.Rotation = Rotation;
            Design.Blacksmith.Add(parameter);
        }

        private void AddFarm(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation)
        {
            Design.Farms.Add(FarmPlacer.Place(Point));
        }

        private void AddHouse(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation, float Distance)
        {
            var parameter = HousePlacer.Place(Point);
            parameter.Rotation = Rotation;
            parameter.Type = Distance > NoPathZone ? BlockType.StonePath : parameter.Type;
            parameter.GroundworkType = Distance > SparseZone ? GroundworkType.Rounded : GroundworkType.Squared;
            Design.Houses.Add(parameter);    
        }

        private float CalculateDistanceFromCenter(Vector3 Position)
        {
            return Position.LengthFast / (VillageDesign.Spacing * VillageSize * .5f);
        }
        
        private Vector2 CalculateOffset()
        {
            return VillageDesign.Spacing * VillageSize * Vector2.One * .5f;
        }

        public override void FinishPlacements(CollidableStructure Structure, PlacementDesign Design)
        {
            void AddGroundwork(Vector2 From, Vector2 To, float DistanceFromCenter)
            {
                var path = new LineGroundwork(From, To, SelectPathType(DistanceFromCenter))
                {
                    BonusHeight = -.4f,
                    Width = PathWidth
                };
                Structure.AddGroundwork(path);
            }

            for (var x = 1; x < VillageSize; ++x)
            {
                for (var z = 1; z < VillageSize; ++z)
                {
                    const float spacing = VillageDesign.Spacing;
                    var addX = 0f;
                    if (x % 2 != 0) addX -= VillageDesign.Spacing * .5f;
                    var offset = CalculateOffset();
                    var startPosition = new Vector2(VillageDesign.Spacing * x + spacing *.5f, VillageDesign.Spacing * z + spacing * .5f + addX) - offset;
                    var distFromCenter = 
                        startPosition.LengthFast / (VillageDesign.Spacing * VillageSize * .5f);
                    if(distFromCenter > NoPathZone) continue;
                    
                    AddGroundwork(
                        startPosition + Design.Position.Xz,
                        startPosition + new Vector2(0, spacing) + Design.Position.Xz,
                        distFromCenter
                    );

                    AddGroundwork(
                        startPosition + new Vector2(spacing, 0) + Design.Position.Xz,
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
        }

        private BlockType SelectPathType(float DistanceFromCenter)
        {
            if (DistanceFromCenter < NoPathZone)
                return BlockType.StonePath;
            return BlockType.None;
        }
    }
}