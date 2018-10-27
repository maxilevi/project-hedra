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
using System.Linq;
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
        public Vector3[] Vertices { get; }
        public uint[] Indices { get; }
        public float BroadphaseRadius { get; set; }
        public Vector3 BroadphaseCenter { get; set; }
        public bool UseBroadphase { get; set; } = true;
        public float Height { get; private set; }
        private CollisionShape _cache;

        public CollisionShape(Vector3[] Vertices, uint[] Indices)
        {
            this.Vertices = Vertices ?? new Vector3[0];
#if !DEBUG
            Indices = null;
#endif
            this.Indices = Indices ?? new uint[0];
            this.RecalculateBroadphase();
            this.Height = (Support(Vector3.UnitY) - Support(-Vector3.UnitY)).Y;
        }

        public CollisionShape Transform(Matrix4 TransMatrix)
        {
            for(var i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.TransformPosition(Vertices[i], TransMatrix);
            }
            this.RecalculateBroadphase();
            this.Height = (Support(Vector3.UnitY) - Support(-Vector3.UnitY)).Y;
            return this;
        }

        public CollisionShape Transform(Vector3 Position)
        {
            for(var i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.TransformPosition(Vertices[i], Matrix4.CreateTranslation(Position));
            }
            this.RecalculateBroadphase();
            return this;
        }

        public Vector3 Support(Vector3 Direction)
        {
            
            var highest = float.MinValue;
            var support = Vector3.Zero;

            for (var i = 0; i < Vertices.Length; ++i)
            {
                Vector3 v = Vertices[i];
                float dot = Vector3.Dot(Direction, v);

                if (!(dot > highest)) continue;
                highest = dot;
                support = v;
            }
        
            return support;
        }

        public void RecalculateBroadphase()
        {
            float dist = 0;
            var verticesSum = Vector3.Zero;
            for (var i = 0; i < Vertices.Length; i++)
            {
                verticesSum += Vertices[i];
            }
            this.BroadphaseCenter = verticesSum / Vertices.Length;
            for (var i = 0; i < Vertices.Length; i++)
            {
                float length = (Vertices[i] - this.BroadphaseCenter).LengthFast;

                if (length > dist)
                    dist = length;
            }
            this.BroadphaseRadius = dist;
        }

        public object Clone()
        {
            return new CollisionShape(Vertices.ToArray(), this.Indices.ToArray());
        }

        public CollisionShape(List<Vector3> Vertices, List<uint> Indices) : this(Vertices.ToArray(), Indices.ToArray())
        {
        }

        public CollisionShape(List<Vector3> Vertices) : this(Vertices.ToArray(), null)
        {
        }

        public CollisionShape(Vector3[] Vertices) : this(Vertices, null)
        {
        }

        public CollisionShape(VertexData Data) : this(Data.Vertices.ToArray(), Data.Indices.ToArray())
        {       
        }

        public CollisionShape Cache
        {
            get
            {
                if(_cache == null) _cache = new CollisionShape(Vertices.ToArray(), Indices.ToArray());
                for (var i = 0; i < _cache.Vertices.Length; i++)
                {
                    _cache.Vertices[i] = this.Vertices[i];
                }
                return _cache;
            }
        }
    }
}
