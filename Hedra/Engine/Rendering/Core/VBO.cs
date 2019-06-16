/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 03:35 p.m.
 *
 */

using System;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering.Core
{
    /// <summary>
    /// A Vertex Buffer Object.
    /// </summary>
    public abstract class VBO : GLObject<VBO>
    {
        public abstract int Count { get; protected set; }
        public abstract int Stride { get; }
        public abstract int SizeInBytes { get; protected set; }
        public abstract Type ElementType { get; protected set; }
    }
    
    public sealed class VBO<T> : VBO where T : struct
    {
        private bool _disposed;
        private uint _id;

        /// <summary>
        /// The ID of this VBO.
        /// </summary>
        public override uint Id => _id;
        
        /// <summary>
        /// The amount of elements.
        /// </summary>
        public override int Count { get; protected set; }
        
        /// <summary>
        /// The amount of values per element. 4 = Vector4, 3 = Vector3, etc.
        /// </summary>
        public override int Stride { get; }
        
        /// <summary>
        /// The size in bytes of the elements in the VBO.
        /// </summary>
        public override int SizeInBytes { get; protected set; }
        
        /// <summary>
        /// The BufferTarget this VBO is bound to.
        /// </summary>
        public BufferTarget BufferTarget { get; }
        
        /// <summary>
        /// The VertexAttribPointerType of this VBO.
        /// </summary>
        public VertexAttribPointerType PointerType { get; }
        
        /// <summary>
        /// The hint to use when uploading data to the buffer.
        /// </summary>
        public BufferUsageHint Hint { get; }

        public override Type ElementType { get; protected set; }

        /// <summary>
        /// Creates a and builds new Vertex Buffer Object from the following parameters.
        /// </summary>
        /// <param name="Data">The T[] of Data to be used.</param>
        /// <param name="SizeInBytes">The size in bytes of the elements.</param>
        /// <param name="PointerType">The VertexAttribPointerType used in this VBO</param>
        /// <param name="Target">The BufferTarget where the VBO should be bind.</param>
        /// <param name="Hint">The BufferUsageHint this VBO should use.</param>
        public VBO(T[] Data, int SizeInBytes, VertexAttribPointerType PointerType, BufferTarget BufferTarget = BufferTarget.ArrayBuffer, BufferUsageHint Hint = BufferUsageHint.StaticDraw)
        {
            this.Stride = Data is Vector4[] ? 4 : Data is Vector3[] ?  3 : Data is Vector2[] ? 2 : 1;
            this.ElementType = Data.GetType().GetElementType();
            this.BufferTarget = BufferTarget;
            this.PointerType = PointerType;
            this.Hint = Hint;
            
            Renderer.GenBuffers(1, out _id);
            Update(Data, SizeInBytes);
        }

        public void Update(T[] Data, int Bytes)
        {
            Bind();
            Renderer.BufferData(BufferTarget, (IntPtr) Bytes, IntPtr.Zero, Hint);
            Renderer.BufferSubData(BufferTarget, IntPtr.Zero, (IntPtr) Bytes, Data);
            Unbind();
            Count = Data.Length;
            SizeInBytes = Bytes;
        }

        public void Update(T[] Data, int Offset, int Bytes)
        {
            if(Offset + Bytes > SizeInBytes)
                throw new ArgumentOutOfRangeException($"Provided data '{Offset + Bytes}' exceeds buffer size of '{SizeInBytes}'");
            
            if(Data.GetType().GetElementType() != ElementType)
                throw new ArgumentOutOfRangeException($"Cannot change element type of VBO from '{ElementType}' to '{Data.GetType().GetElementType()}'");
            
            Bind();
            Renderer.BufferSubData(BufferTarget, (IntPtr)Offset, (IntPtr)Bytes, Data);
            Unbind();
        }

        public void Bind()
        {
            Renderer.BindBuffer(BufferTarget, Id);
        }

        public void Unbind()
        {
            Renderer.BindBuffer(BufferTarget, 0);
        }
        
        /// <summary>
        /// Deletes all the data from the video card. It is automatically called at the end of the program.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            base.Dispose();
            _disposed = true;
            Executer.ExecuteOnMainThread( () => Renderer.DeleteBuffers(1, ref _id) );
        }
    }
}