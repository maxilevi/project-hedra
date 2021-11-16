using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hedra.Engine.Scenes
{
    public class WaypointGrid : WaypointGraph
    {
        private readonly HashSet<Waypoint> _blocked;
        private readonly object _drawLock;
        private readonly Waypoint[][] _waypoints;

        public WaypointGrid(int DimX, int DimY)
        {
            this.DimX = DimX;
            this.DimY = DimY;
            _drawLock = new object();
            _waypoints = new Waypoint[DimX][];
            _blocked = new HashSet<Waypoint>();
            for (var x = 0; x < DimX; ++x) _waypoints[x] = new Waypoint[DimY];
        }

        public int DimX { get; }
        public int DimY { get; }

        public void Rebuild(Vector3 Center, float Scale)
        {
            lock (_drawLock)
            {
                Clear();
                for (var x = 0; x < DimX; ++x)
                for (var y = 0; y < DimY; ++y)
                {
                    _waypoints[x][y] = new Waypoint
                    {
                        Position = new Vector3(x, 0, y) * Scale + Center
                    };
                    AddVertex(_waypoints[x][y]);
                    LinkVertex(new Vector2(x, y));
                }
            }
        }

        public bool IsWaypointBlocked(Vector2 Waypoint)
        {
            return _blocked.Contains(_waypoints[(int)Waypoint.X][(int)Waypoint.Y]);
        }

        public override Waypoint GetNearestVertex(Vector3 Point)
        {
            return GetNearestVertex(Point, W => _blocked.Contains(W), out _);
        }

        public void UnlinkVertex(Vector2 Waypoint)
        {
            var x = (int)Waypoint.X;
            var y = (int)Waypoint.Y;
            _blocked.Add(_waypoints[x][y]);
            if (x > 0)
                RemoveEdge(_waypoints[x][y], _waypoints[x - 1][y]);
            if (y > 0)
                RemoveEdge(_waypoints[x][y], _waypoints[x][y - 1]);
            if (x > 0 && y > 0)
                RemoveEdge(_waypoints[x][y], _waypoints[x - 1][y - 1]);
            if (x < DimX - 1)
                RemoveEdge(_waypoints[x][y], _waypoints[x + 1][y]);
            if (y < DimY - 1)
                RemoveEdge(_waypoints[x][y], _waypoints[x][y + 1]);
            if (x < DimX - 1 && y < DimY - 1)
                RemoveEdge(_waypoints[x][y], _waypoints[x + 1][y + 1]);
            if (x > 0 && y < DimY - 1)
                RemoveEdge(_waypoints[x][y], _waypoints[x - 1][y + 1]);
            if (x < DimX - 1 && y > 0)
                RemoveEdge(_waypoints[x][y], _waypoints[x + 1][y - 1]);
        }

        public void LinkVertex(Vector2 Waypoint)
        {
            var x = (int)Waypoint.X;
            var y = (int)Waypoint.Y;
            _blocked.Remove(_waypoints[x][y]);
            AlterWaypoint(Waypoint, AddEdge);
        }

        private void AlterWaypoint(Vector2 Waypoint, Action<Waypoint, Waypoint> Function)
        {
            var x = (int)Waypoint.X;
            var y = (int)Waypoint.Y;
            if (x > 0)
                Function(_waypoints[x][y], _waypoints[x - 1][y]);
            if (y > 0)
                Function(_waypoints[x][y], _waypoints[x][y - 1]);
            if (x > 0 && y > 0)
                Function(_waypoints[x][y], _waypoints[x - 1][y - 1]);
            if (x > 0 && y < DimY - 1)
                Function(_waypoints[x][y], _waypoints[x - 1][y + 1]);
        }

        public override void MergeGraph(WaypointGraph Graph, int MergeStep)
        {
            lock (_drawLock)
            {
                base.MergeGraph(Graph, MergeStep);
            }
        }

        public override void Clear()
        {
            lock (_drawLock)
            {
                base.Clear();
                _blocked.Clear();
            }
        }

        public override void Draw()
        {
            lock (_drawLock)
            {
                base.Draw();
            }
        }
    }
}