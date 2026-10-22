using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Scenes;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;

namespace Hedra.Engine.Generation
{
    public static class BakedGenerationAssets
    {
        private const int FormatVersion = 2;
        public const string FileName = "generation.bin";
        private sealed class Catalog
        {
            public readonly Dictionary<string, BakedMeshTopology> Models = new();
            public readonly Dictionary<string, (byte[] Hash, Vector3[] Points)> Waypoints = new();
        }
        private static readonly Lazy<Catalog> Assets = new(Load);
        private static readonly ConditionalWeakTable<VertexData, BakedMeshTopology> Models = new();

        public static void Register(CacheItem Type, int Index, VertexData Model)
        {
            if (Assets.Value.Models.TryGetValue($"{Type}:{Index}", out var topology) && topology.Matches(Model))
                Models.Add(Model, topology);
        }

        public static bool TrySplit(VertexData Original, VertexData Transformed, out VertexData[] Parts)
        {
            Parts = null;
            if (!Models.TryGetValue(Original, out var topology) || !topology.TryAssemble(Original, Transformed, out Parts))
                return false;
            if (Environment.GetEnvironmentVariable("HEDRA_VERIFY_BAKED_ASSETS") == "1")
            {
                var reference = Transformed.Ungroup();
                if (Parts.Length != reference.Length)
                    throw new InvalidOperationException($"Baked components differ: {Original.Name} parts={Parts.Length} reference={reference.Length}");
                for (var i = 0; i < Parts.Length; i++)
                {
                    var part = Parts[i];
                    var expected = reference[i];
                    if (!part.Vertices.SequenceEqual(expected.Vertices) || !part.Indices.SequenceEqual(expected.Indices) ||
                        !part.Normals.SequenceEqual(expected.Normals) || !part.Colors.SequenceEqual(expected.Colors))
                        throw new InvalidOperationException($"Baked components differ: {Original.Name} part={i} vertices={part.Vertices.Count}/{expected.Vertices.Count} indices={part.Indices.Count}/{expected.Indices.Count} positions={part.Vertices.SequenceEqual(expected.Vertices)} colors={part.Colors.SequenceEqual(expected.Colors)} normals={part.Normals.SequenceEqual(expected.Normals)} indexing={part.Indices.SequenceEqual(expected.Indices)} triangles={SameTriangles(part, expected)}");
                }
                Log.WriteLine($"GEN_BAKED_VERIFIED model={Original.Name} parts={Parts.Length}");
            }
            return true;
        }

        private static bool SameTriangles(VertexData Left, VertexData Right)
        {
            Dictionary<MeshAnalyzer.Triangle, int> Count(VertexData Model)
            {
                var counts = new Dictionary<MeshAnalyzer.Triangle, int>();
                MeshAnalyzer.Vertex Vertex(uint Index) => new(Model.Vertices[(int)Index], Model.Colors[(int)Index], Model.Normals[(int)Index]);
                for (var i = 0; i < Model.Indices.Count; i += 3)
                {
                    var triangle = new MeshAnalyzer.Triangle { P1 = Vertex(Model.Indices[i]), P2 = Vertex(Model.Indices[i + 1]), P3 = Vertex(Model.Indices[i + 2]) };
                    counts.TryGetValue(triangle, out var count);
                    counts[triangle] = count + 1;
                }
                return counts;
            }
            var left = Count(Left);
            var right = Count(Right);
            return left.Count == right.Count && left.All(pair => right.TryGetValue(pair.Key, out var count) && count == pair.Value);
        }

        public static bool TryGetWaypoints(string Path, VertexData Model, out Vector3[] Points)
        {
            Points = null;
            if (!Assets.Value.Waypoints.TryGetValue(Path, out var data) ||
                !data.Hash.SequenceEqual(BakedMeshTopology.Hash(Model))) return false;
            Points = data.Points;
            return true;
        }

        private static Catalog Load()
        {
            var catalog = new Catalog();
            var path = System.IO.Path.Combine(AssetManager.AppPath, FileName);
            if (!File.Exists(path)) return catalog;
            try
            {
                using var reader = new BinaryReader(File.OpenRead(path));
                if (reader.ReadString() != "HEDRA_GENERATION" || reader.ReadInt32() != FormatVersion) return catalog;
                var models = ReadCount(reader, 1000);
                for (var i = 0; i < models; i++)
                {
                    var key = reader.ReadString();
                    var hash = ReadHash(reader);
                    var parts = new int[ReadCount(reader, 1000000)][];
                    for (var p = 0; p < parts.Length; p++)
                    {
                        parts[p] = new int[ReadCount(reader, 10000000)];
                        for (var v = 0; v < parts[p].Length; v++) parts[p][v] = reader.ReadInt32();
                    }
                    catalog.Models.Add(key, new BakedMeshTopology(hash, parts));
                }
                var waypoints = ReadCount(reader, 1000);
                for (var i = 0; i < waypoints; i++)
                {
                    var key = reader.ReadString();
                    var hash = ReadHash(reader);
                    var points = new Vector3[ReadCount(reader, 10000000)];
                    for (var p = 0; p < points.Length; p++)
                        points[p] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    catalog.Waypoints.Add(key, (hash, points));
                }
                Log.WriteLine($"Loaded baked generation assets: models={models} navigation={waypoints}");
                return catalog;
            }
            catch (Exception exception) when (exception is IOException || exception is ArgumentException)
            {
                Log.WriteLine($"Ignoring invalid baked generation assets: {exception.Message}");
                return new Catalog();
            }
        }

        private static int ReadCount(BinaryReader Reader, int Maximum)
        {
            var count = Reader.ReadInt32();
            if (count < 0 || count > Maximum) throw new InvalidDataException("Invalid baked asset count");
            return count;
        }

        private static byte[] ReadHash(BinaryReader Reader)
        {
            var hash = Reader.ReadBytes(32);
            if (hash.Length != 32) throw new EndOfStreamException();
            return hash;
        }

        // Invoked by the packaging script, never by world generation or application startup.
        public static void Bake(string OutputPath)
        {
            var definitions = new List<(string Key, string Path, Vector3 Scale)>
            {
                ("GnollFortress:0", "Assets/Env/Structures/GnollFortress/GnollFortress0.ply", Vector3.One * 8),
                ("Garrison:0", "Assets/Env/Structures/Garrison/Garrison0-Mesh.ply", Vector3.One)
            };
            for (var i = 1; i <= 6; i++)
                definitions.Add(($"Cave{i}:0", $"Assets/Env/Structures/Caves/Cave{i}.ply", Vector3.One));
            var navigation = new List<(string Path, Vector3 Scale)>
            {
                ("Assets/Env/Structures/GnollFortress/GnollFortress0-Pathfinding.ply", Vector3.One * 8),
                ("Assets/Env/Structures/Garrison/Garrison0-Pathfinding.ply", Vector3.One)
            };
            for (var i = 1; i <= 6; i++)
                navigation.Add(($"Assets/Env/Structures/Caves/Cave{i}-Pathfinding.ply", Vector3.One));
            var temporary = OutputPath + ".tmp";
            using (var writer = new BinaryWriter(File.Create(temporary)))
            {
                writer.Write("HEDRA_GENERATION");
                writer.Write(FormatVersion);
                writer.Write(definitions.Count);
                foreach (var definition in definitions)
                {
                    var model = AssetManager.PLYLoader(definition.Path, definition.Scale);
                    var topology = BakedMeshTopology.Compile(model);
                    writer.Write(definition.Key);
                    writer.Write(topology.Fingerprint);
                    writer.Write(topology.Parts.Length);
                    foreach (var part in topology.Parts)
                    {
                        writer.Write(part.Length);
                        foreach (var vertex in part) writer.Write(vertex);
                    }
                    Console.WriteLine($"Baked {definition.Key}: {topology.Parts.Length} parts");
                }
                writer.Write(navigation.Count);
                foreach (var definition in navigation)
                {
                    var model = AssetManager.PLYLoader(definition.Path, definition.Scale, Vector3.Zero, Vector3.Zero, false);
                    model.Colors = Enumerable.Repeat(Vector4.One, model.Vertices.Count).ToList();
                    var points = WaypointLoader.SamplePositions(model);
                    writer.Write(definition.Path);
                    writer.Write(BakedMeshTopology.Hash(model));
                    writer.Write(points.Length);
                    foreach (var point in points) { writer.Write(point.X); writer.Write(point.Y); writer.Write(point.Z); }
                    Console.WriteLine($"Baked navigation {definition.Path}: {points.Length} samples");
                }
            }
            File.Move(temporary, OutputPath, true);
        }
    }
}
