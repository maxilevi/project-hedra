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
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
	/// <summary>
	/// Description of Box.
	/// </summary>
	public class Box : ICollidable{
		public Vector3 Min { get; set; }
	    public Vector3 Max { get; set; }

	    public Box(Vector3 Min, Vector3 Max){
			this.Min = Min;
			this.Max = Max;
		}
		
		public static Box operator +(Box Box1, Box Box2){
			Box1.Min += Box2.Min;
			Box1.Max += Box2.Max;
			return Box1;
		}
		
		public static Box operator -(Box Box1, Box Box2){
			Box1.Min -= Box2.Min;
			Box1.Max -= Box2.Max;
			return Box1;
		}
		
		public static Box operator *(Box Box1, Vector3 Scale){
			Box1.Min *= Scale;
			Box1.Max *= Scale;
			return Box1;
		}
		
		public static Box operator *(Box Box1, float Scale){
			Box1.Min *= Scale;
			Box1.Max *= Scale;
			return Box1;
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

	    public Box Rotate(Vector3 Euler)
	    {
	        if (Euler == Vector3.Zero || float.IsInfinity(Euler.X) || float.IsNaN(Euler.X) ||
	            float.IsInfinity(Euler.Y) || float.IsNaN(Euler.Y) ||
	            float.IsInfinity(Euler.Z) || float.IsNaN(Euler.Z))
	        {
	            return this;
	        }
	        var mat = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Euler * Mathf.Radian));
                
	        this.Min = Vector3.TransformPosition(this.Min, mat);
	        this.Max = Vector3.TransformPosition(this.Max, mat);
	        return this;
	    }

		public Box Clone(){
			return new Box(this.Min, this.Max);
		}

		private Box _cache;		
		public Box Cache{
			get{
			    var collidable = _cache as ICollidable;
			    if( collidable == null)//Cheap trick so there is no stackoverflow
					_cache = new Box(Vector3.Zero, Vector3.Zero);
				
				this._cache.Min = this.Min;
				this._cache.Max = this.Max;
				
				return _cache;
			}
		}
		
		public Box Add(Vector3 Min, Vector3 Max){
			this.Min += Min;
			this.Max += Max;
			return this;
		}
		
		public Box Translate(Vector3 Position){
			this.Max += Position;
			this.Min += Position;
			return this;
		}
		
		public Box Scale(float Size){
			//this.Cached = false;
			this.Min *= Size;
			this.Max *= Size;
			return this;
		}
		
		public Vector3 Average => (Min + Max) / 2;

	    private CollisionShape _boxShape;
		public CollisionShape BoxShape{
			get{
				if(_boxShape == null){
					
                    var vertices = new List<Vector3>();
					Vector3 Min = Vector3.Zero, Max = Vector3.Zero;
					vertices.Add(Min - (Max - Min) * .5f);
					
					vertices.Add(Min * new Vector3(0,1,1) + new Vector3(Max.X,0,0) - (Max - Min) * .5f);
					vertices.Add(Min * new Vector3(1,0,1) + new Vector3(0,Max.Y,0) - (Max - Min) * .5f);
					vertices.Add(Min * new Vector3(1,1,0) + new Vector3(0,0,Max.Z) - (Max - Min) * .5f);
					
					vertices.Add(Min * new Vector3(0,1,0) + new Vector3(Max.X,0,Max.Z) - (Max - Min) * .5f);
					vertices.Add(Min * new Vector3(0,0,1) + new Vector3(Max.X,Max.Y,0) - (Max - Min) * .5f);
					vertices.Add(Min * new Vector3(1,0,0) + new Vector3(0,Max.Y,Max.Z) - (Max - Min) * .5f);
					
					vertices.Add(Max - (Max - Min) * .5f);

                    _boxShape = new CollisionShape(vertices);
				}
				return _boxShape;
			}
		}
		
		//Multiple threads and this shit explodes
	    private Vector3 _shapeCenter;
	    private CollisionShape _shape;
		public CollisionShape ToShape()
		{

		    if (_shapeCenter == this.Average && _shape != null)
		        return _shape;


            CollisionShape Shape = BoxShape;
		    Vector3 halfSize = (this.Max - this.Min) * .5f;

            Shape.Vertices[0] = this.Min - halfSize;
			
			Shape.Vertices[1] = this.Min * new Vector3(0,1,1) + new Vector3(this.Max.X,0,0) - halfSize;
			Shape.Vertices[2] = this.Min * new Vector3(1,0,1) + new Vector3(0,this.Max.Y,0) - halfSize;
			Shape.Vertices[3] = this.Min * new Vector3(1,1,0) + new Vector3(0,0,this.Max.Z) - halfSize;
			
			Shape.Vertices[4] = this.Min * new Vector3(0,1,0) + new Vector3(this.Max.X,0,this.Max.Z) - halfSize;
			Shape.Vertices[5] = this.Min * new Vector3(0,0,1) + new Vector3(this.Max.X,this.Max.Y,0) - halfSize;
			Shape.Vertices[6] = this.Min * new Vector3(1,0,0) + new Vector3(0,this.Max.Y,this.Max.Z) - halfSize;
			
			Shape.Vertices[7] = this.Max - halfSize;

		    Shape.SetCenter( (this.Min + this.Max) * .5f);
            Shape.BroadphaseRadius = (this.Min - this.Max).LengthFast;

		    _shape = Shape;
		    _shapeCenter = this.Average;

			return BoxShape;
		}

	    public float HighestPoint => Max.Y;
	}
}
