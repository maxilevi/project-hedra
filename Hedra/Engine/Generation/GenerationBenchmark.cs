using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Game;

namespace Hedra.Engine.Generation
{
    // Opt-in real gameplay benchmark. Loads a save without ever writing it back.
    // The camera/player stay in place; normal generation, collisions and GPU uploads run.
    public static class GenerationBenchmark
    {
        private static readonly string CharacterPath = Environment.GetEnvironmentVariable("HEDRA_GENERATION_BENCHMARK");
        private static readonly Stopwatch Clock = new();
        private static readonly Stopwatch HoldClock = new();
        private static readonly int HoldSeconds = int.TryParse(Environment.GetEnvironmentVariable("HEDRA_GENERATION_HOLD_SECONDS"), out var seconds)
            ? Math.Max(0, seconds) : 0;
        private static readonly List<Vector2> Targets = new();
        private static Vector3 _position;
        private static double _lastReport;
        private static bool _started;
        private static bool _finished;
        private static bool _spawnReported;
        public static bool Enabled => !string.IsNullOrEmpty(CharacterPath);

        public static void Update()
        {
            if (!Enabled) return;
            if (_finished)
            {
                if (HoldClock.Elapsed.TotalSeconds >= HoldSeconds) Engine.Program.GameWindow.Close();
                return;
            }
            if (!_started)
            {
                _started = true;
                GenerationMetrics.Reset();
                Clock.Start();
                GameManager.LoadCharacter(DataManager.LoadPlayer(CharacterPath));
                _position = LocalPlayer.Instance.Position;
                var center = World.ToChunkSpace(_position);
                // Leave a two-chunk dependency border inside the unchanged loader radius.
                var radius = Math.Max(1, GameSettings.ChunkLoaderRadius / 2 - 2);
                for (var x = -radius; x <= radius; ++x)
                for (var z = -radius; z <= radius; ++z)
                    if (x * x + z * z <= radius * radius)
                        Targets.Add(center + new Vector2(x, z) * Chunk.Width);
                Log.WriteLine($"GEN_START seed={World.Seed} center={center} radius={GameSettings.ChunkLoaderRadius} targets={Targets.Count} processors={Environment.ProcessorCount}");
            }

            var player = LocalPlayer.Instance;
            player.Position = _position;
            player.Physics.UsePhysics = false;
            player.Physics.ResetVelocity();
            var ready = 0;
            var generated = 0;
            foreach (var target in Targets)
            {
                var chunk = World.GetChunkByOffset(target);
                if (chunk?.IsGenerated == true) ++generated;
                if (IsReady(chunk)) ++ready;
            }

            if (!_spawnReported)
            {
                var center = World.ToChunkSpace(_position);
                var spawnReady = true;
                for (var x = -1; x <= 1; ++x)
                for (var z = -1; z <= 1; ++z)
                    spawnReady &= IsReady(World.GetChunkByOffset(center + new Vector2(x, z) * Chunk.Width));
                if (spawnReady)
                {
                    _spawnReported = true;
                    Log.WriteLine($"GEN_SPAWN elapsed_ms={Clock.Elapsed.TotalMilliseconds:F2} chunks=9");
                }
            }

            if (Clock.Elapsed.TotalSeconds - _lastReport >= 5)
            {
                _lastReport = Clock.Elapsed.TotalSeconds;
                Log.WriteLine($"GEN_PROGRESS elapsed_ms={Clock.Elapsed.TotalMilliseconds:F2} generated={generated} uploaded={ready} targets={Targets.Count} blocks_queued={World.Builder.BlockQueueCount} meshes_queued={World.Builder.MeshQueueCount}");
            }

            var structuresReady = World.Builder.PendingStructureBuilds == 0 &&
                World.StructureHandler.StructureItems.All(s => s.Built || s.Disposed);
            if ((ready == Targets.Count && structuresReady) || Clock.Elapsed.TotalSeconds >= 300)
            {
                _finished = true;
                Clock.Stop();
                Log.WriteLine($"GEN_RESULT status={(ready == Targets.Count && structuresReady ? "complete" : "timeout")} elapsed_ms={Clock.Elapsed.TotalMilliseconds:F2} seed={World.Seed} uploaded={ready} targets={Targets.Count} structures_ready={structuresReady}");
                GenerationMetrics.Report();
                HoldClock.Start();
                if (HoldSeconds == 0) Engine.Program.GameWindow.Close();
            }
        }

        private static bool IsReady(Chunk Chunk) => Chunk != null && !Chunk.Disposed && Chunk.IsGenerated
            && Chunk.BuildedWithStructures && Chunk.BuildedCompletely && !Chunk.NeedsRebuilding
            && !Chunk.IsBuilding && Chunk.UploadedLod == Chunk.Lod && Chunk.Mesh.IsBuilded;
    }
}
