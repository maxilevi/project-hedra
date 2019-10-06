using Hedra.Engine.Core;
using NUnit.Framework;

namespace HedraTests.Core
{
    [TestFixture]
    public class AllocatorTest
    {
        [Test]
        public unsafe void AllocateTest()
        {
            using (var allocator = new HeapAllocator(Allocator.Megabyte))
            {
                Assert.AreEqual(allocator.TotalMemory, Allocator.Megabyte);
                Assert.AreEqual(allocator.FreeMemory, Allocator.Megabyte);
                Assert.AreEqual(allocator.UsedMemory, 0);
                var ptr = allocator.Get<int>(16);
                Assert.AreEqual(allocator.UsedMemory, sizeof(int) * 16);
                Assert.AreEqual(allocator.TotalMemory, Allocator.Megabyte);
                Assert.AreEqual(allocator.FreeMemory, Allocator.Megabyte - sizeof(int) * 16);
                allocator.Free(ref ptr);
                Assert.AreEqual(allocator.UsedMemory, 0);
                Assert.AreEqual(allocator.TotalMemory, Allocator.Megabyte);
                Assert.AreEqual(allocator.FreeMemory, Allocator.Megabyte);
            }
        }
    }
}