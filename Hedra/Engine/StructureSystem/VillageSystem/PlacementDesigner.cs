using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Placers;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal sealed class PlacementDesigner
    {
        private readonly VillageRoot _root;
        private readonly Random _rng;
        private readonly VillageConfiguration _config;
        private readonly FarmPlacer _farmPlacer;
        private readonly BlacksmithPlacer _blacksmithPlacer;
        private readonly Placer<BuildingParameters> _housePlacer;
        private readonly Placer<BuildingParameters> _stablePlacer;
        private readonly MarketPlacer _marketPlacer;

        public PlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng)
        {
            this._root = Root;
            this._rng = Rng;
            this._config = Config;
            this._farmPlacer = new FarmPlacer(Root.Template.Farm.Designs, Root.Template.Windmill.Designs, Rng);
            this._blacksmithPlacer = new BlacksmithPlacer(Root.Template.Blacksmith.Designs, Rng);
            this._housePlacer = new Placer<BuildingParameters>(Root.Template.House.Designs, Rng);
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
                design.Houses = design.Houses.Concat(this._housePlacer.Place(sortedPoints[i], configs[i].HouseChances)).ToArray();
                design.Farms = design.Farms.Concat(this._farmPlacer.Place(sortedPoints[i], configs[i].FarmChances)).ToArray();                
            }
            this.RemoveIntersecting(design);
            return design;
        }

        private void RemoveIntersecting(PlacementDesign Design)
        {
            var parameters = Design.Blacksmith.Concat<IBuildingParameters>(Design.Farms)
                .Concat(Design.Houses).Concat(Design.Stables).Concat(Design.Markets).ToList();
            parameters.Shuffle(_rng);
            var asPoints = parameters.Select(P => new PlacementPoint
            {
                Position = P.Position,
                Radius = P.GetSize(_root),
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
            Design.Houses = this.RemoveIfExists<BuildingParameters>(Design.Houses, toRemove);
            Design.Markets = this.RemoveIfExists<BuildingParameters>(Design.Markets, toRemove);
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
            const int max = 256;
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
                if (PlacementPoint.Collide(Point, Points[i]) && Point != Points[i] && Point.CanBeRemoved)
                    return true;
            }
            return false;
        }
    }
}