namespace Hedra.Framework
{
    public unsafe struct AllocationEntry
    {
        public readonly int Offset;
        public readonly int Length;
        public readonly Pointer Ptr;

        public AllocationEntry(int Offset, int Length, Pointer Ptr)
        { ;
            this.Ptr = Ptr;
            this.Offset = Offset;
            this.Length = Length;
        }
    }
}