namespace Hedra.Framework
{
    public unsafe struct AllocationEntry
    {
        public readonly int Offset;
        public readonly int Length;
        public readonly void* Ptr;

        public AllocationEntry(int Offset, int Length, void* Ptr)
        { ;
            this.Ptr = Ptr;
            this.Offset = Offset;
            this.Length = Length;
        }
    }
}