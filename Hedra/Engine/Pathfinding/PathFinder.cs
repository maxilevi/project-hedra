using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Pathfinding
{
    /// <summary>
    /// Computes a path in a grid according to the A* algorithm
    /// </summary>
    internal static class PathFinder
    {
        public static List<Vector2> FindPath(Grid Grid, Vector2 Start, Vector2 End, Offset[] MovementPattern)
        {
            if (Start == End)
            {
                return new List<Vector2> {Start};
            }

            var lastOption = default(Offset);
            var head = new MinHeapNode(Start, ManhattanDistance(Start, End));
            var open = new MinHeap();            
            open.Push(head);

            var costSoFar = new float[Grid.DimX * Grid.DimY];
            var cameFrom = new Vector2[Grid.DimX * Grid.DimY];                       

            while (open.HasNext())
            {
                // Get the best candidate
                var current = open.Pop().Position;

                if (current == End)
                {
                    return ReconstructPath(Grid, Start, End, cameFrom);
                }

                Step(Grid, open, cameFrom, costSoFar, MovementPattern, current, End, ref lastOption);
            }

            return null;
        }

        private static void Step(
            Grid Grid,
            MinHeap Open,
            Vector2[] CameFrom,
            float[] CostSoFar,
            Offset[] MovementPattern,
            Vector2 Current,
            Vector2 End,
            ref Offset LastDirection)
        {
            // Get the cost associated with getting to the current position
            var initialCost = CostSoFar[Grid.GetIndexUnchecked(Current.X, Current.Y)];

            // Get all directions we can move to according to the movement pattern and the dimensions of the grid
            foreach (var direction in GetMovementOptions(Current, Grid.DimX, Grid.DimY, MovementPattern))
            {
                var position = Current + direction;
                var cellCost = Grid.GetCellCostUnchecked(position);

                // Ignore this option if the cell is blocked
                if (float.IsInfinity(cellCost))
                    continue;

                var index = Grid.GetIndexUnchecked(position.X, position.Y);

                // Compute how much it would cost to get to the new position via this path
                var newCost = initialCost + cellCost * direction.Cost;

                // Compare it with the best cost we have so far, 0 means we don't have any path that gets here yet
                var oldCost = CostSoFar[index];
                if (!(oldCost <= 0) && !(newCost < oldCost))
                    continue;

                // Update the best path and the cost if this path is cheaper
                CostSoFar[index] = newCost;
                CameFrom[index] = Current;

                // Use the heuristic to compute how much it will probably cost 
                // to get from here to the end, and store the node in the open list
                var expectedCost = newCost + ManhattanDistance(position, End);
                Open.Push(new MinHeapNode(position, expectedCost));
                LastDirection = direction;
            }
        }

        private static List<Vector2> ReconstructPath(Grid Grid, Vector2 Start, Vector2 End, Vector2[] CameFrom)
        {
            var path = new List<Vector2> { End };
            var current = End;
            do
            {
                var previous = CameFrom[Grid.GetIndexUnchecked(current.X, current.Y)];               
                current = previous;
                path.Add(current);
            } while (current != Start);

            return path;
        }        

        private static IEnumerable<Offset> GetMovementOptions(
            Vector2 Position,
            int DimX,
            int DimY,
            IEnumerable<Offset> MovementPattern)
        {
            return MovementPattern.Where(
                M =>
                {
                    var target = Position + M;
                    return target.X >= 0 && target.X < DimX && target.Y >= 0 && target.Y < DimY;
                });
        }

        [MethodImpl(256)]
        private static float ManhattanDistance(Vector2 P0, Vector2 P1)
        {
            var dx = Math.Abs(P0.X - P1.X);
            var dy = Math.Abs(P0.Y - P1.Y);
            return dx + dy;
        }
    }
}
