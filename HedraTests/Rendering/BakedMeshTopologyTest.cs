using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Rendering;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    public class BakedMeshTopologyTest
    {
        [Test]
        public void TransformedRecoloredPartsExactlyMatchRuntimeComponents()
        {
            var source = CreateModel();
            source.Indices.AddRange(source.Indices.Take(3).ToArray());
            var baked = BakedMeshTopology.Compile(source);
            Assert.IsTrue(baked.Matches(source));
            var transformed = source.Clone();
            for (var i = 0; i < transformed.Colors.Count; i++)
                transformed.Colors[i] *= 0.4f + transformed.Vertices[i].Y * 0.02f;
            transformed.Transform(Matrix4x4.CreateScale(1.1f) * Matrix4x4.CreateRotationY(1.37f) *
                                  Matrix4x4.CreateTranslation(4992, 128, 4992));
            Assert.IsTrue(baked.TryAssemble(source, transformed, out var parts));
            var expected = transformed.Ungroup();
            Assert.AreEqual(expected.Length, parts.Length);
            for (var i = 0; i < parts.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i].Vertices, parts[i].Vertices);
                CollectionAssert.AreEqual(expected[i].Colors, parts[i].Colors);
                CollectionAssert.AreEqual(expected[i].Normals, parts[i].Normals);
                CollectionAssert.AreEqual(expected[i].Indices, parts[i].Indices);
            }
        }

        [Test]
        public void ChangedSourceInvalidatesBakedTopology()
        {
            var source = CreateModel();
            var baked = BakedMeshTopology.Compile(source);
            source.Vertices[0] += Vector3.UnitX;
            Assert.IsFalse(baked.Matches(source));
        }

        [Test]
        public void RoundingThatMergesSourcePositionsRepairsOnlyTheAffectedPiece()
        {
            var source = CreateModel();
            source.Vertices[1] = source.Vertices[0] + Vector3.UnitX * 0.00001f;
            var baked = BakedMeshTopology.Compile(source);
            var transformed = source.Clone().Transform(Matrix4x4.CreateTranslation(5000, 0, 5000));
            AssertMatchesReference(baked, source, transformed);
        }

        [Test]
        public void RecoloringThatMergesAttributeSeamsPreservesTheReference()
        {
            var source = CreateModel();
            source.Vertices[3] = source.Vertices[0];
            source.Colors[3] = Vector4.UnitX;
            var baked = BakedMeshTopology.Compile(source);
            var transformed = source.Clone();
            transformed.Colors[3] = transformed.Colors[0];
            AssertMatchesReference(baked, source, transformed);
        }

        [Test]
        public void DuplicateAttributesPreserveSignedZeroBitsForNativeRemapping()
        {
            var source = CreateModel();
            source.Vertices[3] = source.Vertices[0];
            source.Normals[3] = new Vector3(0, System.BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)), 1);
            source.Indices = new uint[] { 3, 4, 5 }.ToList();
            var baked = BakedMeshTopology.Compile(source);
            Assert.IsTrue(baked.TryAssemble(source, source.Clone(), out var parts));
            var reference = source.Ungroup();
            CollectionAssert.AreEqual(reference[0].Normals.Select(n => System.BitConverter.SingleToInt32Bits(n.Y)),
                parts[0].Normals.Select(n => System.BitConverter.SingleToInt32Bits(n.Y)));
        }

        [Test]
        public void ManyMergedPositionsAndRemovedAttributeDuplicatesPreserveBfsOrder()
        {
            var source = new VertexData();
            var rng = new System.Random(43);
            for (var i = 0; i < 700; i++)
            for (var p = 0; p < 3; p++)
            {
                source.Indices.Add((uint)source.Vertices.Count);
                source.Vertices.Add(new Vector3(rng.Next(0, 8), rng.Next(0, 8), rng.Next(0, 8)) +
                                    Vector3.UnitX * (rng.Next(0, 2) * 0.00001f));
                source.Colors.Add(i % 2 == 0 ? Vector4.One : Vector4.UnitX);
                source.Normals.Add(Vector3.UnitY);
            }
            var baked = BakedMeshTopology.Compile(source);
            var transformed = source.Clone().Transform(Matrix4x4.CreateTranslation(5000, 0, 5000));
            transformed.Colors = Enumerable.Repeat(Vector4.One, transformed.Vertices.Count).ToList();
            AssertMatchesReference(baked, source, transformed);
        }

        [Test]
        public void RoundingThatConnectsPreviouslySeparatePiecesPreservesTriangleOrder()
        {
            var source = CreateModel();
            source.Vertices[3] = source.Vertices[0] + Vector3.UnitX * 0.00001f;
            var baked = BakedMeshTopology.Compile(source);
            Assert.AreEqual(2, baked.Parts.Length);
            var transformed = source.Clone().Transform(Matrix4x4.CreateTranslation(5000, 0, 5000));
            Assert.AreEqual(1, transformed.Ungroup().Length);
            AssertMatchesReference(baked, source, transformed);
        }

        private static void AssertMatchesReference(BakedMeshTopology Baked, VertexData Source, VertexData Transformed)
        {
            Assert.IsTrue(Baked.TryAssemble(Source, Transformed, out var parts));
            var expected = Transformed.Ungroup();
            Assert.AreEqual(expected.Length, parts.Length);
            for (var i = 0; i < parts.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i].Vertices, parts[i].Vertices);
                CollectionAssert.AreEqual(expected[i].Colors, parts[i].Colors);
                CollectionAssert.AreEqual(expected[i].Normals, parts[i].Normals);
                CollectionAssert.AreEqual(expected[i].Indices, parts[i].Indices);
            }
        }

        private static VertexData CreateModel() => new()
        {
            Vertices = new[] { Vector3.Zero, Vector3.UnitX, Vector3.UnitY,
                new Vector3(20, 10, 3), new Vector3(21, 10, 3), new Vector3(20, 11, 3) }.ToList(),
            Normals = Enumerable.Repeat(Vector3.UnitZ, 6).ToList(),
            Colors = Enumerable.Repeat(Vector4.One, 6).ToList(),
            Indices = new uint[] { 0, 1, 2, 3, 4, 5 }.ToList()
        };
    }
}
