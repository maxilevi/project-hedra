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
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of Plateau.
    /// </summary>
    public class RoundedPlateau : BasePlateau
    {             
        public float Radius { get; set; }     
        public float Hardness { get; set; } = 3.0f;

        public RoundedPlateau(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
        }

        public bool Collides(RoundedPlateau Mount)
        {
            return (this.Position - Mount.Position).LengthFast < Mount.Radius + this.Radius;
        }
        
        public override bool Collides(Vector2 Point)
        {
            return (this.Position.Xz - Point).LengthFast < this.Radius;
        }

        protected override float Density(Vector2 Point)
        {
            var dist = (this.Position.Xz - Point).LengthFast;
            return Math.Min(1.0f, Math.Max(1 - Math.Min(dist / Radius, 1), 0) * Hardness);
        }
    }
}
