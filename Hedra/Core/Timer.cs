/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Threading;

namespace Hedra.Core
{
    /// <summary>
    /// Description of Timer.
    /// </summary>
    public class Timer
    {
        private float _timerCount;
        private Time.TimeProvider _provider;
        public bool AutoReset { get; set; } = true;
        public float AlertTime { get; set; }
        public bool UseTimeScale { get; set; } = true;
        
        public Timer(float AlertTime)
        {
            this.AlertTime = AlertTime;
            if((int)Math.Ceiling(AlertTime) == 0) throw new ArgumentOutOfRangeException($"AlertTime cannot be zero.");
        }
        
        public void Reset()
        {
            _timerCount = 0;
        }
        
        public bool Tick()
        {
            if (_provider == null)
                _provider = Time.GetProvider(Thread.CurrentThread.ManagedThreadId);
            
            _timerCount += UseTimeScale ? _provider.DeltaTime : _provider.IndependentDeltaTime;

            if (!Ready) return false;
            if (AutoReset)_timerCount = 0;

            return true;
        }

        public void MarkReady()
        {
            _timerCount = AlertTime;
        }

        public bool Ready => _timerCount >= AlertTime;
        public float Progress => _timerCount / AlertTime;
    }
}
