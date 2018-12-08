using System;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering
{
    public abstract class LodableObject<T> where T : class
    {
        private Dictionary<int, T> _lodVersions;
        
        public void AddLOD(T Model, int Level)
        {
            if(Level != 2 && Level != 4 && Level != 8)
                throw new ArgumentOutOfRangeException($"LOD needs to be either 2, 4 or 8, '{Level}' given.");

            if (_lodVersions == null) _lodVersions = new Dictionary<int, T>
            {
                {2, null},
                {4, null},
                {8, null}
            };

            _lodVersions[Level] = Model;
        }

        public T Get(int Lod, bool UseFallback = true)
        {
            if (Lod == 1 || _lodVersions == null) return this as T;
            var selectedLod = _lodVersions[Lod];
            if (!UseFallback) return selectedLod;
            while (selectedLod == null)
            {
                selectedLod = Lod > 1
                    ? _lodVersions[Lod = Lod / 2]
                    : this as T;
            }
            return selectedLod;
        }

        public bool HasLod => _lodVersions != null;

        protected void ApplyRecursively(Action<T> Do)
        {
            if(_lodVersions == null) return;
            if(_lodVersions[2] != null) Do(_lodVersions[2]);
            if(_lodVersions[4] != null) Do(_lodVersions[4]);
            if(_lodVersions[8] != null) Do(_lodVersions[8]);
        }
    }
}