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
using System.Numerics;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.PhysicsSystem
{
    /// <inheritdoc cref="ICollidable" />
    /// <summary>
    ///     Description of CollisionShape.
    /// </summary>
    public class CollisionShape : ICloneable
    {
        private CollisionShape _cache;

        private CollisionShape(Vector3[] Vertices, uint[] Indices)
        {
            this.Vertices = Vertices ?? new Vector3[0];
            this.Indices = Indices ?? new uint[0];
            RecalculateBroadphase();
            Height = (SupportPoint(Vector3.UnitY) - SupportPoint(-Vector3.UnitY)).Y;
        }

        public CollisionShape(List<Vector3> Vertices, List<uint> Indices) : this(Vertices.ToArray(), Indices.ToArray())
        {
        }

        public CollisionShape(Vector3[] Vertices) : this(Vertices, null)
        {
        }

        public CollisionShape(VertexData Data) : this(Data.Vertices.ToArray(), Data.Indices.ToArray())
        {
        }

        public Vector3[] Vertices { get; }
        public uint[] Indices { get; }
        public float BroadphaseRadius { get; set; }
        public Vector3 BroadphaseCenter { get; set; }
        public float Height { get; private set; }

        public int SizeInBytes => Indices.Length * sizeof(uint) + Vertices.Length * HedraSize.Vector3;

        public object Clone()
        {
            return new CollisionShape(Vertices.ToArray(), Indices.ToArray());
        }

        public CollisionShape Transform(Matrix4x4 TransMatrix)
        {
            for (var i = 0; i < Vertices.Length; i++) Vertices[i] = Vector3.Transform(Vertices[i], TransMatrix);
            RecalculateBroadphase();
            Height = (SupportPoint(Vector3.UnitY) - SupportPoint(-Vector3.UnitY)).Y;
            return this;
        }

        public CollisionShape Transform(Vector3 Position)
        {
            for (var i = 0; i < Vertices.Length; i++)
                Vertices[i] = Vector3.Transform(Vertices[i], Matrix4x4.CreateTranslation(Position));
            RecalculateBroadphase();
            return this;
        }

        public Vector3 SupportPoint(Vector3 Direction)
        {
            var highest = float.MinValue;
            var support = Vector3.Zero;

            for (var i = 0; i < Vertices.Length; ++i)
            {
                var v = Vertices[i];
                var dot = Vector3.Dot(Direction, v);

                if (!(dot > highest)) continue;
                highest = dot;
                support = v;
            }

            return support;
        }

        public void RecalculateBroadphase()
        {
            RecalculateBroadphase(Vector3.One);
        }

        public void RecalculateBroadphase(Vector3 Mask)
        {
            float dist = 0;
            var verticesSum = Vector3.Zero;
            for (var i = 0; i < Vertices.Length; i++) verticesSum += Vertices[i] * Mask;
            BroadphaseCenter = verticesSum / Vertices.Length;
            for (var i = 0; i < Vertices.Length; i++)
            {
                var length = (Vertices[i] * Mask - BroadphaseCenter).LengthFast();

                if (length > dist)
                    dist = length;
            }

            BroadphaseRadius = dist;
        }
    }
}