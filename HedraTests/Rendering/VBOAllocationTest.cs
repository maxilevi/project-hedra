using System;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Moq;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class VBOAllocationTest
    {
        private Mock<IGLProvider> _gl;

        [SetUp]
        public void Setup()
        {
            GameSettings.TestingMode = true;
            VBOCache.Clear();
            _gl = new Mock<IGLProvider>();
            Renderer.Provider = _gl.Object;
        }

        [Test]
        public void EmptyArrayAllocatesStorageWithoutReadingNullPointer()
        {
            VBOCache.Create(Array.Empty<uint>(), 4096, VertexAttribPointerType.UnsignedInt,
                BufferTarget.ElementArrayBuffer, BufferUsageHint.DynamicDraw, out _);
            _gl.Verify(g => g.BufferData(BufferTarget.CopyWriteBuffer, (IntPtr)4096,
                IntPtr.Zero, BufferUsageHint.DynamicDraw), Times.Once);
            _gl.Verify(g => g.BufferSubData(It.IsAny<BufferTarget>(), It.IsAny<IntPtr>(),
                It.IsAny<IntPtr>(), It.IsAny<uint[]>()), Times.Never);
        }

        [Test]
        public void UploadReadsOnlySuppliedDataWhenStorageIsLarger()
        {
            var data = new uint[] { 1, 2, 3 };
            VBOCache.Create(data, 4096, VertexAttribPointerType.UnsignedInt,
                BufferTarget.ElementArrayBuffer, BufferUsageHint.DynamicDraw, out _);
            _gl.Verify(g => g.BufferSubData(BufferTarget.CopyWriteBuffer, IntPtr.Zero,
                (IntPtr)12, data), Times.Once);
            _gl.Verify(g => g.BindBuffer(BufferTarget.ElementArrayBuffer, It.IsAny<uint>()), Times.Never);
        }
        [Test]
        public void DisablingAnAttributeCallsTheNativeDisableOperation()
        {
            Renderer.BindShader(1);
            var handler = new VertexAttributeHandler();
            handler.Bind(1);
            handler.Enable(0);
            handler.Disable(0);
            Assert.False(handler.IsEnabled(0));
            _gl.Verify(g => g.DisableVertexAttribArray(0), Times.Once);
            _gl.Verify(g => g.EnableVertexAttribArray(0), Times.Once);
            Renderer.BindShader(0);
        }

    }
}
