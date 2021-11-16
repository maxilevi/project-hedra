using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.Rendering.Core
{
    public abstract class GLObject<T> : IDisposable, IdentifiableObject where T : class, IdentifiableObject
    {
        private static readonly Dictionary<uint, T> Indexed;
        private static readonly List<T> Elements;
        private static readonly object SyncRoot;
        private bool _disposed;

        static GLObject()
        {
            Elements = new List<T>();
            Indexed = new Dictionary<uint, T>();
            SyncRoot = new object();
        }

        protected GLObject()
        {
            lock (SyncRoot)
            {
                Elements.Add(this as T);
            }
        }

        public static int Alive
        {
            get
            {
                lock (SyncRoot)
                {
                    return Elements.Count;
                }
            }
        }

        public abstract uint Id { get; }

        public virtual void Dispose()
        {
            _disposed = true;
            lock (SyncRoot)
            {
                Elements.Remove(this as T);
                Indexed.Remove(Id);
            }
        }

        public static T GetById(uint Id)
        {
            if (Id == 0) return null;
            lock (SyncRoot)
            {
                if (!Indexed.ContainsKey(Id))
                    Indexed.Add(Id, Elements.First(O => O.Id == Id));
                //Elements.Remove(Indexed[Id]);
                return Indexed[Id];
            }
        }

        ~GLObject()
        {
            if (!_disposed)
                if (!Program.GameWindow.IsExiting)
                    Dispose();
        }
    }
}