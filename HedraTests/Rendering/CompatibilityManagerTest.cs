using System;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Moq;
using NUnit.Framework;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;
using Hedra.Game;

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

            GameSettings.TestingMode = true;
            VBOCache.Clear();
            Renderer.Provider = glMock.Object;
            CompatibilityManager.Load();

            CompatibilityManager.MultiDrawElementsMethod(PrimitiveType.Triangles, new uint[5], DrawElementsType.UnsignedInt, new IntPtr[5] , 5);            
            Assert.AreEqual(5, timesCalled);
        }
        
        [Test]
        public void TestMultiDrawIsEnabledIfNoErrors()
        {
            var glMock = new DummyGLProvider();

            GameSettings.TestingMode = true;
            VBOCache.Clear();
            Renderer.Provider = glMock;
            CompatibilityManager.Load();
            Assert.AreEqual(1, glMock.MultiDrawTimesCalled);
            
            CompatibilityManager.MultiDrawElementsMethod(PrimitiveType.Triangles, new uint[5], DrawElementsType.UnsignedInt, new IntPtr[5] , 5);            
            Assert.AreEqual(2, glMock.MultiDrawTimesCalled);
        }
    }
}