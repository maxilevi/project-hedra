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
		public float AlertTime {get; set;}
		
		public Timer(float AlertTime){
			this.AlertTime = AlertTime;
		}
		
		public void Reset(){
			_timerCount = 0;
		}
		
		public bool Tick(){
			_timerCount += Time.FrameTimeSeconds;
		    if (!(_timerCount >= AlertTime)) return false;
		    _timerCount = 0;
		    return true;
		}
	}
}
