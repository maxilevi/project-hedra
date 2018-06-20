/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/11/2016
 * Time: 07:34 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
	/// <summary>
	/// Description of Box.
	/// </summary>
	public class Box : ICollidable
    {
		public Vector3 Min { get; set; }
	    public Vector3 Max { get; set; }
        private Box _cache;
        private Pool<Box> _poolCache;
        private Vector3 _shapeCenter;
        private CollisionShape _shape;
        private CollisionShape _boxShape;
        private Vector3[] _verticesCache;

        #region Operators
        public Box() : this(Vector3.Zero, Vector3.One) { }

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

        public Box Add(Vector3 NewMin, Vector3 NewMax)
        {
            this.Min += NewMin;
            this.Max += NewMax;
            return this;
        }

        public Box Translate(Vector3 Position)
        {
            this.Max += Position;
            this.Min += Position;
            return this;
        }

        public static bool operator ==(Box Box1, Box Box2)
		{
		    bool b1Null = object.ReferenceEquals(Box1, null);
            bool b2Null = object.ReferenceEquals(Box2, null);

            if (b1Null && b2Null) return true;
		    if (b1Null || b2Null) return false;
			return Box1.Min == Box2.Min && Box2.Max == Box1.Max;
		}
		
		public static bool operator !=(Box Box1, Box Box2)
		{
		    bool b1Null = object.ReferenceEquals(Box1, null);
		    bool b2Null = object.ReferenceEquals(Box2, null);

		    if (b1Null && b2Null) return false;
		    if (b1Null || b2Null) return true;
		    return Box1.Min != Box2.Min || Box2.Max != Box1.Max;
        }
#endregion

        public Vector3[] Vertices
        {
            get
            {
                if(_verticesCache == null) _verticesCache = new Vector3[8];

                _verticesCache[0] = new Vector3(Min.X, Min.Y, Min.Z);
                _verticesCache[1] = new Vector3(Max.X, Min.Y, Min.Z);
                _verticesCache[2] = new Vector3(Min.X, Min.Y, Max.Z);
                _verticesCache[3] = new Vector3(Max.X, Min.Y, Max.Z);

                _verticesCache[4] = new Vector3(Min.X, Max.Y, Min.Z);
                _verticesCache[5] = new Vector3(Max.X, Max.Y, Min.Z);
                _verticesCache[6] = new Vector3(Min.X, Max.Y, Max.Z);
                _verticesCache[7] = new Vector3(Max.X, Max.Y, Max.Z);
                return _verticesCache;
            }
        }
/*
		public Box Cache
        {
			get
            {
			    if(_poolCache == null) _poolCache = new Pool<Box>();
			    var cache = _poolCache.Grab();
                cache.Min = this.Min;
			    cache.Max = this.Max;				
				return cache;
			}
		}*/

        public Box Cache
        {
            get
            {
                if (!(_cache is ICollidable collidable)) // Cheap trick so there is no stackoverflow
                    _cache = new Box(Vector3.Zero, Vector3.Zero);

                this._cache.Min = this.Min;
                this._cache.Max = this.Max;

                return _cache;
            }
        }

        public Vector3 Size => Max - Min;

		public Vector3 Average => (Min + Max) / 2;

		public CollisionShape ToShape()
		{

		    if (_shapeCenter == this.Average && _shape != null)
		        return _shape;

            if(_boxShape == null)
                _boxShape = new CollisionShape(new Vector3[8]);

		    Vector3 halfSize = (this.Max - this.Min) * .5f;

		    _boxShape.Vertices[0] = this.Min - halfSize;

		    _boxShape.Vertices[1] = this.Min * new Vector3(0,1,1) + new Vector3(this.Max.X,0,0) - halfSize;
		    _boxShape.Vertices[2] = this.Min * new Vector3(1,0,1) + new Vector3(0,this.Max.Y,0) - halfSize;
		    _boxShape.Vertices[3] = this.Min * new Vector3(1,1,0) + new Vector3(0,0,this.Max.Z) - halfSize;

		    _boxShape.Vertices[4] = this.Min * new Vector3(0,1,0) + new Vector3(this.Max.X,0,this.Max.Z) - halfSize;
		    _boxShape.Vertices[5] = this.Min * new Vector3(0,0,1) + new Vector3(this.Max.X,this.Max.Y,0) - halfSize;
		    _boxShape.Vertices[6] = this.Min * new Vector3(1,0,0) + new Vector3(0,this.Max.Y,this.Max.Z) - halfSize;

		    _boxShape.Vertices[7] = this.Max - halfSize;

		    _boxShape.BroadphaseCenter = (this.Min + this.Max) * .5f;
		    _boxShape.BroadphaseRadius = (this.Min - this.Max).LengthFast;

		    _shape = _boxShape;
		    _shapeCenter = this.Average;

			return _shape;
		}

        public Box Clone()
        {
            return new Box(this.Min, this.Max);
        }
    }
}
