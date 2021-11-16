/*
 * Author: Zaphyk
 * Date: 16/02/2016
 * Time: 07:52 p.m.
 *
 */

using System;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Particles
{
    public sealed class ParticleCreator
    {
        public static VBO<Vector3> VerticesVBO { get; private set; }
        public static VBO<ushort> IndicesVBO { get; private set; }
        public static VBO<Vector3> NormalsVBO { get; private set; }

        public static void Load()
        {
            if (VerticesVBO == null && IndicesVBO == null && NormalsVBO == null)
            {
                var Data = AssetManager.PLYLoader("Assets/Env/Particle.ply", Vector3.One, Vector3.Zero, Vector3.Zero);
                var NewIndices = new ushort[Data.Indices.Count];
                for (var i = 0; i < NewIndices.Length; i++) NewIndices[i] = (ushort)Data.Indices[i];

                NormalsVBO = new VBO<Vector3>(Data.Normals.ToArray(), Data.Normals.Count * HedraSize.Vector3,
                    VertexAttribPointerType.Float);
                VerticesVBO = new VBO<Vector3>(Data.Vertices.ToArray(), Data.Vertices.Count * HedraSize.Vector3,
                    VertexAttribPointerType.Float);
                IndicesVBO = new VBO<ushort>(NewIndices, Data.Indices.Count * sizeof(ushort),
                    VertexAttribPointerType.UnsignedShort, BufferTarget.ElementArrayBuffer);
            }
        }

        public static Vector3 UnitWithinCone(Vector3 coneDirection, float angle)
        {
            var cosAngle = (float)Math.Cos(angle);

            var theta = (float)(Utils.Rng.NextFloat() * 2f * Math.PI);
            var z = cosAngle + Utils.Rng.NextFloat() * (1 - cosAngle);
            var rootOneMinusZSquared = (float)Math.Sqrt(1 - z * z);
            var x = (float)(rootOneMinusZSquared * Math.Cos(theta));
            var y = (float)(rootOneMinusZSquared * Math.Sin(theta));

            var direction = new Vector4(x, y, z, 1);
            if (coneDirection.X != 0 || coneDirection.Y != 0 || coneDirection.Z != 1 && coneDirection.Z != -1)
            {
                var rotateAxis = Vector3.Cross(coneDirection, new Vector3(0, 0, 1)).Normalized();
                var rotateAngle = (float)Math.Acos(Vector3.Dot(coneDirection, new Vector3(0, 0, 1)));
                var rotationMatrix = Matrix4x4.CreateFromAxisAngle(rotateAxis, -rotateAngle);
                direction = Vector4.Transform(direction, rotationMatrix);
            }
            else if (coneDirection.Z == -1)
            {
                direction.Z *= -1;
            }

            return direction.Xyz();
        }
    }
}