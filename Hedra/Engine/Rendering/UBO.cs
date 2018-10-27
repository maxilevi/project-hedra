/*
 * Author: Zaphyk
 * Date: 14/03/2016
 * Time: 01:50 p.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of UBO.
    /// </summary>
    public class UBO<T> where T : struct
    {
        public uint UboID;
        public int Size;
        
        public UBO(int Size, int Index)
        {
            Renderer.GenBuffers(1, out UboID);
            
            Renderer.BindBuffer(BufferTarget.UniformBuffer, UboID);
            Renderer.BufferData(BufferTarget.UniformBuffer, (IntPtr) Size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            
            BindBlock(Index);
        }
        
        public void BindBlock(int Index)
        {
            Renderer.BindBuffer(BufferTarget.UniformBuffer, UboID);
            Renderer.BindBufferBase(BufferRangeTarget.UniformBuffer, Index, (int) UboID);
        }
        
        public void Update(T Data)
        {
            Renderer.BindBuffer(BufferTarget.UniformBuffer, UboID);
            Renderer.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr) Size, ref Data);
        }
    }
}
