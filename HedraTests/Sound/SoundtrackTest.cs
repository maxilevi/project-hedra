using System;
using System.IO;
using Hedra;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Sound;
using NUnit.Framework;

namespace HedraTests.Sound
{
    [TestFixture]
    public class SoundtrackTest
    {

        [Test]
        public void TestSoundtrackRuns()
        {
            World.Provider = new SimpleWorldProviderMock();
            AssetManager.Provider = new DummyAssetProvider();
            SoundtrackManager.Load(null);
        }
    }
}