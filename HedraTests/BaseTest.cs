using Hedra;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Bullet;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;

using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Mission;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.Sound;
using HedraTests.Rendering;
using Moq;
using NUnit.Framework;
using AnimationLoader = Hedra.Engine.Rendering.Animation.AnimationLoader;

namespace HedraTests
{
    public abstract class BaseTest
    {
        private static bool _translationsLoaded;
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
            Time.RegisterThread();
            SoundPlayer.Provider = new DummySoundProvider();
            World.Provider = new SimpleWorldProviderMock();
            AssetManager.Provider = new DummyAssetProvider();
            Graphics2D.Provider = new SimpleTexture2DProviderMock();
            Renderer.Provider = new DummyGLProvider();
            VBOCache.Clear();
            Executer.Clear();
            AnimationLoader.Provider = new SimpleAnimationProvider();
            GameManager.Provider = new SimpleGameProviderMock();
            GUIText.Provider = new SimpleTextProviderMock();
            ClassLoader.LoadModules(GameLoader.AppPath);
            Program.GameWindow = new SimpleHedraWindowMock();
            GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
            if(!_translationsLoaded) Translations.Load();
            _translationsLoaded = true;
            GameSettings.Width = 1920;
            GameSettings.Height = 1080;
            BulletPhysics.Load();
            LandformLoader.Load();
            BackgroundUpdater.Load();
            MissionPool.Load();
        }

        [TearDown]
        public virtual void Teardown()
        {
        }
    }
}