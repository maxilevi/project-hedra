/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation
{
	public delegate void OnAnimationHandler(Animation Sender);
	
	public class Animation : IDisposable
	{
		public event OnAnimationHandler OnAnimationEnd;
		public event OnAnimationHandler OnAnimationMid;
		public event OnAnimationHandler OnAnimationStart;
		public float Speed { get; set; } = 1;
		public float Length { get; }
		public KeyFrame[] KeyFrames { get; }
		public bool Loop {get; set;}
		private bool _midAnimation;
	    private bool _startAnimation;
	
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
		}
		
		public void DispatchEvents(float Progress)
		{
			if(Progress >= 1)
			{
			    OnAnimationEnd?.Invoke(this);
			}
			if(Progress >= 0.5f && !_midAnimation)
            {
				_midAnimation = true;
			    OnAnimationMid?.Invoke(this);
			}
			if(Progress <= 0.5f && !_startAnimation)
            {
				_startAnimation = true;
			    OnAnimationStart?.Invoke(this);
			}
		}
		
		public void Reset()
		{
			_midAnimation = false;
			_startAnimation = false;
		}
		
		public void Dispose()
		{
            
        }
	}
}
