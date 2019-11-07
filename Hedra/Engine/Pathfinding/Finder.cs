using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.Pathfinding
{
    public static class Finder
    {
        public static void UpdateGrid(IEntity Parent, Grid Graph)
        {
            for (var x = 0; x < Graph.DimX; ++x)
            {
                for (var y = 0; y < Graph.DimY; ++y)
                {
                    var position = new Vector2(x, y);
                    Graph.UnblockCell(position);
                    var realPosition = ToWorldCoordinates(position, Graph.DimX, Graph.DimY);
                    if(Parent.Physics.CollidesWithOffset(realPosition))
                        Graph.BlockCell(position);
                }
            }
        }

        private static Vector3 ToWorldCoordinates(Vector2 Position, int DimX, int DimY)
        {
            return (Position - new Vector2((int) (DimX / 2f), (int) (DimY / 2f))).ToVector3() *
                   Chunk.BlockSize;
        }

        public static Vector2[] GetPath(Grid Graph, Vector2 Start, Vector2 End)
        {
            return Graph.GetPath(Start, End, MovementPatterns.Full);
        }

        public static Vector2 NearestUnblockedCell(Grid Graph, Vector2 Current)
        {
            return NearestUnblockedCell(Graph, Current, default(Vector2), true);
        }
        
        public static Vector2 NearestUnblockedCell(Grid Graph, Vector2 Current, Vector2 Target, bool NoTarget = false)
        {
            if (IsUnblockedCell(Graph, Current)) return Current;
            var marker = new HashSet<Vector2>();
            var traversed = new HashSet<Vector2>();
            var queue = new Queue<Vector2>();
            queue.Enqueue(Current);
            while (queue.Count > 0)
            {
                var n = queue.Dequeue();
                var newPosition = n + Vector2.UnitX;
                if (InBounds(Graph, newPosition) && !traversed.Contains(newPosition))
                {
                    marker.Clear();
                    if (IsUnblockedCell(Graph, newPosition) &&
                        (NoTarget || IsConnectedWith(Graph, newPosition, Target, marker)))
                        return newPosition;
                    queue.Enqueue(newPosition);
                    traversed.Add(newPosition);
                }

                newPosition = n + Vector2.UnitY;
                if (InBounds(Graph, newPosition) && !traversed.Contains(newPosition))
                {
                    marker.Clear();
                    if (IsUnblockedCell(Graph, newPosition) &&
                        (NoTarget || IsConnectedWith(Graph, newPosition, Target, marker)))
                        return newPosition;
                    queue.Enqueue(newPosition);
                    traversed.Add(newPosition);
                }

                newPosition = n - Vector2.UnitX;
                if (InBounds(Graph, newPosition) && !traversed.Contains(newPosition))
                {
                    marker.Clear();
                    if (IsUnblockedCell(Graph, newPosition) &&
                        (NoTarget || IsConnectedWith(Graph, newPosition, Target, marker)))
                        return newPosition;
                    queue.Enqueue(newPosition);
                    traversed.Add(newPosition);
                }

                newPosition = n - Vector2.UnitY;
                if (InBounds(Graph, newPosition) && !traversed.Contains(newPosition))
                {
                    marker.Clear();
                    if (IsUnblockedCell(Graph, newPosition) &&
                        (NoTarget || IsConnectedWith(Graph, newPosition, Target, marker)))
                        return newPosition;
                    queue.Enqueue(newPosition);
                    traversed.Add(newPosition);
                }
            }

            return Current;
        }

        public static bool IsConnectedWith(Grid Graph, Vector2 Origin, Vector2 Target, HashSet<Vector2> Marked)
        {
            if (Marked.Contains(Origin)) return false;
            if (Origin == Target) return true;
            if (!InBounds(Graph, Origin)) return false;
            if (!IsUnblockedCell(Graph, Origin)) return false;

            /* Mark the cell as visited */
            Marked.Add(Origin);
            if (IsConnectedWith(Graph, Origin + Vector2.UnitX, Target, Marked)) return true;
            if (IsConnectedWith(Graph, Origin + Vector2.UnitY, Target, Marked)) return true;
            if (IsConnectedWith(Graph, Origin - Vector2.UnitX, Target, Marked)) return true;
            if (IsConnectedWith(Graph, Origin - Vector2.UnitY, Target, Marked)) return true;
            
            return false;
        }

        private static bool InBounds(Grid Graph, Vector2 Origin)
        {
            return Origin.X < Graph.DimX && Origin.X > -1 && Origin.Y < Graph.DimY && Origin.Y > -1;
        }
        
        private static bool IsUnblockedCell(Grid Graph, Vector2 Test)
        {
            return !float.IsInfinity(Graph.GetCellCostUnchecked(Test));
        }
    }
}