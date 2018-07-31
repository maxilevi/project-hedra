/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 01:34 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Events
{
	public interface IEventListener
	{
		void OnMouseButtonUp(object sender, MouseButtonEventArgs e);
		
		void OnMouseButtonDown(object sender, MouseButtonEventArgs e);
		
		void OnMouseWheel(object sender, MouseWheelEventArgs e);
		
		void OnMouseMove(object sender, MouseMoveEventArgs e);
		
		void OnKeyDown(object sender, KeyboardKeyEventArgs EventArgs);
		
		void OnKeyUp(object sender, KeyboardKeyEventArgs e);
		
		void OnKeyPress(object sender, KeyPressEventArgs e);
	}
}
