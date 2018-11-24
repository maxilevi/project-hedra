using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
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
            var offset = new Vector3(VillageSize * VillageDesign.Spacing * .5f, 0, VillageSize * VillageDesign.Spacing * .5f);
            for (var x = 0; x < VillageSize; ++x)
            {
                for (var z = 0; z < VillageSize; ++z)
                {
                    var position = new Vector3(x * VillageDesign.Spacing, 0, z * VillageDesign.Spacing) - offset;
                    var size = marketPoint.Size * 1.5f;
                    if ((position - marketPoint.Position).LengthSquared > size * size)
                    {
                        points.Add(new PlacementPoint
                        {
                            Position = position,
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

        private bool SelectPlacer(PlacementPoint Point, PlacementDesign Design)
        {
            var rng = Rng.NextFloat();
            var rotationY = Physics.DirectionToEuler((Vector3.Zero - Point.Position).NormalizedFast()).Y;
            var rotation = Vector3.UnitY * (float) (Math.Round(rotationY / 90f) * 90f);
            
            if (rng < .15 && FarmPlacer.SpecialRequirements(Point))
            {
                Design.Farms.Add(FarmPlacer.Place(Point));
            }
            else if (rng < .25 && BlacksmithPlacer.SpecialRequirements(Point))
            {
                var parameter = BlacksmithPlacer.Place(Point);
                parameter.Rotation = rotation;
                Design.Blacksmith.Add(parameter);
            }
            else if (rng < .75 && HousePlacer.SpecialRequirements(Point))
            {
                var parameter = HousePlacer.Place(Point);
                parameter.Rotation = rotation;
                Design.Neighbourhoods.Add(parameter);
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

            for (var i = 0; i < _pointsWithBuildings.Count; ++i)
            {
                var point = _pointsWithBuildings[i];
                AddGroundwork(
                    new Vector2(point.Position.X + VillageDesign.Spacing * .5f, point.Position.Z) + Design.Position.Xz,
                    new Vector2(point.Position.X + VillageDesign.Spacing * .5f, point.Position.Z + VillageDesign.Spacing) + Design.Position.Xz
                );
                AddGroundwork(
                    new Vector2(point.Position.X, point.Position.Z + VillageDesign.Spacing * .5f) + Design.Position.Xz,
                    new Vector2(point.Position.X + VillageDesign.Spacing, point.Position.Z + VillageDesign.Spacing * .5f) + Design.Position.Xz
                );
            }
        }
    }
}