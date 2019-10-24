/*
 * Author: Zaphyk
 * Date: 28/02/2016
 * Time: 03:24 a.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Core
{
    public abstract class VAO : GLObject<VAO>
    {
        protected uint _id;
        public override uint Id => _id;
        private readonly List<VBO> _buffers;
        private bool _disposed;

        protected VAO()
        {
            _buffers = new List<VBO>();
        }
        
        public virtual void Bind(bool EnableAttributes = true)
        {
            Renderer.BindVAO(_id); 
        }

        public virtual void Unbind(bool DisableAttributes = true)
        {
            Renderer.BindVAO(0);
        }

        protected void Add(VBO Buffer)
        {
            _buffers.Add(Buffer);
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            base.Dispose();
            _disposed = true;
            _buffers.Clear();
            Executer.ExecuteOnMainThread(delegate
            {
                Renderer.DeleteVertexArrays(1, ref _id);
            });
        }

        public uint[] VBOIds => _buffers.Select(B => B.Id).ToArray();
        public VBO[] VBOs => _buffers.ToArray();
        public abstract Type[] Types { get; }
    }
    
    public class VAO<T1> : VAO where T1 : unmanaged
    {
        public VAO(VBO<T1> Buffer)
        {
            Renderer.GenVertexArrays(1, out _id);
            BindAndAdd(0, Buffer);
        }

        protected void BindAndAdd(int Position, VBO Buffer)
        {
            void Rebind()
            {
                Bind(false);
                Buffer.Bind();
                Renderer.VertexAttribPointer(Position, Buffer.Stride, Buffer.PointerType, false, 0);
                Buffer.Unbind();
                Unbind(false);
            }
            Rebind();
            Add(Buffer);
            Buffer.IdChanged += Rebind;
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            if(!EnableAttributes) return;
            Renderer.EnableVertexAttribArray(0);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Unbind(DisableAttributes);
            if(!DisableAttributes) return;
            Renderer.DisableVertexAttribArray(0);
        }

        public override Type[] Types => new[] { typeof(T1) };
    }
    
    public class VAO<T1, T2> : VAO<T1> where T1 : unmanaged where T2 : unmanaged
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2) : base(Buffer1)
        {
            BindAndAdd(1, Buffer2);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            if(!EnableAttributes) return;
            Renderer.EnableVertexAttribArray(1);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Unbind(DisableAttributes);
            if(!DisableAttributes) return;
            Renderer.DisableVertexAttribArray(1);
        }
        
        public override Type[] Types => base.Types.Concat(new[] { typeof(T2) }).ToArray();
    }
    
    public class VAO<T1, T2, T3> : VAO<T1, T2> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3) : base(Buffer1, Buffer2)
        {
            BindAndAdd(2, Buffer3);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            if(!EnableAttributes) return;
            Renderer.EnableVertexAttribArray(2);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Unbind(DisableAttributes);
            if(!DisableAttributes) return;
            Renderer.DisableVertexAttribArray(2);
        }

        public override Type[] Types => base.Types.Concat(new[] { typeof(T3) }).ToArray();
    }
    
    public class VAO<T1, T2, T3, T4> : VAO<T1, T2, T3> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged  where T4 : unmanaged
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4) : base(Buffer1, Buffer2, Buffer3)
        {
            BindAndAdd(3, Buffer4);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            if(!EnableAttributes) return;
            Renderer.EnableVertexAttribArray(3);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Unbind(DisableAttributes);
            if(!DisableAttributes) return;
            Renderer.DisableVertexAttribArray(3);
        }
        
        public override Type[] Types => base.Types.Concat(new[] { typeof(T4) }).ToArray();
    }
    
    public class VAO<T1, T2, T3, T4, T5> : VAO<T1, T2, T3, T4> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged  where T4 : unmanaged where T5 : unmanaged
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4, VBO<T5> Buffer5)
            : base(Buffer1, Buffer2, Buffer3, Buffer4)
        {
            BindAndAdd(4, Buffer5);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            if(!EnableAttributes) return;
            Renderer.EnableVertexAttribArray(4);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Unbind(DisableAttributes);
            if(!DisableAttributes) return;
            Renderer.DisableVertexAttribArray(4);
        }
        
        public override Type[] Types => base.Types.Concat(new[] { typeof(T5) }).ToArray();
    }
}
