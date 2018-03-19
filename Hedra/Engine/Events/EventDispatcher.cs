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

		private static readonly List<IEventListener> EventListeners;
	    private static readonly Dictionary<object, EventHandler<KeyboardKeyEventArgs>> KeyHandlers;
	    private static readonly Dictionary<object, EventHandler<MouseButtonEventArgs>> MouseButtonUpHandlers;
	    private static readonly Dictionary<object, EventHandler<MouseButtonEventArgs>> MouseButtonDownHandlers;
        private static readonly Dictionary<object, EventHandler<MouseMoveEventArgs>> MouseMoveHandlers;
	    private static readonly Dictionary<object, EventHandler<MouseWheelEventArgs>> MouseWheelHandlers;
        private static event EventHandler<KeyboardKeyEventArgs> _onKeyDown;
	    private static event EventHandler<MouseMoveEventArgs> _onMouseMove;
        private static event EventHandler<MouseButtonEventArgs> _onMouseButtonUp;
	    private static event EventHandler<MouseButtonEventArgs> _onMouseButtonDown;
	    private static event EventHandler<MouseWheelEventArgs> _onMouseWheel;
        public static Vector2 Mouse { get; set; } = Vector2.Zero;
        public static Key LastKeyDown { get; private set; }

	    static EventDispatcher()
	    {
	        EventListeners = new List<IEventListener>();
            KeyHandlers = new Dictionary<object, EventHandler<KeyboardKeyEventArgs>>();
	        MouseMoveHandlers = new Dictionary<object, EventHandler<MouseMoveEventArgs>>();
            MouseWheelHandlers = new Dictionary<object, EventHandler<MouseWheelEventArgs>>();
	        MouseButtonUpHandlers = new Dictionary<object, EventHandler<MouseButtonEventArgs>>();
	        MouseButtonDownHandlers = new Dictionary<object, EventHandler<MouseButtonEventArgs>>();
            Program.GameWindow.KeyDown += OnKeyDown;
	        Program.GameWindow.MouseUp += OnMouseButtonUp;
	        Program.GameWindow.MouseDown += OnMouseButtonDown;
	        Program.GameWindow.MouseWheel += OnMouseWheel;
	        Program.GameWindow.MouseMove += OnMouseMove;
	        Program.GameWindow.KeyUp += OnKeyUp;
	        Program.GameWindow.KeyPress += OnKeyPress;
	    }

	    public static void RegisterMouseMove(object Key, EventHandler<MouseMoveEventArgs> EventHandler)
	    {
	        MouseMoveHandlers.Add(Key, EventHandler);
	        _onMouseMove += MouseMoveHandlers[Key];
	    }

	    public static void UnregisterMouseMove(object Key)
	    {
	        _onMouseMove -= MouseMoveHandlers[Key];
	        MouseMoveHandlers.Remove(Key);

	    }

	    public static void RegisterMouseDown(object Key, EventHandler<MouseButtonEventArgs> EventHandler)
	    {
	        MouseButtonDownHandlers.Add(Key, EventHandler);
	        _onMouseButtonDown += MouseButtonDownHandlers[Key];
	    }

	    public static void UnregisterMouseDown(object Key)
	    {
	        _onMouseButtonDown -= MouseButtonDownHandlers[Key];
	        MouseButtonDownHandlers.Remove(Key);

	    }

        public static void RegisterKeyDown(object Key, EventHandler<KeyboardKeyEventArgs> EventHandler)
	    {
	        KeyHandlers.Add(Key, EventHandler);
	        _onKeyDown += KeyHandlers[Key];
	    }

	    public static void UnregisterKeyDown(object Key)
	    {
	        _onKeyDown -= KeyHandlers[Key];
	        KeyHandlers.Remove(Key);

	    }

        public static void Add(IEventListener e){
			EventListeners.Add(e);
	    }
		
		public static void Remove(IEventListener a){
			EventListeners.Remove(a);
		}
		
		private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e){
		    _onMouseButtonDown?.Invoke(sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonDown(sender, e);
			}
        }
		
		private static void OnMouseButtonUp(object sender, MouseButtonEventArgs e){
		    _onMouseButtonUp?.Invoke(sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonUp(sender, e);
			}
        }
		
		private static void OnMouseWheel(object sender, MouseWheelEventArgs e){
		    _onMouseWheel?.Invoke(sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseWheel(sender, e);
			}
        }
		
		private static void OnMouseMove(object sender, MouseMoveEventArgs e){
			Mouse = new Vector2(e.Mouse.X, e.Mouse.Y);
		    _onMouseMove?.Invoke(sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseMove(sender, e);
			}
        }
		
		private static void OnKeyDown(object sender, KeyboardKeyEventArgs e)
		{
		    LastKeyDown = e.Key;
		    _onKeyDown?.Invoke(sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyDown(sender, e);
			}
		}
		
		private static void OnKeyUp(object sender, KeyboardKeyEventArgs e){
			for(var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyUp(sender, e);
			}
		}
		
		private static void OnKeyPress(object sender, KeyPressEventArgs e){
			for(var i = 0;i<EventListeners.Count; i++){
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
