using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hedra.Engine.Scenes
{
    public class WaypointGrid : WaypointGraph
    {
        private readonly Waypoint[][] _waypoints;
        private readonly HashSet<Waypoint> _blocked;
        private Vector3 _position;
        public int DimX { get; }
        public int DimY { get; }

        public WaypointGrid(int DimX, int DimY)
        {
            this.DimX = DimX;
            this.DimY = DimY;
            _waypoints = new Waypoint[DimX][];
            _blocked = new HashSet<Waypoint>();
            for (var x = 0; x < DimX; ++x)
            {
                _waypoints[x] = new Waypoint[DimY];
                for (var y = 0; y < DimY; ++y)
                {
                    _waypoints[x][y] = new Waypoint
                    {
                        Position = new Vector3(x, 0, y)
                    };
                    AddVertex(_waypoints[x][y]);
                    LinkVertex(new Vector2(x,y));
                }
            }
        }

        public bool IsWaypointBlocked(Vector2 Waypoint)
        {
            return _blocked.Contains(_waypoints[(int) Waypoint.X][(int) Waypoint.Y]);
        }

        public void UnlinkVertex(Vector2 Waypoint)
        {
            var x = (int)Waypoint.X;
            var y = (int)Waypoint.Y;
            _blocked.Add(_waypoints[x][y]);
            if(x > 0)
                RemoveEdge(_waypoints[x][y], _waypoints[x-1][y]);
            if(y > 0)
                RemoveEdge(_waypoints[x][y], _waypoints[x][y-1]);
            if(x < DimX-1)
                RemoveEdge(_waypoints[x][y], _waypoints[x+1][y]);
            if(y < DimY-1)
                RemoveEdge(_waypoints[x][y], _waypoints[x][y+1]);
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
            if(x > 0)
                Function(_waypoints[x][y], _waypoints[x-1][y]);
            if(y > 0)
                Function(_waypoints[x][y], _waypoints[x][y-1]);
        }

        public Vector3 Position
        {
            get => _position;
            set
            {
                //for (var i = 0; i <; ++i)
                {
                    
                }
                _position = value;
            }
        }
    }
}