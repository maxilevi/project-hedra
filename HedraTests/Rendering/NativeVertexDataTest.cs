using System;
using System.Diagnostics;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Numerics;
using Hedra.Framework;
using Hedra.Numerics;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class NativeVertexDataTest
    {
        [Test]
        public void TestAdding()
        {
            using (var allocator = new HeapAllocator(Allocator.Megabyte))
            {
                var rng = new Random();
                var count = 1024;
                var vertices = GetAsArray(count, () => new Vector3(rng.NextFloat(), rng.NextFloat(), rng.NextFloat()));
                var normals = GetAsArray(count, () => new Vector3(rng.NextFloat(), rng.NextFloat(), rng.NextFloat()));
                var extradata = GetAsArray(count, () => rng.NextFloat());
                var colors = GetAsArray(count, () => new Vector4(rng.NextFloat(), rng.NextFloat(), rng.NextFloat(), rng.NextFloat()));


                var vertexData = new NativeVertexData(allocator);
                vertexData.Vertices.AddRange(vertices);
                vertexData.Colors.AddRange(colors);
                vertexData.Normals.AddRange(normals);
                vertexData.Extradata.AddRange(extradata);

                Assert.AreEqual(1024, vertexData.Vertices.Count);
                Assert.AreEqual(1024, vertexData.Colors.Count);
                Assert.AreEqual(1024, vertexData.Normals.Count);
                Assert.AreEqual(1024, vertexData.Extradata.Count);
                for (var i = 0; i < count; ++i)
                {
                    Assert.AreEqual(vertices[i], vertexData.Vertices[i]);
                    Assert.AreEqual(colors[i], vertexData.Colors[i]);
                    Assert.AreEqual(normals[i], vertexData.Normals[i]);
                    Assert.AreEqual(extradata[i], vertexData.Extradata[i]);
                }
            }
        }

        [Test]
        public void TestVolume()
        {
            using (var allocator = new HeapAllocator(Allocator.Megabyte * 64))
            {
                var rng = new Random();
                var count = 1000000;
                var vertices = GetAsArray(count, () => new Vector3(rng.NextFloat(), rng.NextFloat(), rng.NextFloat()));
                var normals = GetAsArray(count, () => new Vector3(rng.NextFloat(), rng.NextFloat(), rng.NextFloat()));
                var extradata = GetAsArray(count, () => rng.NextFloat());
                var colors = GetAsArray(count,
                    () => new Vector4(rng.NextFloat(), rng.NextFloat(), rng.NextFloat(), rng.NextFloat()));

                var watch = new Stopwatch();
                var nativeTime = 0L;
                var normalTime = 0L;

                watch.Start();
                var nativeVertexData = new NativeVertexData(allocator);
                for (var i = 0; i < count; ++i)
                {
                    nativeVertexData.Vertices.Add(vertices[i]);
                    nativeVertexData.Colors.Add(colors[i]);
                    nativeVertexData.Normals.Add(normals[i]);
                    nativeVertexData.Extradata.Add(extradata[i]);
                }
                watch.Stop();
                nativeTime = watch.ElapsedMilliseconds;
                TestContext.WriteLine($"Native took '{nativeTime}' MS");

                watch.Restart();
                var vertexData = new VertexData();
                for (var i = 0; i < count; ++i)
                {
                    vertexData.Vertices.Add(vertices[i]);
                    vertexData.Colors.Add(colors[i]);
                    vertexData.Normals.Add(normals[i]);
                    vertexData.Extradata.Add(extradata[i]);
                }
                watch.Stop();
                normalTime = watch.ElapsedMilliseconds;
                TestContext.WriteLine($"Normal took '{normalTime}' MS");
                
                Assert.AreEqual(count, nativeVertexData.Vertices.Count);
                Assert.AreEqual(count, nativeVertexData.Colors.Count);
                Assert.AreEqual(count, nativeVertexData.Normals.Count);
                Assert.AreEqual(count, nativeVertexData.Extradata.Count);
                for (var i = 0; i < count; ++i)
                {
                    Assert.AreEqual(vertices[i], nativeVertexData.Vertices[i]);
                    Assert.AreEqual(colors[i], nativeVertexData.Colors[i]);
                    Assert.AreEqual(normals[i], nativeVertexData.Normals[i]);
                    Assert.AreEqual(extradata[i], nativeVertexData.Extradata[i]);
                }
                
#if RELEASE
                Assert.Less(nativeTime, normalTime);
#endif
            }
        }

        private static T[] GetAsArray<T>(int Count, Func<T> Create)
        {
            var arr = new T[Count];
            for (var i = 0; i < arr.Length; ++i)
            {
                arr[i] = Create();
            }
            return arr;
        }
    }
}