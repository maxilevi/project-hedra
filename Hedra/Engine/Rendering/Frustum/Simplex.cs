using System.Collections.Generic;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering.Frustum
{
    public class Simplex
    {
        private static readonly List<Simplex> _cache = new List<Simplex>();

        public SimplexVertex A = new SimplexVertex();
        public SimplexVertex B = new SimplexVertex();
        public SimplexVertex C = new SimplexVertex();
        public SimplexVertex D = new SimplexVertex();
        public SimplexType Type;
        
        
        public bool Locked { get; private set; }

        public void Lock(){ Locked = true; }
        public void Unlock(){ Locked = false; }
        
        
        public static Simplex Cache{
            get{
                lock (_cache)
                {
                    for (int i = _cache.Count - 1; i > -1; i--)
                    {
                        if (!_cache[i].Locked)
                            return _cache[i];
                    }
                }
                var New = new Simplex();
                lock (_cache)
                    _cache.Add(New);
                return New;
            }
        }
    }
    
    public enum SimplexType {
        Point,
        Edge,
        Face,
        Tetrahedron,
        MaxCount
    }
    
    public struct SimplexVertex
    {
        public Vector3 SupportA;
        public Vector3 SupportB;
        public Vector3 SupportPoint;
    }
}