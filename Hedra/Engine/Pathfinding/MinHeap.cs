namespace Hedra.Engine.Pathfinding
{   
    /// <summary>
    /// Heap which keeps the node with the minimal expected path cost on the head position
    /// </summary>
    internal sealed class MinHeap
    {
        private MinHeapNode _head;      

        /// <summary>
        /// If the heap has a next element
        /// </summary>        
        public bool HasNext() => this._head != null;

        /// <summary>
        /// Pushes a node onto the heap        
        /// </summary>
        public void Push(MinHeapNode Node)
        {
            // If the heap is empty, just add the item to the top
            if (this._head == null)
            {
                this._head = Node;
            }                        
            else if (Node.ExpectedCost < this._head.ExpectedCost)
            {
                Node.Next = this._head;
                this._head = Node;
            }         
            else
            {
                var current = this._head;
                while (current.Next != null && current.Next.ExpectedCost <= Node.ExpectedCost)
                {
                    current = current.Next;
                }

                Node.Next = current.Next;
                current.Next = Node;
            }
        }

        /// <summary>
        /// Pops a node from the heap, this node is always the node
        /// with the cheapest expected path cost
        /// </summary>
        public MinHeapNode Pop()
        {
            var top = this._head;
            this._head = this._head.Next;

            return top;
        }
    }
}
