using System.Collections.Generic;

namespace Hedra.Framework
{
    public interface ISequentialList<T>
    {
        int Count { get; }
        void Add(T Object);
        void AddRange(IEnumerable<T> Array);
        T this[int I] { get; set; }
    }
}