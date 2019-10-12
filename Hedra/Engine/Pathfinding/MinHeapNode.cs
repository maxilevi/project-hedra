using OpenToolkit.Mathematics;

namespace Hedra.Engine.Pathfinding
{
    /// <summary>
    /// Node in a heap
    /// </summary>
    internal sealed class MinHeapNode
    {        
        public MinHeapNode(Vector2 Position, float ExpectedCost)
        {
            this.Position     = Position;
            this.ExpectedCost = ExpectedCost;            
        }

        public Vector2 Position { get; }
        public float ExpectedCost { get; set; }                
        public MinHeapNode Next { get; set; }
    }
}
