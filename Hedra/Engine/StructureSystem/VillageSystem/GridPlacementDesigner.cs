using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Placers;
using Hedra.Engine.WorldBuilding;
using System.Numerics;
using Hedra.Numerics;
using Hedra.Framework;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class GridPlacementDesigner : PlacementDesigner
    {
        public const float NoPathZone = .55f;
        public const float SparseZone = .60f;
        private IBuildingParameters[] _specialPoints;
        private MarketParameters _marketPoint;
        private int VillageSize { get; }
        
        public GridPlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng) : base(Root, Config, Rng)
        {
            VillageSize = Config.Size;
        }
        
        public override PlacementDesign CreateDesign()
        {
            var design = new PlacementDesign();
            _specialPoints = AddSpecialPoints(design);
            var points = new List<PlacementPoint>();
            for (var x = 1; x < VillageSize; ++x)
            {
                for (var z = 1; z < VillageSize; ++z)
                {
                    if( (new Vector2(x,z) - new Vector2(VillageSize, VillageSize) * .5f).LengthSquared() > VillageSize * .5f * .5f * VillageSize)
                        continue;
                    var offset = CalculateOffset();
                    var spacingX = 0f;
                    if (x % 2 == 0) spacingX += VillageDesign.Spacing * .5f;
                    var position = new Vector2(VillageDesign.Spacing * x, VillageDesign.Spacing * z + spacingX) - offset;

                    if (DoesNotIntersectWithSpecialBuildings(_specialPoints, position))
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

            _marketPoint = (MarketParameters) _specialPoints[0];
            points = points.OrderBy(P => (P.Position.Xz() - _marketPoint.Position.Xz()).LengthFast()).ToList();
            for (var i = 0; i < points.Count; i++)
            {
                var successful = SelectPlacer(points[i], design);
            }
            return design;
        }

        private bool DoesNotIntersectWithSpecialBuildings(IBuildingParameters[] SpecialPoints, Vector2 Position)
        {
            return SpecialPoints.All(S => (Position - S.Position.Xz()).LengthSquared() > Math.Pow(S.GetSize(Root.Cache) * 1.5f, 2));
        }

        private IBuildingParameters[] AddSpecialPoints(PlacementDesign Design)
        {
            // Add special buildings like inns and mayors
            var genericPlacers = new []
            {
                new Pair<IPlacer<IBuildingParameters>, Action<IBuildingParameters>>(MayorPlacer, P => Design.Generics.Add((GenericParameters)P)),
                new Pair<IPlacer<IBuildingParameters>, Action<IBuildingParameters>>(ShopPlacer, P => Design.Generics.Add((GenericParameters)P)),
                new Pair<IPlacer<IBuildingParameters>, Action<IBuildingParameters>>(ClothierPlacer, P => Design.Generics.Add((GenericParameters)P)),
                new Pair<IPlacer<IBuildingParameters>, Action<IBuildingParameters>>(MasonryPlacer, P => Design.Generics.Add((GenericParameters)P)),
                new Pair<IPlacer<IBuildingParameters>, Action<IBuildingParameters>>(InnPlacer, P => Design.Generics.Add((GenericParameters)P)),
                //new Pair<IPlacer<IBuildingParameters>, Action<IBuildingParameters>>(BlacksmithPlacer, P => Design.Blacksmith.Add((BlacksmithParameters)P))
            };
            genericPlacers.Shuffle(Rng);
            Design.Markets.Add(MarketPlacer.Place(new PlacementPoint()));
            var points = 5;
            for (var i = 0; i < points; ++i)
            {
                var randomPosition = new Vector2(VillageDesign.Spacing * Rng.NextFloat() * VillageSize, VillageDesign.Spacing * Rng.NextFloat() * VillageSize) * .25f;
                var position = (randomPosition * new Vector2(Rng.Next(0, 2) * 2 - 1, Rng.Next(0, 2) * 2 - 1));
                if(!DoesNotIntersectWithSpecialBuildings(Design.Parameters, position)) continue;
                var point = new PlacementPoint
                {
                    Position = position.ToVector3()
                };
                TryAddAny(genericPlacers, point);
            }
            return Design.Parameters;
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
                if ((int)x % 2 == 0) spacingX += VillageDesign.Spacing * .5f;
                var mod = (1.0f + Math.Max(0, distFromCenter - SparseZone) * 2f) * VillageDesign.Spacing * (Point.Position.NormalizedFast());
                Point.Position = (new Vector2(VillageDesign.Spacing * x, VillageDesign.Spacing * z + spacingX) - offset + mod.Xz()).ToVector3();
            }
            
            if (distFromCenter < .2f)
            {
                if (rng < .15f && BlacksmithPlacer.SpecialRequirements(Point))
                    AddBlacksmith(Point, Design, rotation);
                else if (rng < .95f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else
                    return false;
            }
            else if (distFromCenter < NoPathZone)
            {
                if (rng < .1f && BlacksmithPlacer.SpecialRequirements(Point))
                    AddBlacksmith(Point, Design, rotation);
                else if (rng < .90f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < .95f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation, distFromCenter);
                else
                    return false;
            }
            else if (distFromCenter < NoPathZone + .15f)
            {
                if (rng < .30f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < .50f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation, distFromCenter);
                else
                    return false;
            }
            else if (distFromCenter < 1f)
            {
                if (rng < .05f && HousePlacer.SpecialRequirements(Point))
                    AddHouse(Point, Design, rotation, distFromCenter);
                else if (rng < 0.45f && FarmPlacer.SpecialRequirements(Point))
                    AddFarm(Point, Design, rotation, distFromCenter);
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

        private static void TryAddAny(Pair<IPlacer<IBuildingParameters>, Action<IBuildingParameters>>[] Pairs, PlacementPoint Point)
        {
            for (var i = 0; i < Pairs.Length; ++i)
            {
                if (Pairs[i].One.SpecialRequirements(Point))
                {
                    var parameters = Pairs[i].One.Place(Point);
                    Pairs[i].Two(parameters);
                    break;
                }
            }
        }

        private void AddMayor(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation)
        {
            var parameter = MayorPlacer.Place(Point);
            parameter.Rotation = Rotation;
            Design.Generics.Add(parameter);
        }
        
        private void AddFarm(PlacementPoint Point, PlacementDesign Design, Vector3 Rotation, float Distance)
        {
            var parameter = FarmPlacer.Place(Point);
            parameter.InsidePaths = Distance <= (NoPathZone+.05f);
            Design.Farms.Add(parameter);
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
            return Position.LengthFast() / (VillageDesign.Spacing * VillageSize * .5f);
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
                    Width = VillageDesign.PathWidth
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
                        startPosition.LengthFast() / (VillageDesign.Spacing * VillageSize * .5f);
                    
                    if(distFromCenter > NoPathZone) continue;
                    
                    var start = startPosition + Design.Position.Xz();
                    var halfStartX = start + new Vector2(spacing, 0) * .5f;
                    var endX = start + new Vector2(spacing, 0);
                    var halfStartZ = start + new Vector2(0, spacing) * .5f;
                    var endZ = start + new Vector2(0, spacing);
                    Design.Graph.AddEdge(start, halfStartX);
                    Design.Graph.AddEdge(halfStartX, endX);
                    Design.Graph.AddEdge(start, halfStartZ);
                    Design.Graph.AddEdge(halfStartZ, endZ);

                    AddGroundwork(
                        start,
                        start + new Vector2(0, spacing),
                        distFromCenter
                    );

                    AddGroundwork(
                        start + new Vector2(spacing, 0),
                        start,
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
            /* We start at one because we already did the market one. */
            for (var i = 1; i < _specialPoints.Length; ++i)
            {
                Structure.AddGroundwork(new RoundedGroundwork(_specialPoints[i].Position, _specialPoints[i].GetSize(Root.Cache), BlockType.StonePath)
                {
                    BonusHeight = .25f,
                });
            }
            PlaceDecorations(Structure, Design);
        }        


        protected Vector2 GetRealSize(IBuildingParameters Parameters)
        {
            if (Parameters.Design != null)
                return Root.Cache.GrabSize(Parameters.Design.Path).Xz();
            else
                return VillageDesign.Spacing * Vector2.One;
        }

        private void PlaceDecorations(CollidableStructure Structure, PlacementDesign Design)
        {
            var vertices = Design.Graph.Vertices;
            for (var i = 0; i < vertices.Length; ++i)
            {
                /* Unless we can guarantee the terrain is flat don't place anything */
                if(Structure.Mountain.Density(vertices[i]) < 1) continue;            
                if((vertices[i] - Design.Position.Xz()).LengthSquared() < Math.Pow(_marketPoint.Size, 2)) continue;
                
                if (Rng.Next(0, 5) == 1)
                {
                    var inIntersection = Design.Graph.Degree(i) == 3;
                    var edges = Design.Graph.GetEdgesWithVertex(vertices[i]);
                    var possibleDirections = new List<Vector2>(new[]
                    {
                        Vector2.UnitX,
                        -Vector2.UnitX,
                        Vector2.UnitY,
                        -Vector2.UnitY
                    });
                    var k = i;
                    Vector2 EdgeDirection(GraphEdge E) => (vertices[E.GetOtherVertex(k)] - vertices[k]).Normalized();
                    edges.ToList().ForEach(E => possibleDirections.Remove(EdgeDirection(E)));
                    var orientation = possibleDirections.Select(V => V.ToVector3()).ToList().Random(Rng);
                    DecorationsPlacer.PlaceBench(vertices[i].ToVector3(), inIntersection, orientation, Structure, Root, Rng);
                }
            }
        }

        private BlockType SelectPathType(float DistanceFromCenter)
        {
            if (DistanceFromCenter < NoPathZone)
                return BlockType.StonePath;
            return BlockType.None;
        }
    }
}