using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.StructureSystem;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [TestFixture]
    public class OffsetCheckerTest
    {
        [Test]
        public void RepeatedRangeRegistersOneReferencePerChunk()
        {
            var checker = new OffsetChecker();
            Assert.AreEqual(16, checker.RegisterRange(Vector2.Zero, -2, 2).Count);
            Assert.IsEmpty(checker.RegisterRange(Vector2.Zero, -2, 2));
            var removed = new List<Vector2>();
            checker.DeleteOffset(Vector2.Zero, removed.Add);
            Assert.AreEqual(16, removed.Count);
            Assert.AreEqual(16, checker.RegisterRange(Vector2.Zero, -2, 2).Count);
        }

        [Test]
        public void OverlappingRangeRemainsRegisteredUntilBothChunksLeave()
        {
            var checker = new OffsetChecker();
            var other = new Vector2(Chunk.Width, 0);
            Assert.AreEqual(16, checker.RegisterRange(Vector2.Zero, -2, 2).Count);
            Assert.AreEqual(4, checker.RegisterRange(other, -2, 2).Count);
            var removed = new List<Vector2>();
            checker.DeleteOffset(Vector2.Zero, removed.Add);
            Assert.AreEqual(4, removed.Count);
            checker.DeleteOffset(other, removed.Add);
            Assert.AreEqual(20, removed.Count);
        }

        [Test]
        public void LargerSearchRegistersOnlyItsNewBorder()
        {
            var checker = new OffsetChecker();
            Assert.AreEqual(16, checker.RegisterRange(Vector2.Zero, -2, 2).Count);
            Assert.AreEqual(48, checker.RegisterRange(Vector2.Zero, -4, 4).Count);
            Assert.IsEmpty(checker.RegisterRange(Vector2.Zero, -3, 3));
            var removed = new List<Vector2>();
            checker.DeleteOffset(Vector2.Zero, removed.Add);
            Assert.AreEqual(64, removed.Count);
        }

        [Test]
        public void OverlappingBuilderWaitsForPlacementToFinish()
        {
            var checker = new OffsetChecker();
            var positions = checker.RegisterRange(Vector2.Zero, -2, 2);
            using var entered = new ManualResetEventSlim();
            var waiter = Task.Run(() =>
            {
                checker.RegisterRange(Vector2.Zero, -2, 2);
                entered.Set();
                checker.WaitForRange(Vector2.Zero);
            });
            Assert.True(entered.Wait(3000));
            Assert.False(waiter.Wait(50), "Registration must not imply completed placement.");
            foreach (var position in positions) checker.CompletePosition(position);
            Assert.True(waiter.Wait(3000));
        }

        [Test]
        public void PlacementFailureReleasesDependentBuildersWithTheError()
        {
            var checker = new OffsetChecker();
            checker.RegisterRange(Vector2.Zero, -2, 2);
            checker.FailRange(Vector2.Zero, new InvalidOperationException("placement failed"));
            var waiter = Task.Run(() => checker.WaitForRange(Vector2.Zero));
            Assert.Throws<InvalidOperationException>(() => waiter.GetAwaiter().GetResult());
        }
    }
}
