﻿using System;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class PlacementPoint
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public bool CanBeRemoved { get; set; }

        public static bool Collide(PlacementPoint PointA, PlacementPoint PointB)
        {
            return (PointA.Position - PointB.Position).LengthFast < PointA.Radius + PointB.Radius;
        }
    }
}