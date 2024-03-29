/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/11/2016
 * Time: 07:34 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.PhysicsSystem
{
    /// <summary>
    ///     Description of Box.
    /// </summary>
    public class Box
    {
        private CollisionShape _boxShape;
        private Box _cache;
        private Vector3[] _highResVerticesCache;
        private CollisionShape _shape;
        private Vector3 _shapeCenter;
        private Vector3[] _verticesCache;

        public Box Cache
        {
            get
            {
                _cache = new Box(Vector3.Zero, Vector3.Zero)
                {
                    Min = Min,
                    Max = Max
                };
                return _cache;
            }
        }

        public Vector3 Size => Max - Min;

        private Vector3 Average => (Min + Max) / 2;

        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public CollisionShape ToShape()
        {
            var avg = Average;
            if (_shapeCenter == avg && _shape != null)
                return _shape;

            if (_boxShape == null)
                _boxShape = new CollisionShape(new Vector3[8]);

            _boxShape.Vertices[0] = Min;
            _boxShape.Vertices[1] = new Vector3(Max.X, Min.Y, Min.Z);
            _boxShape.Vertices[2] = new Vector3(Min.X, Min.Y, Max.Z);
            _boxShape.Vertices[3] = new Vector3(Max.X, Min.Y, Max.Z);

            _boxShape.Vertices[4] = new Vector3(Min.X, Max.Y, Min.Z);
            _boxShape.Vertices[5] = new Vector3(Max.X, Max.Y, Min.Z);
            _boxShape.Vertices[6] = new Vector3(Min.X, Max.Y, Max.Z);
            _boxShape.Vertices[7] = Max;

            _boxShape.BroadphaseCenter = (Min + Max) * .5f;
            _boxShape.BroadphaseRadius = (Min - Max).LengthFast();

            _shape = _boxShape;
            _shapeCenter = avg;

            return _shape;
        }

        #region Operators

        public Box() : this(Vector3.Zero, Vector3.One)
        {
        }

        public Box(Vector3 Min, Vector3 Max)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public static Box operator +(Box Box1, Box Box2)
        {
            Box1.Min += Box2.Min;
            Box1.Max += Box2.Max;
            return Box1;
        }

        public static Box operator -(Box Box1, Box Box2)
        {
            Box1.Min -= Box2.Min;
            Box1.Max -= Box2.Max;
            return Box1;
        }

        public static Box operator *(Box Box1, Vector3 Scale)
        {
            Box1.Min *= Scale;
            Box1.Max *= Scale;
            return Box1;
        }

        public static Box operator *(Box Box1, float Scale)
        {
            Box1.Min *= Scale;
            Box1.Max *= Scale;
            return Box1;
        }

        public Box Translate(Vector3 Position)
        {
            Max += Position;
            Min += Position;
            return this;
        }

        public static bool operator ==(Box Box1, Box Box2)
        {
            var b1Null = ReferenceEquals(Box1, null);
            var b2Null = ReferenceEquals(Box2, null);

            if (b1Null && b2Null) return true;
            if (b1Null || b2Null) return false;
            return Box1.Min == Box2.Min && Box2.Max == Box1.Max;
        }

        public static bool operator !=(Box Box1, Box Box2)
        {
            var b1Null = ReferenceEquals(Box1, null);
            var b2Null = ReferenceEquals(Box2, null);

            if (b1Null && b2Null) return false;
            if (b1Null || b2Null) return true;
            return Box1.Min != Box2.Min || Box2.Max != Box1.Max;
        }

        #endregion
    }
}