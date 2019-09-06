using System.Collections.Generic;
using Hedra.Engine.Rendering.Frustum;

namespace Hedra.Engine.DataStructures
{
    public class Octree<T>
    {
        private OctreeNode<T> _root;
        
        public void Insert(T Object, AABB Bounding)
        {
            _root.Insert(Object, Bounding);
        }
        
        public void Remove(T Object)
        {
            _root.Remove(Object);
        }

        public void Update(T Object, AABB Bounding)
        {
            Remove(Object);
            Insert(Object, Bounding);
        }
    }
}