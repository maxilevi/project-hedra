/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/06/2016
 * Time: 10:32 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Input;

namespace VoxelShift.Engine.Events
{
	/// <summary>
	/// Description of OnDrag.
	/// </summary>
	public class DisplayOnDrag : EventListener
	{
		public bool InFocus;
		public Vector2 Scale, Position;
		public float Value;
		public float Increment = 1;
		
		public override void OnMouseClick(object sender, MouseButtonEventArgs e){
			Vector2 Coords = Mathf.ToNormalizedDeviceCoordinates(e.Mouse.X, e.Mouse.Y);
				
			if(Position.Y + Scale.Y > -Coords.Y && Position.Y - Scale.Y < -Coords.Y 
				&& Position.X + Scale.X > Coords.X && Position.X - Scale.X < Coords.X ){
					if(e.Button == MouseButton.Left)
						InFocus = e.IsPressed;
			}
		}
		public override void OnMouseMove(object sender, MouseMoveEventArgs e){
			if(InFocus){
				Value += XDelta * Increment * Time.unScaledDeltaTime;
			}
		}
	}
}
