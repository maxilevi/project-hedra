using System;

namespace Hedra.Engine.Rendering.Animation
{
    public class ProgressEvent : IDisposable
    {
        private readonly float _progress;
        private readonly OnAnimationHandler _callback;
        private bool _executed;
        
        public ProgressEvent(float Progress, OnAnimationHandler Callback)
        {
            _progress = Progress;
            _callback = Callback;
        }

        public void Update(Animation Animation, float Progress)
        {
            if (Progress > _progress && !_executed)
            {
                _callback(Animation);
                _executed = true;
            }
        }
        
        public void Reset()
        {
            _executed = false;
        }

        public void Dispose()
        {
        }
    }
}