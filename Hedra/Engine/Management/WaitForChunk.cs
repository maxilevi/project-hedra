using System;
using System.Collections;
using Hedra.Core;
using Hedra.Engine.Generation;
using System.Numerics;

namespace Hedra.Engine.Management
{
    public class WaitForChunk
    {
        private readonly Vector3 _position;
        private readonly Timer _timer;
        public Func<bool> DisposeCondition { get; set; }
        public Action OnDispose { get; set; }
        public bool Disposed { get; private set; }

        public WaitForChunk(Vector3 Position)
        {
            _position = Position;
            _timer = new Timer(.05f);
        }
        
        public bool MoveNext()
        {
            if (!_timer.Tick()) return true;
            var underChunk = World.GetChunkAt(_position);
            var currentSeed = World.Seed;
            if(underChunk == null || !underChunk.BuildedWithStructures)
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