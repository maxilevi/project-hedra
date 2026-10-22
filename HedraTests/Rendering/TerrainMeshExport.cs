using System;
using System.IO;
using System.Numerics;
using Hedra;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [NonParallelizable]
    public class TerrainMeshExport
    {
        [TestCase("flat")]
        [TestCase("hills")]
        [TestCase("caves")]
        [TestCase("materials")]
        public void ExportSimplifierInputsWithoutStartingTheGame(string Shape)
        {
            var directory = Environment.GetEnvironmentVariable("HEDRA_MESH_EXPORT_DIRECTORY");
            if (string.IsNullOrEmpty(directory)) Assert.Ignore("Opt-in simplifier research inputs");
            Directory.CreateDirectory(directory);
            GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
            using var terrain = new HeadlessTerrainFixture(Shape);
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 128, Allocator.Megabyte * 4);
            foreach (var lod in new[] { 1, 2, 4, 8 })
            {
                Utils.Rng = new Random(43);
                var output = terrain.RawMesh(allocator, lod, true);
                try
                {
                    var mesh = output.StaticData;
                    mesh.UniqueVertices();
                    var border = new ChunkMeshBorderDetector().ProcessEntireBorder(mesh, Vector3.Zero, new Vector3(128, 0, 128));
                    using var writer = new BinaryWriter(File.Create(Path.Combine(directory, $"{Shape}-lod{lod}.bin")));
                    writer.Write(0x484d4553);
                    writer.Write(1);
                    writer.Write(mesh.Vertices.Count);
                    writer.Write(mesh.Indices.Count);
                    writer.Write(border.Length);
                    for (var i = 0; i < mesh.Vertices.Count; i++)
                    {
                        var p = mesh.Vertices[i];
                        var c = mesh.Colors[i];
                        writer.Write(p.X); writer.Write(p.Y); writer.Write(p.Z);
                        writer.Write(c.X); writer.Write(c.Y); writer.Write(c.Z); writer.Write(c.W);
                    }
                    foreach (var index in mesh.Indices) writer.Write(index);
                    foreach (var index in border) writer.Write(index);
                    TestContext.Progress.WriteLine($"MESH_EXPORT shape={Shape} lod={lod} vertices={mesh.Vertices.Count} indices={mesh.Indices.Count} locks={border.Length}");
                }
                finally { output.Dispose(); }
            }
        }
    }
}
