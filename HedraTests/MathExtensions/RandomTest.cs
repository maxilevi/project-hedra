using System;
using Hedra;
using Hedra.Numerics;
using NUnit.Framework;

namespace HedraTests.MathExtensions
{
    [TestFixture]
    public class RandomTest
    {
        private int _seed;
        private Random _random;
        private IRandom _distribution;
        
        [SetUp]
        public void Setup()
        {
            _seed = new Random().Next();
            _random = new Random(_seed);
            _distribution = new RandomDistribution(_seed);
        }
        
        [Test]
        public void TestObjectsMatchNext()
        {
            int a, c;
            Assert.AreEqual(a = _random.Next(0, 100), c = _distribution.Next(0, 100),
                $"Result mismatch with seed '{_seed}' on Next(). Expected={a}, got Distribution={c}");
        }
        
        [Test]
        public void TestObjectsMatchNextDouble()
        {
            double a, c;
            Assert.AreEqual( a = (float)_random.NextDouble(), c = (float)_distribution.NextDouble(),
                $"Result mismatch with seed '{_seed}' on NextDouble(). Expected={a}, got Distribution={c}");
        }
        
        [Test]
        public void TestRandomDistributionCache()
        {
            var distribution = new RandomDistribution(true);
            distribution.Seed = _seed;
            var original = distribution.Next(0, 100);
            distribution.Seed = _seed;
            Assert.AreEqual(original, distribution.Next(0, 100));
        }
        
        [Test]
        public void TestRandomDistributionSeed()
        {
            var seed = Utils.Rng.Next();
            var distribution = new RandomDistribution(seed);
            var original = new Random(seed);
            Assert.AreEqual(original.Next(0, 100000), distribution.Next(0, 100000));
            Assert.AreEqual(original.Next(-123123, 345234), distribution.Next(-123123, 345234));
        }
    }
}