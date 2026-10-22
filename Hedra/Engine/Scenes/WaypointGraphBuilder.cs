using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hedra.Engine.Scenes
{
    // Keep the original insertion order: adjacency order determines BFS tie breaking.
    public sealed class WaypointGraphBuilder
    {
        private readonly WaypointGraph _graph;
        private readonly float _step;
        private readonly double _cellSize;
        private readonly Dictionary<(int, int, int), List<int>> _cells = new();
        private readonly HashSet<Waypoint> _seen = new();
        private readonly List<Waypoint> _vertices = new();
        private readonly List<int> _neighbors = new();

        public WaypointGraphBuilder(WaypointGraph Graph, float Step)
        {
            if (!(Step > 0) || !float.IsFinite(Step)) throw new ArgumentOutOfRangeException(nameof(Step));
            _graph = Graph;
            _step = Step;
            // Wider cells also cover floating point rounding at the inclusive boundary.
            _cellSize = (double)Step * 2;
            foreach (var vertex in Graph.Vertices) Index(vertex);
        }

        public void Add(Vector3 Position)
        {
            var cell = GetCell(Position);
            _neighbors.Clear();
            for (var x = cell.Item1 - 1; x <= cell.Item1 + 1; x++)
            for (var y = cell.Item2 - 1; y <= cell.Item2 + 1; y++)
            for (var z = cell.Item3 - 1; z <= cell.Item3 + 1; z++)
            {
                if (!_cells.TryGetValue((x, y, z), out var candidates)) continue;
                foreach (var index in candidates)
                {
                    var vertex = _vertices[index];
                    if (Math.Abs(vertex.Position.X - Position.X) <= _step &&
                        Math.Abs(vertex.Position.Y - Position.Y) <= _step &&
                        Math.Abs(vertex.Position.Z - Position.Z) <= _step)
                        _neighbors.Add(index);
                }
            }

            _neighbors.Sort();
            var waypoint = new Waypoint { Position = Position };
            _graph.AddVertex(waypoint);
            foreach (var index in _neighbors) _graph.AddEdge(waypoint, _vertices[index]);
            Index(waypoint);
        }

        private (int, int, int) GetCell(Vector3 Position) =>
            ((int)Math.Floor(Position.X / _cellSize), (int)Math.Floor(Position.Y / _cellSize),
                (int)Math.Floor(Position.Z / _cellSize));

        private void Index(Waypoint Waypoint)
        {
            if (!_seen.Add(Waypoint)) return;
            var cell = GetCell(Waypoint.Position);
            if (!_cells.TryGetValue(cell, out var bucket))
                _cells.Add(cell, bucket = new List<int>());
            bucket.Add(_vertices.Count);
            _vertices.Add(Waypoint);
        }
    }
}
