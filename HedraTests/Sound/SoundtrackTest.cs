using System;
using System.IO;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
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
            SoundtrackManager.Load(null);
            SoundtrackManager.PlayTrack(0);
            Assert.Catch(typeof(NullReferenceException), SoundtrackManager.Update);
        }
    }
}