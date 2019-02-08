namespace Hedra.Engine.Core
{
    public class Pair<T, U>
    {
        public Pair(T One, U Two)
        {
            this.One = One;
            this.Two = Two;
        }
        
        public T One { get; set; }
        public U Two { get; set; }
    }
}