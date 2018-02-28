/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 01:34 a.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Events
{
	public static class EventDispatcher{

		private static readonly List<IEventListener> EventListeners = new List<IEventListener>();
	    private static Dictionary<object, EventHandler<KeyboardKeyEventArgs>> _keyHandlers;
	    private static event EventHandler<KeyboardKeyEventArgs> _onKeyDown;
		public static Vector2 Mouse { get; set; } = Vector2.Zero;
        public static Key LastKeyDown { get; private set; }

	    static EventDispatcher()
	    {
	        _keyHandlers = new Dictionary<object, EventHandler<KeyboardKeyEventArgs>>();
            Program.GameWindow.KeyDown += OnKeyDown;
	        Program.GameWindow.MouseUp += OnMouseButtonUp;
	        Program.GameWindow.MouseDown += OnMouseButtonDown;
	        Program.GameWindow.MouseWheel += OnMouseWheel;
	        Program.GameWindow.MouseMove += OnMouseMove;
	        Program.GameWindow.KeyUp += OnKeyUp;
	        Program.GameWindow.KeyPress += OnKeyPress;
	    }

        public static void RegisterKeyDown(object Key, EventHandler<KeyboardKeyEventArgs> EventHandler)
	    {
	        _keyHandlers.Add(Key, EventHandler);
	        _onKeyDown += _keyHandlers[Key];
	    }

	    public static void UnregisterKeyDown(object Key)
	    {
	        _onKeyDown -= _keyHandlers[Key];
	        _keyHandlers.Remove(Key);

	    }

        public static void Add(IEventListener e){
			EventListeners.Add(e);
	    }
		
		public static void Remove(IEventListener a){
			EventListeners.Remove(a);
		}
		
		private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e){
			for(int i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonDown(sender, e);
			}
		}
		
		private static void OnMouseButtonUp(object sender, MouseButtonEventArgs e){
			for(int i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonUp(sender, e);
			}
		}
		
		private static void OnMouseWheel(object sender, MouseWheelEventArgs e){
			for(int i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseWheel(sender, e);
			}
	    }
		
		private static void OnMouseMove(object sender, MouseMoveEventArgs e){
			Mouse = new Vector2(e.Mouse.X, e.Mouse.Y);
			for(int i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseMove(sender, e);
			}
	    }
		
		private static void OnKeyDown(object sender, KeyboardKeyEventArgs e)
		{
		    LastKeyDown = e.Key;
            for (int i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyDown(sender, e);
			}
		    _onKeyDown?.Invoke(sender, e);
		}
		
		private static void OnKeyUp(object sender, KeyboardKeyEventArgs e){
			for(int i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyUp(sender, e);
			}
		}
		
		private static void OnKeyPress(object sender, KeyPressEventArgs e){
			for(int i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyPress(sender, e);
			}
		}
	}
}

public enum EventPriority{
	Low,
	Normal,
	High,
}
