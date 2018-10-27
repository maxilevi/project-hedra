/*
 * Author: Zaphyk
 * Date: 28/02/2016
 * Time: 03:24 a.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering
{
    public abstract class VAO : IDisposable
    {
        public uint ID => _id;
        protected uint _id;
        private bool _disposed;

        public virtual void Bind(bool EnableAttributes = true)
        {
            Renderer.BindVertexArray(ID);
        }
        
        public virtual void Unbind(bool DisableAttributes = true)
        {
            Renderer.BindVertexArray(0);
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Executer.ExecuteOnMainThread(delegate
            {
                Renderer.DeleteVertexArrays(1, ref _id);
            });
        }
    }
    
    public class VAO<T1> : VAO where T1 : struct
    {
        public VAO(VBO<T1> Buffer)
        {
            Renderer.GenVertexArrays(1, out _id);
            Renderer.BindVertexArray(_id);
            
            Renderer.BindBuffer(Buffer.BufferTarget, Buffer.ID);
            Renderer.VertexAttribPointer(0, Buffer.Size, Buffer.PointerType, false, 0, 0);
            
            Renderer.BindVertexArray(0);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            Renderer.EnableVertexAttribArray(0);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Bind(DisableAttributes);
            Renderer.DisableVertexAttribArray(0);
        }
    }
    
    public class VAO<T1, T2> : VAO where T1 : struct where T2 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2)
        {
            Renderer.GenVertexArrays(1, out _id);
            Renderer.BindVertexArray(_id);
            
            Renderer.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
            Renderer.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer2.BufferTarget, Buffer2.ID);
            Renderer.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
            
            Renderer.BindVertexArray(0);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            Renderer.EnableVertexAttribArray(0);
            Renderer.EnableVertexAttribArray(1);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Bind(DisableAttributes);
            Renderer.DisableVertexAttribArray(0);
            Renderer.DisableVertexAttribArray(1);
        }
    }
    
    public class VAO<T1, T2, T3> : VAO where T1 : struct where T2 : struct where T3 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3)
        {
            Renderer.GenVertexArrays(1, out _id);
            Renderer.BindVertexArray(_id);
            
            Renderer.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
            Renderer.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
            Renderer.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
            Renderer.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
            
            Renderer.BindVertexArray(0);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            Renderer.EnableVertexAttribArray(0);
            Renderer.EnableVertexAttribArray(1);
            Renderer.EnableVertexAttribArray(2);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Bind(DisableAttributes);
            Renderer.DisableVertexAttribArray(0);
            Renderer.DisableVertexAttribArray(1);
            Renderer.DisableVertexAttribArray(2);
        }
    }
    
    public class VAO<T1, T2, T3, T4> : VAO where T1 : struct where T2 : struct where T3 : struct  where T4 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4)
        {
            Renderer.GenVertexArrays(1, out _id);
            Renderer.BindVertexArray(_id);
            
            Renderer.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
            Renderer.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
            Renderer.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
            Renderer.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer4.BufferTarget, Buffer4.ID);
            Renderer.VertexAttribPointer(3, Buffer4.Size, Buffer4.PointerType, false, 0, 0);
            
            Renderer.BindVertexArray(0);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            Renderer.EnableVertexAttribArray(0);
            Renderer.EnableVertexAttribArray(1);
            Renderer.EnableVertexAttribArray(2);
            Renderer.EnableVertexAttribArray(3);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Bind(DisableAttributes);
            Renderer.DisableVertexAttribArray(0);
            Renderer.DisableVertexAttribArray(1);
            Renderer.DisableVertexAttribArray(2);
            Renderer.DisableVertexAttribArray(3);
        }
    }
    
    public class VAO<T1, T2, T3, T4, T5> : VAO where T1 : struct where T2 : struct where T3 : struct  where T4 : struct where T5 : struct
    {
        public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4, VBO<T5> Buffer5)
        {
            Renderer.GenVertexArrays(1, out _id);
            Renderer.BindVertexArray(_id);
            
            Renderer.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
            Renderer.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
            Renderer.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
            Renderer.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer4.BufferTarget, Buffer4.ID);
            Renderer.VertexAttribPointer(3, Buffer4.Size, Buffer4.PointerType, false, 0, 0);
            
            Renderer.BindBuffer(Buffer5.BufferTarget, Buffer5.ID);
            Renderer.VertexAttribPointer(4, Buffer5.Size, Buffer5.PointerType, false, 0, 0);
            
            Renderer.BindVertexArray(0);
        }

        public override void Bind(bool EnableAttributes = true)
        {
            base.Bind(EnableAttributes);
            Renderer.EnableVertexAttribArray(0);
            Renderer.EnableVertexAttribArray(1);
            Renderer.EnableVertexAttribArray(2);
            Renderer.EnableVertexAttribArray(3);
            Renderer.EnableVertexAttribArray(4);
        }

        public override void Unbind(bool DisableAttributes = true)
        {
            base.Bind(DisableAttributes);
            Renderer.DisableVertexAttribArray(0);
            Renderer.DisableVertexAttribArray(1);
            Renderer.DisableVertexAttribArray(2);
            Renderer.DisableVertexAttribArray(3);
            Renderer.DisableVertexAttribArray(4);
        }
    }
}
