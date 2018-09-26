/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 03:35 p.m.
 *
 */
using System;
using Hedra.Engine.Management;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// A Vertex Buffer Object.
	/// </summary>
	public class VBO<T> : IDisposable where T : struct
	{
		private bool Disposed;
		/// <summary>
		/// The ID of this VBO.
		/// </summary>
		public uint ID;
		
		/// <summary>
		/// The amount of elements.
		/// </summary>
		public int Count {get; private set;}
		
		/// <summary>
		/// The amount of values per element. 4 = Vector4, 3 = Vector3, etc.
		/// </summary>
		public int Size {get; private set;}
		
		/// <summary>
		/// The size in bytes of the elements in the VBO.
		/// </summary>
		public int SizeInBytes {get; private set;}
		
		/// <summary>
		/// The BufferTarget this VBO is bound to.
		/// </summary>
		public BufferTarget BufferTarget { get; private set; }
		
		/// <summary>
		/// The VertexAttribPointerType of this VBO.
		/// </summary>
        public VertexAttribPointerType PointerType { get; private set; }
        
        /// <summary>
        /// The hint to use when uploading data to the buffer.
        /// </summary>
        public BufferUsageHint Hint {get; private set;}
		
        /// <summary>
        /// Creates a and builds new Vertex Buffer Object from the following parameters.
        /// </summary>
        /// <param name="Data">The T[] of Data to be used.</param>
        /// <param name="SizeInBytes">The size in bytes of the elements.</param>
        /// <param name="PointerType">The VertexAttribPointerType used in this VBO</param>
        /// <param name="Target">The BufferTarget where the VBO should be bind.</param>
        /// <param name="Hint">The BufferUsageHint this VBO should use.</param>
        public VBO(T[] Data, int SizeInBytes, VertexAttribPointerType PointerType, BufferTarget BufferTarget = BufferTarget.ArrayBuffer, BufferUsageHint Hint = BufferUsageHint.StaticDraw){
        	
			Renderer.GenBuffers(1, out ID);
			Renderer.BindBuffer(BufferTarget, ID);
			Renderer.BufferData(BufferTarget, (IntPtr)(SizeInBytes), Data, Hint);
			
			this.Size = (Data is Vector4[]) ? 4 : (Data is Vector3[]) ?  3 : (Data is Vector2[]) ? 2 : 1;
			this.BufferTarget = BufferTarget;
			this.PointerType = PointerType;
			this.SizeInBytes = SizeInBytes;
			this.Count = Data.Length;
			this.Hint = Hint;
        }
        
        /// <summary>
        /// Appends Data to the end of the buffer
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="SizeInBytes"></param>
        public void Add(T[] Data, int SizeInBytes){
        	Renderer.BindBuffer(BufferTarget, ID);
        	Renderer.BufferSubData(BufferTarget, (IntPtr) (this.SizeInBytes), (IntPtr)(SizeInBytes), Data);
			this.Count += Data.Length;
			this.SizeInBytes += SizeInBytes;
        }
        
        public void Update(T[] Data, int SizeInBytes){
        	Renderer.BindBuffer(BufferTarget, ID);
			Renderer.BufferData(BufferTarget, (IntPtr)(SizeInBytes), Data, Hint);
			this.Count = Data.Length;
			this.SizeInBytes = SizeInBytes;
        }
        
        /// <summary>
        /// Deletes all the data from the video card. It is automatically called at the end of the program.
        /// </summary>
		public void Dispose(){
        	if(!Disposed){
        		Disposed = true;
				Executer.ExecuteOnMainThread( () => Renderer.DeleteBuffers(1, ref ID) );
        	}
		}
	}
}
