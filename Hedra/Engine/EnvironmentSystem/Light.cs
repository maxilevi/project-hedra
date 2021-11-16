/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 05:04 a.m.
 *
 */

using System.Numerics;

namespace Hedra.Engine.EnvironmentSystem
{
    /// <summary>
    ///     Description of Light.
    /// </summary>
    public class Light
    {
        public Vector3 Color;
        public Vector3 Position;

        public Light(Vector3 Position, Vector3 Color)
        {
            this.Position = Position;
            this.Color = Color;
        }
    }
}