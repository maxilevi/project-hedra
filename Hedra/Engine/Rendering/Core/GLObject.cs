using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;

namespace Hedra.Engine.Rendering.Core
{
    public abstract class GLObject<T> : IDisposable, IdentifiableObject where T : class, IdentifiableObject
    {
        private static readonly Dictionary<uint, GLObjectCounter> Elements;
        private static readonly List<T> Pending;
        private static readonly object Lock;
        private bool _disposed;

        static GLObject()
        {
            Elements = new Dictionary<uint, GLObjectCounter>();
            Pending = new List<T>();
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
                Pending.Add(this as T);
            TaskScheduler.When(() => Id != 0 || _disposed, () =>
            {
                if(_disposed) return;
                lock (Lock)
                {
                    Pending.Remove(this as T);
                    if (Elements.ContainsKey(Id))
                        Elements[Id].Count++;
                    else
                        Elements.Add(Id, new GLObjectCounter
                        {
                            GLObject = this as T,
                            Count = 1
                        });
                }
            });
        }
        
        public static T GetById(uint Id)
        {
            if (Id == 0) return default(T);
            lock (Lock)
            {
                if (!Elements.ContainsKey(Id))
                    return Pending.FirstOrDefault(O => O.Id == Id);
                return Elements[Id].GLObject;
            }
        }

        public virtual void Dispose()
        {
            _disposed = true;
            lock (Lock)
            {
                if(!Elements.ContainsKey(Id)) return;
                if (Elements[Id].Count == 1)
                    Elements.Remove(Id);
                else
                    Elements[Id].Count--;
            }
        }
        
        private class GLObjectCounter
        {
            public T GLObject;
            public int Count;
        }
    }
}