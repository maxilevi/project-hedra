/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 02:14 a.m.
 *
 */
using System;
using OpenTK.Input;
using OpenTK;
namespace Hedra.Engine.Events
{
	public class EventListener : IEventListener
	{
		public EventListener(){
			EventDispatcher.Add(this);
		}
		
		public virtual void OnMouseButtonUp(object sender, MouseButtonEventArgs e){}
		
		public virtual void OnMouseButtonDown(object sender, MouseButtonEventArgs e){}
		
		public virtual void OnMouseWheel(object sender, MouseWheelEventArgs e){}
		
		public virtual void OnMouseMove(object sender, MouseMoveEventArgs e){}
		
		public virtual void OnKeyDown(object sender, KeyboardKeyEventArgs e){}

		public virtual void OnKeyUp(object sender, KeyboardKeyEventArgs e){}
		
		public virtual void OnKeyPress(object sender, KeyPressEventArgs e){}
	}
}
