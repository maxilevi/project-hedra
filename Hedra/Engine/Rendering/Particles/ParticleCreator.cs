/*
 * Author: Zaphyk
 * Date: 16/02/2016
 * Time: 07:52 p.m.
 *
 */
using System;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering.Particles
{
    public sealed class ParticleCreator
    {    
        
        public static VBO<Vector3> VerticesVBO { get; private set; }
        public static VBO<ushort> IndicesVBO { get; private set; }
        public static VBO<Vector3> NormalsVBO { get; private set; }
        
        public static void Load(){
            if(VerticesVBO == null && IndicesVBO == null && NormalsVBO == null){
                VertexData Data = AssetManager.PLYLoader("Assets/Env/Particle.ply", Vector3.One,Vector3.Zero, Vector3.Zero, true);
                ushort[] NewIndices = new ushort[Data.Indices.Count];
                for(int i = 0; i < NewIndices.Length; i++){
                    NewIndices[i] = (ushort) Data.Indices[i];
                }
                
                NormalsVBO = new VBO<Vector3>(Data.Normals.ToArray(), Data.Normals.Count * Vector3.SizeInBytes, VertexAttribPointerType.Float);
                VerticesVBO = new VBO<Vector3>(Data.Vertices.ToArray(), Data.Vertices.Count * Vector3.SizeInBytes, VertexAttribPointerType.Float);
                IndicesVBO = new VBO<ushort>(NewIndices, Data.Indices.Count * sizeof(ushort), VertexAttribPointerType.UnsignedShort, BufferTarget.ElementArrayBuffer);
            }
        }
        
        public static Vector3 UnitWithinCone(Vector3 coneDirection, float angle) {
            float cosAngle = (float) Math.Cos(angle);

            float theta = (float) (Utils.Rng.NextFloat() * 2f * Math.PI);
            float z = cosAngle + (Utils.Rng.NextFloat() * (1 - cosAngle));
            float rootOneMinusZSquared = (float) Math.Sqrt(1 - z * z);
            float x = (float) (rootOneMinusZSquared * Math.Cos(theta));
            float y = (float) (rootOneMinusZSquared * Math.Sin(theta));
     
            Vector4 direction = new Vector4(x, y, z, 1);
            if (coneDirection.X != 0 || coneDirection.Y != 0 || (coneDirection.Z != 1 && coneDirection.Z != -1)) {
                Vector3 rotateAxis = Vector3.Cross(coneDirection, new Vector3(0, 0, 1));
                rotateAxis.Normalize();
                float rotateAngle = (float) Math.Acos(Vector3.Dot(coneDirection, new Vector3(0, 0, 1)));
                Matrix4 rotationMatrix = Matrix4.CreateFromAxisAngle(rotateAxis, -rotateAngle);
                direction = Vector4.Transform(direction, rotationMatrix);
            } else if (coneDirection.Z == -1) {
                direction.Z *= -1;
            }
            return new Vector3(direction);
        }
    }
}
