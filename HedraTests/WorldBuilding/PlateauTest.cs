using System.Collections.Generic;
using Hedra;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using Moq;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.WorldBuilding
{
    [TestFixture]
    public class PlateauTest
    {
        private float _height;
        
        [SetUp]
        public void Setup()
        {
            BlockType blockType;
            var designMock = new Mock<BiomeGenerationDesign>();
            designMock.Setup(B =>
                    B.GetHeight(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<Dictionary<Vector2, float[]>>(), out blockType))
                .Returns(_height);
            var generationMock = new RegionGeneration(0, designMock.Object);
            var regionMock = new Region
            {
                Generation = generationMock
            };
            var biomePoolMock = new Mock<IBiomePool>();
            biomePoolMock.Setup(B => B.GetRegion(It.IsAny<Vector3>())).Returns(regionMock);
            var provider = new SimpleWorldProviderMock
            {
                BiomePool = biomePoolMock.Object
            };
            World.Provider = provider;
        }
        
        [Test]
        public void TestRoundedPlateauFalloff()
        {
            var radius = Utils.Rng.NextFloat() * 1024 + 1;
            var rounded = new RoundedPlateau(Vector2.Zero, radius);
            Assert.AreEqual(1, rounded.Density(Vector2.Zero));
            Assert.Greater(rounded.Density(Vector2.UnitX * radius * .9f), 0);
            Assert.AreEqual(0, rounded.Density(Vector2.UnitX * radius));
        }
        
        [Test]
        public void TestSquaredPlateauFalloff()
        {
            var diameter = Utils.Rng.NextFloat() * 1024 + 1;
            var squared = new SquaredPlateau(Vector2.Zero, diameter);
            Assert.AreEqual(1, squared.Density(Vector2.Zero));
            Assert.Greater(squared.Density(Vector2.UnitX * (diameter * .5f)), 0);
            Assert.AreEqual(0, squared.Density(Vector2.UnitX * diameter));
        }
        
        [Test]
        public void TestBoundingBox()
        {
            var radius = Utils.Rng.NextFloat() * 1024 + 20;
            var rounded = new RoundedPlateau(Vector2.Zero, radius);
            var box = rounded.ToBoundingBox();
            var pos = Vector2.UnitX * (radius - 5);
            var b = box.Density(pos);
            Assert.Greater(b, rounded.Density(pos));
        }
    }
}