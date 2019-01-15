namespace Hedra.Engine.Rendering
{
    public struct CompressedValue<T> where T : struct
    {
        public ushort Count { get; set; }
        public T Type { get; set; }
    }
}