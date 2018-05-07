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

        private static event EventHandler<KeyboardKeyEventArgs> OnKeyDownEvent;
	    private static event EventHandler<KeyboardKeyEventArgs> OnKeyUpEvent;
        private static event EventHandler<KeyPressEventArgs> OnKeyPressedEvent;
        private static event EventHandler<MouseMoveEventArgs> OnMouseMoveEvent;
        private static event EventHandler<MouseButtonEventArgs> OnMouseButtonUpEvent;
	    private static event EventHandler<MouseButtonEventArgs> OnMouseButtonDownEvent;
	    private static event EventHandler<MouseWheelEventArgs> OnMouseWheelEvent;

        public static Vector2 Mouse { get; set; } = Vector2.Zero;

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
	        OnMouseMoveEvent += MouseMoveHandlers[Key];
	    }

	    public static void UnregisterMouseMove(object Key)
	    {
	        OnMouseMoveEvent -= MouseMoveHandlers[Key];
	        MouseMoveHandlers.Remove(Key);

	    }

	    public static void RegisterMouseDown(object Key, EventHandler<MouseButtonEventArgs> EventHandler)
	    {
	        MouseButtonDownHandlers.Add(Key, EventHandler);
	        OnMouseButtonDownEvent += MouseButtonDownHandlers[Key];
	    }

	    public static void UnregisterMouseDown(object Key)
	    {
	        OnMouseButtonDownEvent -= MouseButtonDownHandlers[Key];
	        MouseButtonDownHandlers.Remove(Key);

	    }

	    public static void RegisterMouseUp(object Key, EventHandler<MouseButtonEventArgs> EventHandler)
	    {
	        MouseButtonUpHandlers.Add(Key, EventHandler);
	        OnMouseButtonUpEvent += MouseButtonUpHandlers[Key];
	    }

	    public static void UnregisterMouseUp(object Key)
	    {
	        OnMouseButtonUpEvent -= MouseButtonUpHandlers[Key];
	        MouseButtonUpHandlers.Remove(Key);

	    }

        public static void RegisterKeyDown(object Key, EventHandler<KeyboardKeyEventArgs> EventHandler)
	    {
	        KeyDownHandlers.Add(Key, EventHandler);
	        OnKeyDownEvent += KeyDownHandlers[Key];
	    }

	    public static void UnregisterKeyDown(object Key)
	    {
	        OnKeyDownEvent -= KeyDownHandlers[Key];
	        KeyDownHandlers.Remove(Key);
	    }

	    public static void RegisterKeyUp(object Key, EventHandler<KeyboardKeyEventArgs> EventHandler)
	    {
	        KeyUpHandlers.Add(Key, EventHandler);
	        OnKeyUpEvent += KeyUpHandlers[Key];
	    }

	    public static void UnregisterKeyUp(object Key)
	    {
	        OnKeyUpEvent -= KeyUpHandlers[Key];
	        KeyUpHandlers.Remove(Key);
	    }

	    public static void RegisterKeyPress(object Key, EventHandler<KeyPressEventArgs> EventHandler)
	    {
	        KeyPressedHandlers.Add(Key, EventHandler);
	        OnKeyPressedEvent += KeyPressedHandlers[Key];
	    }

	    public static void UnregisterKeyPress(object Key)
	    {
	        OnKeyPressedEvent -= KeyPressedHandlers[Key];
	        KeyPressedHandlers.Remove(Key);
	    }

        public static void Add(IEventListener E){
			EventListeners.Add(E);
	    }
		
		public static void Remove(IEventListener A){
			EventListeners.Remove(A);
		}

	    public static void OnMouseButtonDown(object Sender, MouseButtonEventArgs E){
		    OnMouseButtonDownEvent?.Invoke(Sender, E);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonDown(Sender, E);
			}
        }

	    public static void OnMouseButtonUp(object Sender, MouseButtonEventArgs E){
		    OnMouseButtonUpEvent?.Invoke(Sender, E);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseButtonUp(Sender, E);
			}
        }

	    public static void OnMouseWheel(object Sender, MouseWheelEventArgs E){
		    OnMouseWheelEvent?.Invoke(Sender, E);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseWheel(Sender, E);
			}
        }

	    public static void OnMouseMove(object Sender, MouseMoveEventArgs E){
			Mouse = new Vector2(E.Mouse.X, E.Mouse.Y);
		    OnMouseMoveEvent?.Invoke(Sender, E);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnMouseMove(Sender, E);
			}
        }

	    public static void OnKeyDown(object Sender, KeyboardKeyEventArgs E)
		{
		    OnKeyDownEvent?.Invoke(Sender, E);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyDown(Sender, E);
			}
		}

	    public static void OnKeyUp(object Sender, KeyboardKeyEventArgs E){
		    OnKeyUpEvent?.Invoke(Sender, E);
            for (var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyUp(Sender, E);
			}
		}
		
		public static void OnKeyPress(object Sender, KeyPressEventArgs E){
            OnKeyPressedEvent?.Invoke(Sender, E);
			for(var i = 0;i<EventListeners.Count; i++){
				EventListeners[i].OnKeyPress(Sender, E);
			}
		}
	}
}

public enum EventPriority{
	Low,
	Normal,
	High,
}
