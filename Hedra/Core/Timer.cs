/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Diagnostics;
using System.Threading;
using Hedra.Game;

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
        private readonly Stopwatch _sw;
        
        public Timer(float AlertTime)
        {
            _sw = new Stopwatch();
            this.AlertTime = AlertTime;
            if((int)Math.Ceiling(AlertTime) == 0) throw new ArgumentOutOfRangeException($"AlertTime cannot be zero.");
        }
        
        public void Reset()
        {
            _timerCount = 0;
            _sw.Restart();
        }
        
        public bool Tick()
        {
            _sw.Stop();
            _timerCount += (_sw.ElapsedMilliseconds / 1000f) * (UseTimeScale ? GameSettings.Paused ? 1 : 0 : 1);
            _sw.Restart();
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
