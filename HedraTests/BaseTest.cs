using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Moq;
using NUnit.Framework;
using AnimationLoader = Hedra.Engine.Rendering.Animation.AnimationLoader;

namespace HedraTests
{
    public abstract class BaseTest
    {
        protected SimpleEventProvider EventProvider { get; private set; }
        
        [SetUp]
        public virtual void Setup()
        {
            EventProvider = new SimpleEventProvider();
            World.Provider = new SimpleWorldProviderMock();
            EventDispatcher.Provider = EventProvider;
            AssetManager.Provider = new SimpleAssetProvider();
            Graphics2D.Provider = new SimpleTexture2DProviderMock();
            Renderer.Provider = new SimpleGLProviderMock();
            AnimationLoader.Provider = new SimpleAnimationProvider();
            ColladaLoader.Provider = new SimpleColladaProvider();
        }

        [TearDown]
        public virtual void Teardown()
        {
        }
    }
}