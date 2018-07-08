/*
 * Author: Zaphyk
 * Date: 28/02/2016
 * Time: 03:24 a.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering
{
	internal abstract class VAO : IDisposable
	{
	    public uint ID => _id;
		protected uint _id;
		private bool _disposed;

	    public virtual void Bind(bool EnableAttributes = true)
	    {
	        GL.BindVertexArray(ID);
        }
		
		public virtual void Unbind(bool DisableAttributes = true)
        {
			GL.BindVertexArray(0);
		}
		
		public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Executer.ExecuteOnMainThread(delegate
            {
                GL.DeleteVertexArrays(1, ref _id);
            });
        }
	}
	
	internal class VAO<T1> : VAO where T1 : struct
	{
		public VAO(VBO<T1> Buffer)
        {
			GL.GenVertexArrays(1, out _id);
			GL.BindVertexArray(_id);
			
			GL.BindBuffer(Buffer.BufferTarget, Buffer.ID);
			GL.VertexAttribPointer(0, Buffer.Size, Buffer.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}

	    public override void Bind(bool EnableAttributes = true)
	    {
	        base.Bind(EnableAttributes);
            GL.EnableVertexAttribArray(0);
	    }

	    public override void Unbind(bool DisableAttributes = true)
	    {
	        base.Bind(DisableAttributes);
	        GL.DisableVertexAttribArray(0);
	    }
    }
	
	internal class VAO<T1, T2> : VAO where T1 : struct where T2 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2)
        {
			GL.GenVertexArrays(1, out _id);
			GL.BindVertexArray(_id);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer2.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}

	    public override void Bind(bool EnableAttributes = true)
	    {
	        base.Bind(EnableAttributes);
	        GL.EnableVertexAttribArray(0);
	        GL.EnableVertexAttribArray(1);
        }

	    public override void Unbind(bool DisableAttributes = true)
	    {
	        base.Bind(DisableAttributes);
	        GL.DisableVertexAttribArray(0);
	        GL.DisableVertexAttribArray(1);
        }
    }
	
	internal class VAO<T1, T2, T3> : VAO where T1 : struct where T2 : struct where T3 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3)
        {
			GL.GenVertexArrays(1, out _id);
			GL.BindVertexArray(_id);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
			GL.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}

	    public override void Bind(bool EnableAttributes = true)
	    {
	        base.Bind(EnableAttributes);
	        GL.EnableVertexAttribArray(0);
	        GL.EnableVertexAttribArray(1);
	        GL.EnableVertexAttribArray(2);
        }

	    public override void Unbind(bool DisableAttributes = true)
	    {
	        base.Bind(DisableAttributes);
	        GL.DisableVertexAttribArray(0);
	        GL.DisableVertexAttribArray(1);
	        GL.DisableVertexAttribArray(2);
        }
    }
	
	internal class VAO<T1, T2, T3, T4> : VAO where T1 : struct where T2 : struct where T3 : struct  where T4 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4)
        {
			GL.GenVertexArrays(1, out _id);
			GL.BindVertexArray(_id);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
			GL.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer4.BufferTarget, Buffer4.ID);
			GL.VertexAttribPointer(3, Buffer4.Size, Buffer4.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}

	    public override void Bind(bool EnableAttributes = true)
	    {
	        base.Bind(EnableAttributes);
	        GL.EnableVertexAttribArray(0);
	        GL.EnableVertexAttribArray(1);
	        GL.EnableVertexAttribArray(2);
	        GL.EnableVertexAttribArray(3);
        }

	    public override void Unbind(bool DisableAttributes = true)
	    {
	        base.Bind(DisableAttributes);
	        GL.DisableVertexAttribArray(0);
	        GL.DisableVertexAttribArray(1);
	        GL.DisableVertexAttribArray(2);
	        GL.DisableVertexAttribArray(3);
        }
    }
	
	internal class VAO<T1, T2, T3, T4, T5> : VAO where T1 : struct where T2 : struct where T3 : struct  where T4 : struct where T5 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4, VBO<T5> Buffer5)
        {
			GL.GenVertexArrays(1, out _id);
			GL.BindVertexArray(_id);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
			GL.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer4.BufferTarget, Buffer4.ID);
			GL.VertexAttribPointer(3, Buffer4.Size, Buffer4.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer5.BufferTarget, Buffer5.ID);
			GL.VertexAttribPointer(4, Buffer5.Size, Buffer5.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}

	    public override void Bind(bool EnableAttributes = true)
	    {
	        base.Bind(EnableAttributes);
	        GL.EnableVertexAttribArray(0);
	        GL.EnableVertexAttribArray(1);
	        GL.EnableVertexAttribArray(2);
	        GL.EnableVertexAttribArray(3);
	        GL.EnableVertexAttribArray(4);
        }

	    public override void Unbind(bool DisableAttributes = true)
	    {
	        base.Bind(DisableAttributes);
	        GL.DisableVertexAttribArray(0);
	        GL.DisableVertexAttribArray(1);
	        GL.DisableVertexAttribArray(2);
	        GL.DisableVertexAttribArray(3);
	        GL.DisableVertexAttribArray(4);
        }
    }
}
