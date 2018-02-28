/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of Timer.
	/// </summary>
	public class Timer
	{
		private float TimerCount = 0;
		public float AlertTime {get; set;}
		
		public Timer(float AlertTime){
			this.AlertTime = AlertTime;
		}
		
		public void Reset(){
			TimerCount = 0;
		}
		
		public bool Tick(){
			TimerCount += Time.FrameTimeSeconds;
			if(TimerCount >= AlertTime){
				TimerCount = 0;
				return true;
			}else{
				return false;
			}
		}
	}
}
