using System;
using System.Runtime.InteropServices;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    public abstract class UBO : GLObject<UBO>
    {
        protected static int BufferObjectBinding;
    }
    
    public sealed class UBO<T> : UBO where T : unmanaged
    {
        public override uint Id => _id;
        private readonly int _size;
        private readonly int _bindingPoint;
        private readonly string _uniformName;
        private uint _id;
        private T _lastValue;

        public UBO(string UniformName)
        {
            _bindingPoint = BufferObjectBinding++;
            _uniformName = UniformName;
            _size = Marshal.SizeOf(default(T));
            Renderer.GenBuffers(1, out _id);
            Renderer.BindBuffer(BufferTarget.UniformBuffer, Id);
            Renderer.BufferData(BufferTarget.UniformBuffer, (IntPtr) _size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            Renderer.BindBufferBase(BufferRangeTarget.UniformBuffer, _bindingPoint, (int) Id);
        }

        public void RegisterShader(Shader Entry)
        {
            void BindToShader()
            {
                var index = Renderer.GetUniformBlockIndex(Entry.ShaderId, _uniformName);
                if(index != -1)
                    Renderer.UniformBlockBinding(Entry.ShaderId, index, _bindingPoint);
            }
            BindToShader();
            Entry.ShaderReloaded += BindToShader;
        }

        public void Update(T Data)
        {
            if(Data.Equals(_lastValue)) return;
            Renderer.BindBuffer(BufferTarget.UniformBuffer, Id);
            Renderer.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)_size, ref Data);
            Renderer.BindBuffer(BufferTarget.UniformBuffer, 0);
            _lastValue = Data;
        }

        public override void Dispose()
        {
            base.Dispose();
            Renderer.DeleteBuffers(1, ref _id);
        }
    }
}