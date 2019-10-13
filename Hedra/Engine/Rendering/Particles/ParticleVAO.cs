using System;
using System.Linq;
using Hedra.Engine.Rendering.Core;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;

namespace Hedra.Engine.Rendering.Particles
{
    public sealed class ParticleVAO : VAO<Vector3, Vector3>
    {
        public ParticleVAO(VBO<Vector3> Vertices, VBO<Vector3> Normals, VBO<Vector4> ParticleBuffer)
            : base(Vertices, Normals)
        {
            void Rebind()
            {
                Bind(false);
                ParticleBuffer.Bind();
                Renderer.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes * InstanceStride, IntPtr.Zero);            
                Renderer.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes * InstanceStride, (IntPtr) (Vector4.SizeInBytes * 1));
                Renderer.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes * InstanceStride, (IntPtr) (Vector4.SizeInBytes * 2));
                Renderer.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes * InstanceStride, (IntPtr) (Vector4.SizeInBytes * 3));
                Renderer.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes * InstanceStride, (IntPtr) (Vector4.SizeInBytes * 4));
            
                Renderer.VertexAttribDivisor(2,1);
                Renderer.VertexAttribDivisor(3,1);
                Renderer.VertexAttribDivisor(4,1);
                Renderer.VertexAttribDivisor(5,1);
                Renderer.VertexAttribDivisor(6,1);
                ParticleBuffer.Unbind();
                Unbind(false);
            }
            Rebind();
            ParticleBuffer.IdChanged += Rebind;

            Add(ParticleBuffer);
        }

        public override Type[] Types => base.Types.Concat(new[]
        {
            typeof(Vector4),
            typeof(Vector4),
            typeof(Vector4),
            typeof(Vector4),
            typeof(Vector4)
        }).ToArray();

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            if(!EnableAttributes) return;
            Renderer.EnableVertexAttribArray(2);
            Renderer.EnableVertexAttribArray(3);
            Renderer.EnableVertexAttribArray(4);
            Renderer.EnableVertexAttribArray(5);
            Renderer.EnableVertexAttribArray(6);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Unbind(DisableAttributes);
            if(!DisableAttributes) return;
            Renderer.DisableVertexAttribArray(2);
            Renderer.DisableVertexAttribArray(3);
            Renderer.DisableVertexAttribArray(4);
            Renderer.DisableVertexAttribArray(5);
            Renderer.DisableVertexAttribArray(6);
        }

        public int InstanceStride => 5;
        public VBO ParticleBuffer => VBOs[VBOs.Length - 1];
    }
}