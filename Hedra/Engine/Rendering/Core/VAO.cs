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
        private readonly List<uint> _bufferIds;
        private bool _disposed;

        protected VAO()
        {
            _bufferIds = new List<uint>();
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
            _bufferIds.Add(Buffer.Id);
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            base.Dispose();
            _disposed = true;
            Executer.ExecuteOnMainThread(delegate
            {
                Renderer.DeleteVertexArrays(1, ref _id);
            });
        }

        public uint[] VBOIds => _bufferIds.ToArray();
        public VBO[] VBOs => _bufferIds.Select(VBO.GetById).ToArray();
        public abstract Type[] Types { get; }
    }
    
    public class VAO<T1> : VAO where T1 : struct
    {
        public VAO(VBO<T1> Buffer)
        {
            Renderer.GenVertexArrays(1, out _id);
            Bind(false);
            
            Buffer.Bind();
            Renderer.VertexAttribPointer(0, Buffer.Stride, Buffer.PointerType, false, 0, 0);
            Buffer.Unbind();
            
            Unbind(false);

            Add(Buffer);
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
    
    public class VAO<T1, T2> : VAO<T1> where T1 : struct where T2 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2) : base(Buffer1)
        {
            Bind(false);

            Buffer2.Bind();
            Renderer.VertexAttribPointer(1, Buffer2.Stride, Buffer2.PointerType, false, 0, 0);
            Buffer2.Unbind();
            
            Unbind(false);
            
            Add(Buffer2);
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
    
    public class VAO<T1, T2, T3> : VAO<T1, T2> where T1 : struct where T2 : struct where T3 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3) : base(Buffer1, Buffer2)
        {
            Bind(false);
            
            Buffer3.Bind();
            Renderer.VertexAttribPointer(2, Buffer3.Stride, Buffer3.PointerType, false, 0, 0);
            Buffer3.Unbind();
            
            Unbind(false);
            
            Add(Buffer3);
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
    
    public class VAO<T1, T2, T3, T4> : VAO<T1, T2, T3> where T1 : struct where T2 : struct where T3 : struct  where T4 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4) : base(Buffer1, Buffer2, Buffer3)
        {
            Bind(false);
            
            Buffer4.Bind();
            Renderer.VertexAttribPointer(3, Buffer4.Stride, Buffer4.PointerType, false, 0, 0);
            Buffer4.Unbind();
            
            Unbind(false);
            
            Add(Buffer4);
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
    
    public class VAO<T1, T2, T3, T4, T5> : VAO<T1, T2, T3, T4> where T1 : struct where T2 : struct where T3 : struct  where T4 : struct where T5 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4, VBO<T5> Buffer5)
            : base(Buffer1, Buffer2, Buffer3, Buffer4)
        {
            Bind(false);
            
            Buffer5.Bind();
            Renderer.VertexAttribPointer(4, Buffer5.Stride, Buffer5.PointerType, false, 0, 0);
            Buffer5.Unbind();
            
            Unbind(false);
            
            Add(Buffer5);
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
