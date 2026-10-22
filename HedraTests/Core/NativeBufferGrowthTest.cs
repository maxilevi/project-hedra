using System;
using System.Linq;
using System.Reflection;
using Hedra.Framework;
using NUnit.Framework;

namespace HedraTests.Core
{
    public class NativeBufferGrowthTest
    {
        [Test]
        public void InPlaceGrowthExpandsThePhysicalHeapBeforePublishingTheNewLength()
        {
            using var allocator = new ResizableHeapAllocator(8192, 256);
            var array = new NativeArray<int>(allocator, 16);
            array[0] = 123;
            array.Resize(512);
            var size = (int)typeof(ResizableHeapAllocator).GetField("_currentSize", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(allocator)!;
            Assert.GreaterOrEqual(size, 512 * sizeof(int));
            Assert.AreEqual(123, array[0]);
            array[511] = 456;
            Assert.AreEqual(456, array[511]);
        }

        [Test]
        public void OtherAllocationsKeepTheirDataWhenTheBackingHeapMoves()
        {
            using var allocator = new ResizableHeapAllocator(16384, 64);
            var first = new NativeArray<int>(allocator, 4);
            first[0] = 7;
            first[3] = 9;
            var second = new NativeArray<int>(allocator, 2048);
            second[2047] = 111;
            Assert.AreEqual(7, first[0]);
            Assert.AreEqual(9, first[3]);
            Assert.AreEqual(111, second[2047]);
        }

        [Test]
        public void RelocationPreservesBothTheGrownArrayAndItsNeighbor()
        {
            using var allocator = new HeapAllocator(16384);
            var first = new NativeArray<int>(allocator, 16);
            var neighbor = new NativeArray<int>(allocator, 16);
            for (var i = 0; i < 16; i++) { first[i] = i + 100; neighbor[i] = i + 200; }
            first.Resize(1024);
            for (var i = 0; i < 16; i++)
            {
                Assert.AreEqual(i + 100, first[i]);
                Assert.AreEqual(i + 200, neighbor[i]);
            }
        }

        [Test]
        public void ClearReusesCapacityAndAppendHandlesRelocationAndSelfAppend()
        {
            using var allocator = new ResizableHeapAllocator(16384, 256);
            using var list = new NativeList<int>(allocator);
            list.AddRange(Enumerable.Range(0, 40).ToArray());
            var capacity = list.InternalArray.Length;
            list.Clear();
            Assert.AreEqual(capacity, list.InternalArray.Length);
            Assert.AreEqual(0, list.Count);
            list.AddRange(Enumerable.Range(0, 40));
            var neighbor = new NativeArray<int>(allocator, 64);
            neighbor[63] = 222;
            list.AddRange(list);
            CollectionAssert.AreEqual(Enumerable.Range(0, 40).Concat(Enumerable.Range(0, 40)), list);
            Assert.AreEqual(222, neighbor[63]);
        }

        [Test]
        public void ManagedAndBoxedNativeArraysAppendAcrossHeapGrowth()
        {
            using var allocator = new ResizableHeapAllocator(16384, 128);
            using var list = new NativeList<int>(allocator);
            list.Add(-1);
            list.AddRange(Enumerable.Range(0, 80).ToArray());
            var source = new NativeArray<int>(allocator, 80);
            for (var i = 0; i < source.Length; i++) source[i] = 100 + i;
            list.AddRange((System.Collections.Generic.IEnumerable<int>)source);
            list.AddRange(Enumerable.Range(200, 80).ToList());
            CollectionAssert.AreEqual(new[] { -1 }.Concat(Enumerable.Range(0, 80)).Concat(Enumerable.Range(100, 80))
                .Concat(Enumerable.Range(200, 80)), list);
            using var clone = list.Clone();
            CollectionAssert.AreEqual(list, clone);
            clone.Set(new[] { 9, 8, 7 });
            CollectionAssert.AreEqual(new[] { 9, 8, 7 }, clone);
        }

        [Test]
        public void BulkAppendPreservesExistingElementsAndCopiesOnlyTheRequestedPrefix()
        {
            using var allocator = new HeapAllocator(16384);
            using var list = new NativeList<int>(allocator);
            var source = new NativeArray<int>(allocator, 100);
            for (var i = 0; i < source.Length; i++) source[i] = i;
            list.Add(-1);
            list.AddRange(source, 70);
            CollectionAssert.AreEqual(new[] { -1 }.Concat(Enumerable.Range(0, 70)), list);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.AddRange(source, 101));
        }
    }
}
