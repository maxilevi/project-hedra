using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK;

namespace Hedra.Engine.Pathfinding
{
    /// <summary>
    /// Representation of your world for the pathfinding algorithm.
    /// Use SetCellCost to change the cost of traversing a cell.
    /// Use BlockCell to make a cell completely intraversable.
    /// </summary>
    public sealed class Grid
    {       
        private readonly float _defaultCost;
        private readonly float[] _weights;        

        /// <summary>
        /// Creates a grid
        /// </summary>
        /// <param name="DimX">The x-dimension of your world</param>
        /// <param name="DimY">The y-dimesion of your world</param>
        /// <param name="DefaultCost">The default cost every cell is initialized with</param>
        public Grid(int DimX, int DimY, float DefaultCost = 1.0f)
        {
            if (DefaultCost < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"Argument {nameof(DefaultCost)} with value {DefaultCost} is invalid. The cost of traversing a cell cannot be less than one");
            }

            this._defaultCost = DefaultCost;
            this._weights = new float[DimX * DimY];
            this.DimX = DimX;
            this.DimY = DimY;

            for (var n = 0; n < this._weights.Length; n++)
            {
                this._weights[n] = DefaultCost;
            }
        }

        /// <summary>
        /// X-dimension of the grid
        /// </summary>
        public int DimX { get; }
        
        /// <summary>
        /// Y-dimension of the grid
        /// </summary>
        public int DimY { get; }

        /// <summary>
        /// Sets the cost for traversing a cell
        /// </summary>
        /// <param name="Position">A position inside the grid</param>
        /// <param name="Cost">The cost of traversing the cell, cannot be less than one</param>
        public void SetCellCost(Vector2 Position, float Cost)
        {
            if (Cost < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"Argument {nameof(Cost)} with value {Cost} is invalid. The cost of traversing a cell cannot be less than one");
            }

            this._weights[GetIndex(Position.X, Position.Y)] = Cost;
        }

        /// <summary>
        /// Makes the cell intraversable
        /// </summary>
        /// <param name="Position">A position inside the grid</param>
        public void BlockCell(Vector2 Position) => SetCellCost(Position, float.PositiveInfinity);

        /// <summary>
        /// Makes the cell traversable, gives it the default traversal cost as provided in the constructor
        /// </summary>
        /// <param name="Position">A position inside the grid</param>
        public void UnblockCell(Vector2 Position) => SetCellCost(Position, this._defaultCost);

        /// <summary>
        /// Looks-up the cost for traversing a given cell, if a cell is blocked (<see cref="BlockCell"/>) 
        /// +infinity is returned
        /// </summary>
        /// <param name="Position">A position inside the grid</param>
        /// <returns>The cost</returns>
        public float GetCellCost(Vector2 Position)
        {
            return this._weights[GetIndex(Position.X, Position.Y)];
        }

        /// <summary>
        /// Looks-up the cost for traversing a given cell, does not check
        /// if the position is inside the grid
        /// </summary>
        /// <param name="Position">A position inside the grid</param>
        /// <returns>The cost</returns>
        internal float GetCellCostUnchecked(Vector2 Position)
        {
            return this._weights[GetIndexUnchecked(Position.X, Position.Y)];
        }

        /// <summary>
        /// Computes the lowest-cost path from start to end inside the grid for an agent that can
        /// move both diagonal and lateral
        /// </summary>
        /// <param name="Start">The start position</param>
        /// <param name="End">The end position</param>        
        /// <returns>Positions along the shortest path from start to end, or an empty array if no path could be found</returns>
        public Vector2[] GetPath(Vector2 Start, Vector2 End)
            => GetPath(Start, End, MovementPatterns.Full);

        /// <summary>
        /// Computes the lowest-cost path from start to end inside the grid for an agent with a custom
        /// movement pattern
        /// </summary>
        /// <param name="Start">The start position</param>
        /// <param name="End">The end position</param>
        /// <param name="MovementPattern">The movement pattern of the agent, <see cref="MovementPatterns"/> for several built-in options</param>
        /// <returns>Positions along the shortest path from start to end, or an empty array if no path could be found</returns>
        public Vector2[] GetPath(Vector2 Start, Vector2 End, Offset[] MovementPattern)
        {
            var current = PathFinder.FindPath(this, Start, End, MovementPattern);

            if (current == null)
            {
                return new Vector2[0];
            }

            // The Pathfinder returns the positions that found the end. If we want
            // to list positions from start to end we need reverse the traversal.
            var steps = new Stack<Vector2>();
            
            foreach (var step in current)
            {
                steps.Push(step);
            }            

            return steps.ToArray();                        
        }

        /// <summary>
        /// Converts a 2d index to a 1d index and performs bounds checking
        /// </summary>        
        private int GetIndex(float X, float Y)
        {
            if (X < 0 || X >= this.DimX)
            {
                throw new ArgumentOutOfRangeException(
                    $"The x-coordinate {X} is outside of the expected range [0...{this.DimX})");
            }

            if (Y < 0 || Y >= this.DimY)
            {
                throw new ArgumentOutOfRangeException(
                    $"The y-coordinate {Y} is outside of the expected range [0...{this.DimY})");
            }

            return GetIndexUnchecked(X, Y);
        }     
        
        /// <summary>
        /// Converts a 2d index to a 1d index without any bounds checking
        /// </summary>        
        [MethodImpl(256)]
        internal int GetIndexUnchecked(float X, float Y) => (this.DimX * ((int)Y) + ((int)X));
    }    
}

