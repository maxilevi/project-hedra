using System.Collections.Generic;
using Hedra.Framework;

namespace Hedra.Engine.Core
{
    public class SequentialList<T> : List<T>, ISequentialList<T>
    {
        public SequentialList(ICollection<T> Collection) : base(Collection)
        {
        }
    }
}