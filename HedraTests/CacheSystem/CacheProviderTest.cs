using System.Collections.Generic;
using System.Linq;
using Hedra;
using Hedra.Engine;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.CacheSystem
{
    [TestFixture]
    public class CacheProviderTest
    {
        private CacheProvider Provider { get; set; }
        
        [SetUp]
        public void Setup()
        {
            AssetManager.Provider = new SimpleAssetProvider();
            Provider = new CacheProvider();
            Provider.Load();
        }
        
        [Test]
        public void TestUsesCache()
        {
            var data = BuildRandom();
            var newData = new InstanceData
            {
                Colors = new List<Vector4>(data.Colors),
                ExtraData = new List<float>(data.ExtraData)
            };
            Provider.Check(data);         
            Provider.Check(newData);
            
            Assert.AreEqual(data.ColorCache, newData.ColorCache);
            Assert.AreEqual(data.ExtraDataCache, newData.ExtraDataCache);
            
            Assert.AreNotEqual(-1, newData.ColorCache);
            Assert.AreNotEqual(-1, data.ColorCache);
            
            Assert.AreNotEqual(-1, newData.ExtraDataCache);
            Assert.AreNotEqual(-1, data.ExtraDataCache);
        }

        private InstanceData BuildRandom()
        {
            var colorList = new List<Vector4>();
            for (var i = 0; i < Utils.Rng.Next(0, 20); i++)
            {
                colorList.Add(new Vector4(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()));
            }
            var extradataList = new List<float>();
            for (var i = 0; i < Utils.Rng.Next(0, 20); i++)
            {
                extradataList.Add(Utils.Rng.NextFloat());
            }
            return new InstanceData
            {
                Colors = colorList,
                ExtraData = extradataList
            };
        }
    }
}