using System;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Moq;
using NUnit.Framework;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class CompatibilityManagerTest
    {
        [SetUp]
        public void Setup()
        {
            AssetManager.Provider = new DummyAssetProvider();
            Renderer.Provider = new DummyGLProvider();
        }
        
        [Test]
        public void TestGeometryShadersAreDisabledIfFailure()
        {
            var glMock = new Mock<IGLProvider>();
            glMock.Setup(M => M.CreateProgram()).Returns(1);
            glMock.Setup(M => M.UseProgram(It.IsAny<uint>())).Throws(new RenderException("Test Exception"));
            Renderer.Provider = glMock.Object;
            
            CompatibilityManager.Load();
            Assert.False(CompatibilityManager.SupportsGeometryShaders);
        }
        
        [Test]
        public void TestGeometryShadersAreEnabledIfNoErrors()
        {
            CompatibilityManager.Load();
            Assert.True(CompatibilityManager.SupportsGeometryShaders);
        }
        
        [Test]
        public void TestMultiDrawIsDisabledIfFailure()
        {
            var glMock = new Mock<IGLProvider>();
            
            glMock.Setup(M => M.MultiDrawElements(It.IsAny<PrimitiveType>(), It.IsAny<uint[]>(),
                    It.IsAny<DrawElementsType>(), It.IsAny<IntPtr[]>(), It.IsAny<int>()))
                .Throws(new RenderException("Test Exception"));

            var timesCalled = 0;
            glMock.Setup(M => M.DrawElements(It.IsAny<PrimitiveType>(), It.IsAny<int>(), It.IsAny<DrawElementsType>(),
                It.IsAny<IntPtr>())).Callback(() => ++timesCalled);
            
            Renderer.Provider = glMock.Object;
            CompatibilityManager.Load();

            CompatibilityManager.MultiDrawElementsMethod(PrimitiveType.Triangles, new uint[5], DrawElementsType.UnsignedInt, new IntPtr[5] , 5);            
            Assert.AreEqual(5, timesCalled);
        }
        
        [Test]
        public void TestMultiDrawIsEnabledIfNoErrors()
        {
            var glMock = new Mock<IGLProvider>();
            
            var timesCalled = 0;
            glMock.Setup(M => M.MultiDrawElements(It.IsAny<PrimitiveType>(), It.IsAny<uint[]>(),
                    It.IsAny<DrawElementsType>(), It.IsAny<IntPtr[]>(), It.IsAny<int>())).Callback( () => ++timesCalled);

            glMock.Setup(M => M.GetInteger(It.IsAny<GetPName>())).Returns(Shader.Passthrough.ShaderId);
            
            Renderer.Provider = glMock.Object;
            CompatibilityManager.Load();
            Assert.AreEqual(1, timesCalled);
            
            CompatibilityManager.MultiDrawElementsMethod(PrimitiveType.Triangles, new uint[5], DrawElementsType.UnsignedInt, new IntPtr[5] , 5);            
            Assert.AreEqual(2, timesCalled);
        }
    }
}