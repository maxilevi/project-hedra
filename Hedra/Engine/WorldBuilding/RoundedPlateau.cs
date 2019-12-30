/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/06/2017
 * Time: 02:29 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Generation;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of Plateau.
    /// </summary>
    public class RoundedPlateau : BasePlateau
    {             
        public float Radius { get; set; }     
        public float Hardness { get; set; } = 3.0f;

        public RoundedPlateau(Vector2 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
        }

        public bool Collides(RoundedPlateau Mount)
        {
            return Collides(Mount.Position, Mount.Radius);
        }
        
        public bool Collides(Vector2 Point, float PointRadius)
        {
            return (this.Position - Point).LengthFast() < PointRadius + Radius;
        }
        
        public override bool Collides(Vector2 Point)
        {
            return Collides(Point, 1);
        }

        public override float Density(Vector2 Point)
        {
            var dist = (Position - Point).LengthFast();
            return (float) Math.Min(1.0f, Math.Max(1 - Math.Min(dist / Radius, 1), 0) * Hardness);
        }

        public override BoundingBox ToBoundingBox()
        {
            return new BoundingBox(Position, Radius * 2f * BorderMultiplier);
        }

        public override BasePlateau Clone()
        {
            return new RoundedPlateau(Position, Radius)
            {
                Hardness = Hardness,
                NoPlants = NoPlants,
                NoTrees = NoTrees
            };
        }
    }
}
