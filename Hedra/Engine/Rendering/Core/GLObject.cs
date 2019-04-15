using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.Rendering.Core
{
    public abstract class GLObject<T> : IDisposable, IdentifiableObject where T : class, IdentifiableObject
    {
        private static readonly List<T> Elements;
        private static readonly object Lock;

        static GLObject()
        {
            Elements = new List<T>();
            Lock = new object();
        }

        public abstract uint Id { get; }
        public static int Alive
        {
            get
            {
                lock (Lock)
                    return Elements.Count;
            }
        }

        protected GLObject()
        {
            lock(Lock)
                Elements.Add(this as T);
        }
        
        public static T GetById(uint Id)
        {
            lock(Lock)
                return Elements.FirstOrDefault(F => F.Id == Id);
        }

        public virtual void Dispose()
        {
            lock(Lock)
                Elements.Remove(this as T);
        }
    }
}