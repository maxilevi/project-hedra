using System.Collections.Generic;

namespace Hedra.Engine.DataStructures
{
    public class OctreeNode<T>
    {
        private OctreeNode<T>[] _children;
        private Dictionary<T, AABB> _objects;
        public AABB Bounds { get; set; }

        public bool IsLeaf => _children == null;

        private void Split(int Depth)
        {
            if (Depth-- <= 0) return;
            if (IsLeaf)
            {
                _children = new OctreeNode<T>[8];
                _children[0] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
                _children[1] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
                _children[2] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
                _children[3] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
                _children[4] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
                _children[5] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
                _children[6] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
                _children[7] = new OctreeNode<T>
                {
                    Bounds = new AABB()
                };
            }

            if (!IsLeaf && _objects.Count != 0)
            {
                for (var i = 0; i < _children.Length; ++i)
                    foreach (var pair in _objects)
                        if (_children[i].Bounds.Intersects(pair.Value))
                            _children[i]._objects.Add(pair.Key, pair.Value);
                _objects.Clear();
            }

            for (var i = 0; i < _children.Length; ++i) Split(Depth);
        }

        public void Insert(T Object, AABB Bounding)
        {
            if (Bounds.Intersects(Bounding))
            {
                if (IsLeaf)
                    _objects.Add(Object, Bounding);
                else
                    for (var i = 0; i < _children.Length; ++i)
                        _children[i].Insert(Object, Bounding);
            }
        }

        public void Remove(T Object)
        {
            if (IsLeaf && _objects.ContainsKey(Object))
                _objects.Remove(Object);
            else
                for (var i = 0; i < _children.Length; ++i)
                    _children[i].Remove(Object);
        }
    }
}