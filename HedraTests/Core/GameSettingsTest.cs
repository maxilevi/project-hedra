using System.IO;
using Hedra.Engine;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Core
{
    [TestFixture]
    public class GameSettingsTest : BaseTest
    {
        private string _dummyConfig;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Hedra.Program.GameWindow = new SimpleHedraWindowMock();
            _dummyConfig = $"{Path.GetTempPath()}/hedra_test_settings.cfg";
            if(File.Exists(_dummyConfig))
                File.Delete(_dummyConfig);
        }
        
        [Test]
        public void TestGameSettingsSaveAndLoading()
        {
            GameSettings.MouseSensibility = 100;
            GameSettings.Save(_dummyConfig);
            GameSettings.MouseSensibility = 0;
            GameSettings.LoadAll(_dummyConfig);
            Assert.AreEqual(100, GameSettings.MouseSensibility);
        }
    }
}