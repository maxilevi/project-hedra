using System.Collections.Generic;

namespace Hedra.Engine.Core
{
    public interface ISequentialList<T>
    {
        int Count { get; }
        void Add(T Object);
        void AddRange(IEnumerable<T> Array);
        T this[int I] { get; set; }
    }
}