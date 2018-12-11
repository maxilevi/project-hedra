using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Hedra;
using Hedra.Engine;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Engine.Rendering.UI;
using HedraTests.Rendering;
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
            GameSettings.TestingMode = true;
            EventProvider = new SimpleEventProvider();
            EventDispatcher.Provider = EventProvider;
            MockEngine();
            EventDispatcher.Clear();
        }

        protected static void MockEngine()
        {
            GameSettings.TestingMode = true;
            World.Provider = new SimpleWorldProviderMock();
            AssetManager.Provider = new SimpleAssetProvider();
            Graphics2D.Provider = new SimpleTexture2DProviderMock();
            Renderer.Provider = new SimpleGLProviderMock();
            AnimationLoader.Provider = new SimpleAnimationProvider();
            ColladaLoader.Provider = new SimpleColladaProvider();
            GameManager.Provider = new SimpleGameProviderMock();
            GUIText.Provider = new SimpleTextProviderMock();
        }

        [TearDown]
        public virtual void Teardown()
        {
        }
    }
}