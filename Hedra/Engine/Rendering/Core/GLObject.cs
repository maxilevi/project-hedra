using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.Rendering.Core
{
    public abstract class GLObject<T> : IDisposable, IdentifiableObject where T : class, IdentifiableObject
    {
        private static readonly List<T> Elements;

        static GLObject()
        {
            Elements = new List<T>();
        }

        public abstract uint Id { get; }
        public static int Alive => Elements.Count;
        
        protected GLObject()
        {
            Elements.Add(this as T);
        }
        
        public static T GetById(uint Id)
        {
            return Elements.FirstOrDefault(F => F.Id == Id);
        }

        public virtual void Dispose()
        {
            Elements.Remove(this as T);
        }
    }
}