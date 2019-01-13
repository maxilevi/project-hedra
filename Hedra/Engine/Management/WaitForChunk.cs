using System;
using System.Collections;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.Management
{
    public class WaitForChunk
    {
        private readonly Vector3 _position;
        public Func<bool> DisposeCondition { get; set; }
        public Action OnDispose { get; set; }
        public bool Disposed { get; private set; }
        
        public WaitForChunk(Vector3 Position)
        {
            _position = Position;
        }
        
        public bool MoveNext()
        {
            var underChunk = World.GetChunkAt(_position);
            var currentSeed = World.Seed;
            while(underChunk == null || !underChunk.BuildedWithStructures)
            {
                if (World.Seed != currentSeed || (DisposeCondition?.Invoke() ?? false))
                {
                    OnDispose?.Invoke();
                    Disposed = true;
                    return true;
                }
                return true;
            }
            return false;
        }
    }
}