/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/11/2016
 * Time: 05:54 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using OpenTK;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.PhysicsSystem
{
    /// <inheritdoc cref="ICollidable" />
    /// <summary>
    /// Description of CollisionShape.
    /// </summary>
    public class CollisionShape : ICollidable, ICloneable
	{
		public List<Vector3> Vertices = new List<Vector3>();
		public List<uint> Indices = new List<uint>();
		public bool UseCache {get; set;}
        public float BroadphaseRadius {get; set; }
	    public Vector3 Center { get; private set; }
	    public bool UseBroadphase { get; set; } = true;

	    private Vector3 _avgCache;
	    private bool _avgCacheCalculated;
	    private float _radiusCache;
	    private bool _radiusCacheCalculated;
	    private Vector3 _position;


	    public CollisionShape(List<Vector3> Verts, List<uint> Indices)
	    {
	        if (Verts != null)
                this.Vertices = Verts;
            #if DEBUG
            if(Indices != null)
	            this.Indices = Indices;
#endif

	        this.CalculateCenter();
            this.CalculateBroadphase();
	    }

        public CollisionShape(List<Vector3> Verts) : this(Verts, null){}
		
		public CollisionShape(VertexData Data) : this(Data.Vertices, Data.Indices){}

	    public void SetCenter(Vector3 Center)
	    {
	        this.Center = Center;
	    }

	    private void CalculateBroadphase()
	    {
	        Vector3 middle = this.Center;

	        float dist = 0;
	        for (int i = 0; i < Vertices.Count; i++)
	        {
	            float length = (Vertices[i] - middle).LengthFast;

	            if (length > dist)
	                dist = length;
	        }
	        BroadphaseRadius = dist;
	    }

		public void Transform(Matrix4 TransMatrix){
			for(var i = 0; i < Vertices.Count; i++){
				Vertices[i] = Vector3.TransformPosition(Vertices[i], TransMatrix);
			}
            this.CalculateCenter();
            this.CalculateBroadphase();
        }
		public void Transform(Vector3 Position){
			for(var i = 0; i < Vertices.Count; i++){
				Vertices[i] = Vector3.TransformPosition(Vertices[i], Matrix4.CreateTranslation(Position));
			}
		    this.CalculateCenter();
            this.CalculateBroadphase();
        }
		
		
		public void CalculateCenter(){
			Vector3 avg = Vector3.Zero;
			for(var i = 0; i < Vertices.Count; i++){
				avg += Vertices[i];
			}
			avg /= Vertices.Count;
		    Center = avg;
		}
		
		public float Diameter{
			get{
				if(_radiusCacheCalculated) return _radiusCache;
				
				float radius = (this.Support(Vector3.One) - this.Support(-Vector3.One)).LengthFast;

			    if (_radiusCacheCalculated) return radius;
			    _radiusCache = radius;
			    _radiusCacheCalculated = true;
			    return radius;
			}
		}
		
		public Vector3 Support(Vector3 Direction){
			
		    float highest = float.MinValue;
		    Vector3 support = Vector3.Zero;

		    for (var i = 0; i < Vertices.Count; ++i) {
		        Vector3 v = Vertices[i];
		        float dot = Vector3.Dot(Direction, v);

		        if (!(dot > highest)) continue;
		        highest = dot;
		        support = v;
		    }
		
		    return support;
		}
		
		public object Clone(){
			var Shape = new CollisionShape( new List<Vector3>(Vertices) );
			#if DEBUG
			Shape.Indices = new List<uint>(this.Indices);
			#endif
			//Shape.SupportCache = this.SupportCache;
			return Shape;
		}

	    public float HighestPoint => Support(Vector3.UnitY).Y;
	}
}
