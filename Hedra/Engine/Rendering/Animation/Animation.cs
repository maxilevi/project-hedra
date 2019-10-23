/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation
{
    public delegate void OnAnimationHandler(Animation Sender);
    
    public class Animation : IDisposable
    {
        public event OnAnimationHandler OnAnimationEnd;
        public event OnAnimationHandler OnAnimationMid;
        public event OnAnimationHandler OnAnimationStart;
        public event OnAnimationHandler OnAnimationUpdate;
        public float Speed { get; set; } = 1;
        public float Length { get; }
        public KeyFrame[] KeyFrames { get; }
        public bool Loop {get; set;}
        private readonly List<ProgressEvent> _events;
        private bool _midAnimation;
        private bool _startAnimation;
        private bool _endAnimation;
    
        /**
         * @param lengthInSeconds
         *            - the total length of the animation in seconds.
         * @param frames
         *            - all the keyframes for the animation, ordered by time of
         *            appearance in the animation.
         */
        public Animation(float LengthInSeconds, KeyFrame[] Frames)
        {
            this.KeyFrames = Frames;
            this.Length = LengthInSeconds;
            this.Loop = true;
            this._events = new List<ProgressEvent>();
        }
        
        public void DispatchEvents(float Progress)
        {
            if(Progress >= 1 && !_endAnimation)
            {
                _endAnimation = true;
                OnAnimationEnd?.Invoke(this);
            }
            if(Progress >= 0.5f && !_midAnimation)
            {
                _midAnimation = true;
                OnAnimationMid?.Invoke(this);
            }
            if(Progress > 0f && !_startAnimation)
            {
                _startAnimation = true;
                OnAnimationStart?.Invoke(this);
            }
            for (var i = 0; i < _events.Count; i++)
            {
                _events[i].Update(this, Progress);
            }
            OnAnimationUpdate?.Invoke(this);
        }

        public void RegisterOnProgressEvent(float Progress, OnAnimationHandler Callback)
        {
            _events.Add(new ProgressEvent(Progress, Callback));
        }

        public void Reset()
        {
            _midAnimation = false;
            _startAnimation = false;
            _endAnimation = false;
            _events.ForEach(E => E.Reset());
        }
        
        private static void RemoveListeners(OnAnimationHandler Handler)
        {
            if (Handler != null)
            {
                var list = Handler.GetInvocationList();
                for(var i = 0; i < list.Length; i++)
                {
                    Handler -= (OnAnimationHandler) list[i];
                }
            }
        }
        
        public void Dispose()
        {
            RemoveListeners(OnAnimationEnd);
            RemoveListeners(OnAnimationMid);
            RemoveListeners(OnAnimationStart);
            _events.ForEach(E => E.Dispose());
            _events.Clear();
        }
    }
}
