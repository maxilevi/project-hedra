/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Hedra.Engine.Management
{
    /// <summary>
    /// Description of Timer.
    /// </summary>
    public class Timer
    {
        private float _timerCount;
        public bool AutoReset { get; set; } = true;
        public float AlertTime { get; set; }
        
        public Timer(float AlertTime)
        {
            this.AlertTime = AlertTime;
        }
        
        public void Reset()
        {
            _timerCount = 0;
        }
        
        public bool Tick()
        {
            _timerCount += Time.IndependantDeltaTime;

            if (!Ready) return false;
            if (AutoReset)_timerCount = 0;

            return true;
        }

        public void MakeReady()
        {
            _timerCount = AlertTime;
        }

        public bool Ready => _timerCount >= AlertTime;

    }
}
