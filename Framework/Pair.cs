using System;

namespace Hedra.Framework
{
    public class Pair<T, U> : Tuple<T, U>
    {
        public Pair(T One, U Two) : base(One, Two)
        {
        }
        
        public T One => base.Item1;
        public U Two => base.Item2;
    }
}