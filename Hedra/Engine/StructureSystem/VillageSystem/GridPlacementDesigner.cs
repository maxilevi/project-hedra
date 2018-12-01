using System;
using System.Collections.Generic;
using System.Linq;
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
        private int VillageSize { get; }
        private readonly List<PlacementPoint> _pointsWithBuildings;
        
        public GridPlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng) : base(Root, Config, Rng)
        {
            VillageSize = Config.Size;
            _pointsWithBuildings = new List<PlacementPoint>();
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
            var marketPoint = design.Markets[0];
            var points = new List<PlacementPoint>();
            for (var x = 1; x < VillageSize; ++x)
            {
                for (var z = 1; z < VillageSize; ++z)
                {
                    if( (new Vector2(x,z) - new Vector2(VillageSize, VillageSize) * .5f).LengthSquared > VillageSize * .5f * .5f * VillageSize)
                        continue;
                    var spacing = CalculateSpacing(x, z);
                    var offset = new Vector3(VillageSize * spacing * .5f, 0, VillageSize * spacing * .5f);
                    var position = new Vector3(x * spacing, 0, z * spacing) - offset;
                    var size = marketPoint.Size * 1.0f;
                    if ((position - marketPoint.Position).LengthSquared > size * size)
                    {
                        points.Add(new PlacementPoint
                        {
                            Position = position,
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
            if ((int)X == VillageSize/2 - 1 && (int)Z == VillageSize/2-1) return VillageDesign.Spacing * 5;
            return (float) Math.Max(
                Math.Pow(
                   VillageDesign.Spacing * 
                        (1.0f + (new Vector2(X, Z) - new Vector2(VillageSize, VillageSize) * .5f).LengthFast / (VillageSize)),
                .975f),
                VillageDesign.Spacing
            );
        }

        private bool SelectPlacer(PlacementPoint Point, PlacementDesign Design)
        {
            var rng = Rng.NextFloat();
            var rotationY = Physics.DirectionToEuler(-(Vector3.Zero - Point.Position).NormalizedFast()).Y;
            var rotation = Vector3.UnitY * (float) (Math.Round(rotationY / 90f) * 90f);
            var distFromCenter = 
                (Point.GridPosition - new Vector2(VillageSize, VillageSize) * .5f).LengthFast / (VillageSize);
            
            if (rng < .1f && FarmPlacer.SpecialRequirements(Point))
            {
                Design.Farms.Add(FarmPlacer.Place(Point));
            }
            else if (rng < .2f && BlacksmithPlacer.SpecialRequirements(Point))
            {
                var parameter = BlacksmithPlacer.Place(Point);
                parameter.Rotation = rotation;
                Design.Blacksmith.Add(parameter);
            }
            else if (rng < .85f && HousePlacer.SpecialRequirements(Point))
            {
                var parameter = HousePlacer.Place(Point);
                parameter.Rotation = rotation;
                Design.Houses.Add(parameter);
            }
            else
            {
                return false;
            }

            return true;
        }
        

        public override void FinishPlacements(CollidableStructure Structure, PlacementDesign Design)
        {
            void AddGroundwork(Vector2 From, Vector2 To)
            {
                var path = new LineGroundwork(From, To, BlockType.StonePath)
                {
                    BonusHeight = -.25f,
                    Width = 20
                };
                Structure.AddGroundwork(path);
            }

            for (var x = 1; x < VillageSize; ++x)
            {
                for (var z = 1; z < VillageSize; ++z)
                {
                    var squaredDistFromCenter =
                        (new Vector2(x, z) - new Vector2(VillageSize-2, VillageSize-2) * .5f).LengthSquared;
                    //if(squaredDistFromCenter > VillageSize * .5f * VillageSize * .5f)
                    //    continue;
                    //if(squaredDistFromCenter <= 1)
                    //    continue;
                    
                    var spacing = CalculateSpacing(x, z);
                    var offset = new Vector2(VillageSize * spacing * .5f, VillageSize * spacing * .5f);
                    var startPosition = new Vector2(x * spacing + spacing * .5f, z * spacing + spacing * .5f) - offset;
                    AddGroundwork(
                        startPosition + Design.Position.Xz,
                        startPosition + new Vector2(0, spacing) + Design.Position.Xz
                    );

                    AddGroundwork(
                        startPosition + new Vector2(spacing, 0) + Design.Position.Xz,
                        startPosition + Design.Position.Xz
                    );
                }
            }

            /*for (var i = 0; i < _pointsWithBuildings.Count; ++i)
            {
                var point = _pointsWithBuildings[i];
                var spacing = CalculateSpacing(point.GridPosition.X, point.GridPosition.Y) + 24;
                AddGroundwork(
                    new Vector2(point.Position.X + spacing * .5f, point.Position.Z) + Design.Position.Xz,
                    new Vector2(point.Position.X + spacing * .5f, point.Position.Z + spacing) + Design.Position.Xz
                );
                AddGroundwork(
                    new Vector2(point.Position.X, point.Position.Z + spacing * .5f) + Design.Position.Xz,
                    new Vector2(point.Position.X + spacing, point.Position.Z + spacing * .5f) + Design.Position.Xz
                );
                /* Negative */
                /*AddGroundwork(
                    new Vector2(point.Position.X - spacing * .5f, point.Position.Z) + Design.Position.Xz,
                    new Vector2(point.Position.X - spacing * .5f, point.Position.Z - spacing) + Design.Position.Xz
                );
                AddGroundwork(
                    new Vector2(point.Position.X, point.Position.Z - spacing * .5f) + Design.Position.Xz,
                    new Vector2(point.Position.X - spacing, point.Position.Z - spacing * .5f) + Design.Position.Xz
                );
            }*/
        }
    }
}