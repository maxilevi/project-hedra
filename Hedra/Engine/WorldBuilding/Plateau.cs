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
    public class Plateau
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public float MaxHeight { get; set; }
        public bool NoTrees { get; set; }

        public Plateau(Vector3 Position, float Radius)
        {
            this.Position = Position;
            this.Radius = Radius;
            this.MaxHeight = World.BiomePool.GetRegion(Position).Generation.GetHeight(Position.X, Position.Z, null, out _);
        }

        public bool Collides(Plateau Mount)
        {
            return (this.Position - Mount.Position).LengthFast < Mount.Radius + this.Radius;
        }
        
        public bool Collides(Vector2 Point)
        {
            return (this.Position.Xz - Point).LengthFast < this.Radius;
        }

        public float Apply(Vector2 Point, float Height, float SmallFrequency = 0)
        {
            var dist = (this.Position.Xz - Point).LengthSquared;
            var final = Math.Max(1 - Math.Min(dist / (this.Radius * this.Radius), 1), 0);
            var addonHeight = this.MaxHeight * Math.Max(final, 0f);

            Height += addonHeight;
            return Mathf.Lerp(Height - addonHeight,
                Math.Min(this.MaxHeight + SmallFrequency, Height),
                Math.Min(1.0f, final * 1.5f));
        }
    }
}
