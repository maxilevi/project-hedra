using System.Collections.Generic;
using System.Linq;
using Hedra;
using Hedra.Core;
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
            AssetManager.Provider = new DummyAssetProvider();
            Provider = new CacheProvider();
            Provider.Load();
        }

        [Test]
        public void TestExtraDataCacheHashing()
        {
            var list = new List<float>();
            for(var i = 0; i < 1000; ++i)
                list.Add(Utils.Rng.NextFloat());
            var hash1 = Provider.MakeHash(list);
            var hash2 = Provider.MakeHash(list);
            Assert.AreEqual(hash1, hash2);         
        }
        
        [Test]
        public void TestColorsCacheHashing()
        {
            var list = new List<Vector4>(new []
            {
                new Vector4(.323f, 12.3312f, 23.32f, 21.33333f),
                new Vector4(.4423f, 122.3367812f, 23124.12321332f, 21.213f), 
                new Vector4(.64323f, 12132.38312f, 23.31232f, 21.213f),
                new Vector4(.33f, 18762.33812f, 23.332f, 21.21443f)
            });
            var hash1 = Provider.MakeHash(list);
            var hash2 = Provider.MakeHash(list);
            Assert.AreEqual(hash1, hash2);         
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
            for (var i = 0; i < Utils.Rng.Next(1, 21); i++)
            {
                colorList.Add(new Vector4(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()));
            }
            var extradataList = new List<float>();
            for (var i = 0; i < Utils.Rng.Next(1, 21); i++)
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