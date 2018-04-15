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
	    private static readonly Dictionary<object, EventHandler<KeyboardKeyEventArgs>> KeyDownHandlers;
	    private static readonly Dictionary<object, EventHandler<KeyboardKeyEventArgs>> KeyUpHandlers;
	    private static readonly Dictionary<object, EventHandler<KeyPressEventArgs>> KeyPressedHandlers;
        private static readonly Dictionary<object, EventHandler<MouseButtonEventArgs>> MouseButtonUpHandlers;
	    private static readonly Dictionary<object, EventHandler<MouseButtonEventArgs>> MouseButtonDownHandlers;
        private static readonly Dictionary<object, EventHandler<MouseMoveEventArgs>> MouseMoveHandlers;
	    private static readonly Dictionary<object, EventHandler<MouseWheelEventArgs>> MouseWheelHandlers;
        private static event EventHandler<KeyboardKeyEventArgs> _onKeyDown;
	    private static event EventHandler<KeyboardKeyEventArgs> _onKeyUp;
        private static event EventHandler<KeyPressEventArgs> _onKeyPressed;
        private static event EventHandler<MouseMoveEventArgs> _onMouseMove;
        private static event EventHandler<MouseButtonEventArgs> _onMouseButtonUp;
	    private static event EventHandler<MouseButtonEventArgs> _onMouseButtonDown;
	    private static event EventHandler<MouseWheelEventArgs> _onMouseWheel;
        public static Vector2 Mouse { get; set; } = Vector2.Zero;
        public static Key LastKeyDown { get; private set; }

	    static EventDispatcher()
	    {
	        EventListeners = new List<IEventListener>();
            KeyDownHandlers = new Dictionary<object, EventHandler<KeyboardKeyEventArgs>>();
	        KeyUpHandlers = new Dictionary<object, EventHandler<KeyboardKeyEventArgs>>();
	        KeyPressedHandlers = new Dictionary<object, EventHandler<KeyPressEventArgs>>();
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

	    public static void RegisterMouseUp(object Key, EventHandler<MouseButtonEventArgs> EventHandler)
	    {
	        MouseButtonUpHandlers.Add(Key, EventHandler);
	        _onMouseButtonUp += MouseButtonUpHandlers[Key];
	    }

	    public static void UnregisterMouseUp(object Key)
	    {
	        _onMouseButtonUp -= MouseButtonUpHandlers[Key];
	        MouseButtonUpHandlers.Remove(Key);

	    }

        public static void RegisterKeyDown(object Key, EventHandler<KeyboardKeyEventArgs> EventHandler)
	    {
	        KeyDownHandlers.Add(Key, EventHandler);
	        _onKeyDown += KeyDownHandlers[Key];
	    }

	    public static void UnregisterKeyDown(object Key)
	    {
	        _onKeyDown -= KeyDownHandlers[Key];
	        KeyDownHandlers.Remove(Key);
	    }

	    public static void RegisterKeyUp(object Key, EventHandler<KeyboardKeyEventArgs> EventHandler)
	    {
	        KeyUpHandlers.Add(Key, EventHandler);
	        _onKeyUp += KeyUpHandlers[Key];
	    }

	    public static void UnregisterKeyUp(object Key)
	    {
	        _onKeyUp -= KeyUpHandlers[Key];
	        KeyUpHandlers.Remove(Key);
	    }

	    public static void RegisterKeyPress(object Key, EventHandler<KeyPressEventArgs> EventHandler)
	    {
	        KeyPressedHandlers.Add(Key, EventHandler);
	        _onKeyPressed += KeyPressedHandlers[Key];
	    }

	    public static void UnregisterKeyPress(object Key)
	    {
	        _onKeyPressed -= KeyPressedHandlers[Key];
	        KeyPressedHandlers.Remove(Key);
	    }

        public static void Add(IEventListener e){
			EventListeners.Add(e);
	    }
		
		public static void Remove(IEventListener a){
			EventListeners.Remove(a);
		}
		
		private static void OnMouseButtonDown(object Sender, MouseButtonEventArgs e){
		    _onMouseButtonDown?.Invoke(Sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonDown(Sender, e);
			}
        }
		
		private static void OnMouseButtonUp(object Sender, MouseButtonEventArgs e){
		    _onMouseButtonUp?.Invoke(Sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonUp(Sender, e);
			}
        }
		
		private static void OnMouseWheel(object Sender, MouseWheelEventArgs e){
		    _onMouseWheel?.Invoke(Sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseWheel(Sender, e);
			}
        }
		
		private static void OnMouseMove(object Sender, MouseMoveEventArgs e){
			Mouse = new Vector2(e.Mouse.X, e.Mouse.Y);
		    _onMouseMove?.Invoke(Sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseMove(Sender, e);
			}
        }
		
		private static void OnKeyDown(object Sender, KeyboardKeyEventArgs e)
		{
		    LastKeyDown = e.Key;
		    _onKeyDown?.Invoke(Sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyDown(Sender, e);
			}
		}
		
		private static void OnKeyUp(object Sender, KeyboardKeyEventArgs e){
		    _onKeyUp?.Invoke(Sender, e);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyUp(Sender, e);
			}
		}
		
		private static void OnKeyPress(object Sender, KeyPressEventArgs e){
            _onKeyPressed?.Invoke(Sender, e);
			for(var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyPress(Sender, e);
			}
		}
	}
}

public enum EventPriority{
	Low,
	Normal,
	High,
}
