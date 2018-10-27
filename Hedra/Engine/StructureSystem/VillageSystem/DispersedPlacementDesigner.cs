using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Layout;
using Hedra.Engine.StructureSystem.VillageSystem.Placers;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public sealed class DispersedPlacementDesigner : IDispersedPlacementDesigner
    {
        private readonly VillageRoot _root;
        private readonly Random _rng;
        private readonly VillageConfiguration _config;
        private readonly FarmPlacer _farmPlacer;
        private readonly BlacksmithPlacer _blacksmithPlacer;
        private readonly NeighbourhoodPlacer _neighbourhoodPlacer;
        private readonly Placer<BuildingParameters> _stablePlacer;
        private readonly MarketPlacer _marketPlacer;

        public DispersedPlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng)
        {
            this._root = Root;
            this._rng = Rng;
            this._config = Config;
            this._farmPlacer = new FarmPlacer(Root.Template.Farm.Designs, Root.Template.Windmill.Designs, Rng);
            this._blacksmithPlacer = new BlacksmithPlacer(Root.Template.Blacksmith.Designs, Rng);
            this._neighbourhoodPlacer = new NeighbourhoodPlacer(Root.Template.House.Designs, Root.Template.Well.Designs, Rng);
            this._stablePlacer = new Placer<BuildingParameters>(Root.Template.Stable.Designs, Rng);
            this._marketPlacer = new MarketPlacer(Root.Template.Well.Designs, Rng);
        }
        
        public PlacementDesign CreateDesign()
        {
            var design = new PlacementDesign();
            var ring = VillageRing.Default;
            var points = SamplePoints(ring);
            var sortedPoints = SortPoints(ring, points);
            var configs = new[] { _config.InnerRing, _config.MiddleRing, _config.OuterRing };
            for (var i = 0; i < configs.Length; i++)
            {
                design.Markets = design.Markets.Concat(this._marketPlacer.Place(sortedPoints[i], configs[i].MarketChances)).ToArray();
                design.Stables = design.Stables.Concat(this._stablePlacer.Place(sortedPoints[i], configs[i].StableChances)).ToArray();
                design.Blacksmith = design.Blacksmith.Concat(this._blacksmithPlacer.Place(sortedPoints[i], configs[i].BlacksmithChances)).ToArray();
                design.Neighbourhoods = design.Neighbourhoods.Concat(this._neighbourhoodPlacer.Place(sortedPoints[i], configs[i].HouseChances)).ToArray();
                design.Farms = design.Farms.Concat(this._farmPlacer.Place(sortedPoints[i], configs[i].FarmChances)).ToArray();                
            }
            this.RemoveIntersecting(design);
            return design;
        }

        public void FinishPlacements(PlacementDesign Design)
        {
            var candidates = Design.Blacksmith.Concat<IBuildingParameters>(Design.Neighbourhoods).Concat(Design.Farms).Concat(Design.Markets).ToList();
            var graph = this.CreateGraph(Design, candidates, out var vertices);
            this.BuildPaths(Design, graph);
            this.MarkGraphSpots(graph, candidates, vertices);
        }

        private void MarkGraphSpots(PathGraph Graph, List<IBuildingParameters> Candidates, Dictionary<IBuildingParameters, PathVertex> Vertices)
        {
            for (var i = 0; i < Candidates.Count; i++)
            {
                var degree = Graph.Degree(Vertices[Candidates[i]]);
                var newParam = this.ParameterFromDegree(degree, Candidates[i].Position);
                if (degree >= 3)
                {
                    World.WorldBuilding.AddPlateau(new Plateau(Candidates[i].Position, degree * 32)
                    {
                        NoTrees = true
                    });
                }
            }
        }

        private void BuildPaths(PlacementDesign Design, PathGraph Graph)
        {
            var edges = Graph.Edges;
            for (var i = 0; i < edges.Length; i++)
            {
                var from = edges[i].Origin;
                var to = edges[i].End;
                var path = new LineGroundwork(from.Point.Xz, to.Point.Xz, BlockType.Path)
                {
                    BonusHeight = -1f,
                    Width = 24
                };
                World.WorldBuilding.AddGroundwork(path);
            }
        }

        private PathGraph CreateGraph(PlacementDesign Design, List<IBuildingParameters> Candidates, out Dictionary<IBuildingParameters, PathVertex> Vertices)
        {
            var graph = new PathGraph(Design.Position);
            var vertices = new Dictionary<IBuildingParameters, PathVertex>();
            var edges = new List<PathEdge>();
            Candidates.ForEach(C => vertices.Add(C, new PathVertex
            {
                Point = C.Position
            }));
            graph.AddVertex(vertices.Values.ToArray());
            graph.AddAttribute("OriginWeight", V => (V.Point - Design.Position).LengthFast);
            for (var i = 0; i < Candidates.Count; i++)
            {
                var from = vertices[Candidates[i]];
                if(from.Attributes.Get<float>("OriginWeight") <= 0f) continue;
                var to = this.FindNearest(graph, from);
                if(to == from || to == null) continue;
                Candidates[i].Rotation = Vector3.UnitY *
                                         Physics.DirectionToEuler((to.Point - from.Point).NormalizedFast());
                edges.Add(new PathEdge
                {
                    Origin = from,
                    End = to
                });
            }
            graph.AddEdge(edges.ToArray());
            Vertices = vertices;
            return graph;
        }

        private IBuildingParameters ParameterFromDegree(int Degree, Vector3 Position)
        {
            var point = new PlacementPoint
            {
                Position = Position,
            };
            
            if(Degree == 1) return this._farmPlacer.FromPoint(point);
            if(Degree == 2) return this._neighbourhoodPlacer.FromPoint(point);
            if(Degree == 3) return this._blacksmithPlacer.FromPoint(point);
            if(Degree > 3) return this._marketPlacer.FromPoint(point);
            throw new ArgumentOutOfRangeException();
        }
        
        private PathVertex FindNearest(PathGraph Graph, PathVertex Vertex)
        {
            var vertices = Graph.Vertices;
            PathVertex found = null;
            var weight = float.MaxValue;
            for (var i = 0; i < vertices.Length; i++)
            {
                var newWeight = Vertex.Attributes.Get<float>("OriginWeight") * (Vertex.Point - vertices[i].Point).LengthFast;
                if (Vertex != vertices[i] && newWeight < weight 
                    && vertices[i].Attributes.Get<float>("OriginWeight") < Vertex.Attributes.Get<float>("OriginWeight"))
                {
                    weight = newWeight;
                    found = vertices[i];
                }
            }
            return found;
        }
        private void RemoveIntersecting(PlacementDesign Design)
        {
            var parameters = Design.Blacksmith.Concat<IBuildingParameters>(Design.Farms)
                .Concat(Design.Neighbourhoods).Concat(Design.Stables).Concat(Design.Markets).ToList();
            parameters.Shuffle(_rng);
            var asPoints = parameters.Select(P => new PlacementPoint
            {
                Position = P.Position,
                Radius = P.GetSize(_root.Cache),
                CanBeRemoved = Array.IndexOf(Design.Markets, P) == -1
            }).ToList();

            var toRemove = new List<IBuildingParameters>();
            for (var i = asPoints.Count-1; i > -1; i--)
            {
                if (CollidesWithOtherPlacements(asPoints[i], asPoints))
                {
                    var torem = parameters[i];
                    toRemove.Add(torem);
                    asPoints.RemoveAt(i);
                    parameters.RemoveAt(i);
                }
            }
            Design.Blacksmith = this.RemoveIfExists<BlacksmithParameters>(Design.Blacksmith, toRemove);
            Design.Farms = this.RemoveIfExists<FarmParameters>(Design.Farms, toRemove);
            Design.Neighbourhoods = this.RemoveIfExists<NeighbourhoodParameters>(Design.Neighbourhoods, toRemove);
            Design.Markets = this.RemoveIfExists<MarketParameters>(Design.Markets, toRemove);
            Design.Stables = this.RemoveIfExists<BuildingParameters>(Design.Stables, toRemove);
        }

        private T[] RemoveIfExists<T>(object[] Parameters, List<IBuildingParameters> Posibilities)
        {
            var newList = Parameters.ToList();
            for (var i = 0; i < Posibilities.Count; i++)
            {
                var index = newList.IndexOf(Posibilities[i]);
                if (index != -1)
                {
                    newList.RemoveAt(index);
                }
            }
            return newList.Cast<T>().ToArray();
        }

        private PlacementPoint[] SamplePoints(VillageRing Ring)
        {
            const int max = 2048;
            var points = new PlacementPoint[max];
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = new PlacementPoint
                {
                     Position = new Vector3(
                         _rng.Next((int)-Ring.Radius, (int)Ring.Radius),
                         0,
                         _rng.Next((int) -Ring.Radius, (int) Ring.Radius)
                         ),
                     Radius = 0
                };
            }
            return points;
        }

        private static PlacementPoint[][] SortPoints(VillageRing Ring, PlacementPoint[] Points)
        {
            var innerRingPoints = new List<PlacementPoint>();
            var middleRingPoints = new List<PlacementPoint>();
            var outerRingPoints = new List<PlacementPoint>();
            for (var i = 0; i < Points.Length; i++)
            {
                var innerRing = Ring.InnerRing.InnerRing;
                var middleRing = Ring.InnerRing;

                if(Ring.Collides(Points[i]))
                    outerRingPoints.Add(Points[i]);

                if(middleRing.Collides(Points[i]))
                    middleRingPoints.Add(Points[i]);

                if(innerRing.Collides(Points[i]))
                    innerRingPoints.Add(Points[i]);    
            }
            return new[]
            {
                innerRingPoints.ToArray(),
                middleRingPoints.ToArray(),
                outerRingPoints.ToArray()
            };
        }

        private static bool CollidesWithOtherPlacements(PlacementPoint Point, List<PlacementPoint> Points)
        {
            for (var i = 0; i < Points.Count; i++)
            {
                if (Point.CanBeRemoved && PlacementPoint.Collide(Point, Points[i]) && Point != Points[i])
                    return true;
            }
            return false;
        }
    }
}