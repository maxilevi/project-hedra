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
    ///     Description of Timer.
    /// </summary>
    public class Timer
    {
        private Time.TimeProvider _provider;
        private int _lastThreadId;
        private float _timerCount;

        public Timer(float AlertTime)
        {
            this.AlertTime = AlertTime;
            if ((int)Math.Ceiling(AlertTime) == 0) throw new ArgumentOutOfRangeException("AlertTime cannot be zero.");
        }

        public bool AutoReset { get; set; } = true;
        public float AlertTime { get; set; }
        public bool UseTimeScale { get; set; } = true;

        public bool Ready => _timerCount >= AlertTime;
        public float Progress => _timerCount / AlertTime;

        public void Reset()
        {
            _timerCount = 0;
        }

        public bool Tick()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (_provider == null || _lastThreadId != threadId)
            {
                _provider = Time.Current;
                _lastThreadId = threadId;
            }

            _timerCount += UseTimeScale ? _provider.DeltaTime : _provider.IndependentDeltaTime;

            if (!Ready) return false;
            if (AutoReset) _timerCount = 0;

            return true;
        }

        public void MarkReady()
        {
            _timerCount = AlertTime;
        }
    }
}