using System.Collections.Generic;
using Hedra;
using Hedra.Core;
using Hedra.Engine.Generation;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Generation
{
    [TestFixture]
    public class BlockTest
    {

        [Test]
        public void TestDefaultValues()
        {
            var block = new Block();
            Assert.AreEqual(BlockType.Air, block.Type);
            Assert.AreEqual(0, block.Density, 0.01);
        }

        [TestCaseSource(nameof(RandomBlocks))]
        public void TestDensity(TestTuple Tuple)
        {
            Assert.AreEqual(Tuple.ExpectedDensity, Tuple.Block.Density, 0.02);
        }

        [TestCaseSource(nameof(RandomBlocks))]
        public void TestBlockType(TestTuple Tuple)
        {
            Assert.AreEqual(Tuple.ExpectedType, Tuple.Block.Type);
        }

        private static IEnumerable<TestTuple> RandomBlocks()
        {
            for (var i = 0; i < 5; i++)
            {
                var type = (BlockType) Utils.Rng.Next(0, (int) BlockType.MaxNums);
                var density = new Half(Utils.Rng.NextFloat() * 20f - 10f);
                yield return new TestTuple(
                    new Block(type, density),
                    type,
                    density
                );
            }
        }

        public class TestTuple
        {
            public readonly Block Block;
            public readonly BlockType ExpectedType;
            public readonly Half ExpectedDensity;

            public TestTuple(Block Block, BlockType ExpectedType, Half ExpectedDensity)
            {
                this.Block = Block;
                this.ExpectedType = ExpectedType;
                this.ExpectedDensity = ExpectedDensity;
            }
        }
    }
}