/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 01:34 a.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Windowing;
using OpenToolkit.Mathematics;
using Silk.NET.GLFW;
using Silk.NET.Input.Common;

namespace Hedra.Engine.Events
{
    public static class EventDispatcher
    {

        private static readonly List<IEventListener> EventListeners;
        private static readonly Dictionary<object, EventHandler<KeyEventArgs>> HighKeyDownHandlers;
        private static readonly Dictionary<object, EventHandler<KeyEventArgs>> NormalKeyDownHandlers;
        private static readonly Dictionary<object, EventHandler<KeyEventArgs>> LowKeyDownHandlers;
        private static readonly Dictionary<object, EventHandler<KeyEventArgs>> HighKeyUpHandlers;
        private static readonly Dictionary<object, EventHandler<KeyEventArgs>> NormalKeyUpHandlers;
        private static readonly Dictionary<object, EventHandler<KeyEventArgs>> LowKeyUpHandlers;
        private static readonly Dictionary<object, EventHandler<MouseButtonEventArgs>> MouseButtonUpHandlers;
        private static readonly Dictionary<object, EventHandler<MouseButtonEventArgs>> MouseButtonDownHandlers;
        private static readonly Dictionary<object, EventHandler<MouseMoveEventArgs>> MouseMoveHandlers;
        private static readonly Dictionary<object, EventHandler<MouseWheelEventArgs>> MouseWheelHandlers;

        private static event EventHandler<KeyEventArgs> HighOnKeyDownEvent;
        private static event EventHandler<KeyEventArgs> NormalOnKeyDownEvent;
        private static event EventHandler<KeyEventArgs> LowOnKeyDownEvent;
        private static event EventHandler<KeyEventArgs> HighOnKeyUpEvent;
        private static event EventHandler<KeyEventArgs> NormalOnKeyUpEvent;
        private static event EventHandler<KeyEventArgs> LowOnKeyUpEvent;
        private static event EventHandler<MouseMoveEventArgs> OnMouseMoveEvent;
        private static event EventHandler<MouseButtonEventArgs> OnMouseButtonUpEvent;
        private static event EventHandler<MouseButtonEventArgs> OnMouseButtonDownEvent;
        private static event EventHandler<MouseWheelEventArgs> OnMouseWheelEvent;

        public static Vector2 Mouse { get; set; } = Vector2.Zero;

        private static IEventProvider _provider;
        public static IEventProvider Provider
        {
            get => _provider;
            set
            {
                if (Provider != null)
                {
                    Provider.MouseUp -= OnMouseButtonUp;
                    Provider.MouseDown -= OnMouseButtonDown;
                    Provider.MouseWheel -= OnMouseWheel;
                    Provider.MouseMove -= OnMouseMove;
                    Provider.KeyDown -= OnKeyDown;
                    Provider.KeyUp -= OnKeyUp;
                }
                _provider = value;
                if (Provider != null)
                {
                    Provider.MouseUp += OnMouseButtonUp;
                    Provider.MouseDown += OnMouseButtonDown;
                    Provider.MouseWheel += OnMouseWheel;
                    Provider.MouseMove += OnMouseMove;
                    Provider.KeyDown += OnKeyDown;
                    Provider.KeyUp += OnKeyUp;
                }
            }
        }

        static EventDispatcher()
        {
            EventListeners = new List<IEventListener>();
            HighKeyDownHandlers = new Dictionary<object, EventHandler<KeyEventArgs>>();
            NormalKeyDownHandlers = new Dictionary<object, EventHandler<KeyEventArgs>>();
            LowKeyDownHandlers = new Dictionary<object, EventHandler<KeyEventArgs>>();
            HighKeyUpHandlers = new Dictionary<object, EventHandler<KeyEventArgs>>();
            NormalKeyUpHandlers = new Dictionary<object, EventHandler<KeyEventArgs>>();
            LowKeyUpHandlers = new Dictionary<object, EventHandler<KeyEventArgs>>();
            MouseMoveHandlers = new Dictionary<object, EventHandler<MouseMoveEventArgs>>();
            MouseWheelHandlers = new Dictionary<object, EventHandler<MouseWheelEventArgs>>();
            MouseButtonUpHandlers = new Dictionary<object, EventHandler<MouseButtonEventArgs>>();
            MouseButtonDownHandlers = new Dictionary<object, EventHandler<MouseButtonEventArgs>>();
            Provider = (IEventProvider) Program.GameWindow;
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

        public static void RegisterKeyDown(object Key, EventHandler<KeyEventArgs> EventHandler, EventPriority Priority = EventPriority.Normal)
        {
            switch (Priority)
            {
                case EventPriority.Low:
                    LowKeyDownHandlers.Add(Key, EventHandler);
                    LowOnKeyDownEvent += LowKeyDownHandlers[Key];
                    break;
                case EventPriority.Normal:
                    NormalKeyDownHandlers.Add(Key, EventHandler);
                    NormalOnKeyDownEvent += NormalKeyDownHandlers[Key];
                    break;
                case EventPriority.High:
                    HighKeyDownHandlers.Add(Key, EventHandler);
                    HighOnKeyDownEvent += HighKeyDownHandlers[Key];
                    break;
            }
        }

        public static void UnregisterKeyDown(object Key)
        {
            if (LowKeyDownHandlers.ContainsKey(Key))
            {
                LowOnKeyDownEvent -= LowKeyDownHandlers[Key];
                LowKeyDownHandlers.Remove(Key);
            }
            else if (NormalKeyDownHandlers.ContainsKey(Key))
            {
                NormalOnKeyDownEvent -= NormalKeyDownHandlers[Key];
                NormalKeyDownHandlers.Remove(Key);
            }
            else if (HighKeyDownHandlers.ContainsKey(Key))
            {
                HighOnKeyDownEvent -= HighKeyDownHandlers[Key];
                HighKeyDownHandlers.Remove(Key);
            }
        }

        public static void RegisterKeyUp(object Key, EventHandler<KeyEventArgs> EventHandler, EventPriority Priority = EventPriority.Normal)
        {
            switch (Priority)
            {
                case EventPriority.Low:
                    LowKeyUpHandlers.Add(Key, EventHandler);
                    LowOnKeyUpEvent += LowKeyUpHandlers[Key];
                    break;
                case EventPriority.Normal:
                    NormalKeyUpHandlers.Add(Key, EventHandler);
                    NormalOnKeyUpEvent += NormalKeyUpHandlers[Key];
                    break;
                case EventPriority.High:
                    HighKeyUpHandlers.Add(Key, EventHandler);
                    HighOnKeyUpEvent += HighKeyUpHandlers[Key];
                    break;
            }
        }

        public static void UnregisterKeyUp(object Key)
        {
            if (LowKeyUpHandlers.ContainsKey(Key))
            {
                LowOnKeyUpEvent -= LowKeyUpHandlers[Key];
                LowKeyUpHandlers.Remove(Key);
            }
            else if (NormalKeyUpHandlers.ContainsKey(Key))
            {
                NormalOnKeyUpEvent -= NormalKeyUpHandlers[Key];
                NormalKeyUpHandlers.Remove(Key);
            }
            else if (HighKeyUpHandlers.ContainsKey(Key))
            {
                HighOnKeyUpEvent -= HighKeyUpHandlers[Key];
                HighKeyUpHandlers.Remove(Key);
            }
        }

        public static void Add(IEventListener E)
        {
            EventListeners.Add(E);
        }
        
        public static void Remove(IEventListener A)
        {
            EventListeners.Remove(A);
        }

        public static void OnMouseButtonDown(MouseButtonEventArgs E)
        {
            OnMouseButtonDownEvent?.Invoke(null, E);
            for (var i = 0;i<EventListeners.Count; i++){
                EventListeners[i].OnMouseButtonDown(null, E);
            }
        }

        public static void OnMouseButtonUp(MouseButtonEventArgs E)
        {
            OnMouseButtonUpEvent?.Invoke(null, E);
            for (var i = 0;i<EventListeners.Count; i++){
                EventListeners[i].OnMouseButtonUp(null, E);
            }
        }

        public static void OnMouseWheel(MouseWheelEventArgs E)
        {
            OnMouseWheelEvent?.Invoke(null, E);
            for (var i = 0;i<EventListeners.Count; i++){
                EventListeners[i].OnMouseWheel(null, E);
            }
        }

        public static void OnMouseMove(MouseMoveEventArgs E)
        {
            Mouse = new Vector2(E.X, E.Y);
            OnMouseMoveEvent?.Invoke(null, E);
            for (var i = 0;i<EventListeners.Count; i++){
                EventListeners[i].OnMouseMove(null, E);
            }
        }

        public static void OnKeyDown(KeyboardKeyEventArgs E)
        {
            var keyEvent = new KeyEventArgs(E);
            HighOnKeyDownEvent?.Invoke(null, keyEvent);
            NormalOnKeyDownEvent?.Invoke(null, keyEvent);
            LowOnKeyDownEvent?.Invoke(null, keyEvent);
            for (var i = 0; i < EventListeners.Count; i++)
            {
                EventListeners[i].OnKeyDown(null, keyEvent);
            }
        }

        public static void OnKeyUp(KeyboardKeyEventArgs E)
        {
            var keyEvent = new KeyEventArgs(E);
            HighOnKeyUpEvent?.Invoke(null, keyEvent);
            NormalOnKeyUpEvent?.Invoke(null, keyEvent);
            LowOnKeyUpEvent?.Invoke(null, keyEvent);
            for (var i = 0; i < EventListeners.Count; i++)
            {
                EventListeners[i].OnKeyUp(null, keyEvent);
            }
        }

        public static void Clear()
        {
            EventListeners.Clear();
            var keyDownHandlers = HighKeyDownHandlers.Keys
                .Concat(NormalKeyDownHandlers.Keys)
                .Concat(LowKeyDownHandlers.Keys).ToArray();
            
            var keyUpHandlers = HighKeyUpHandlers.Keys
                .Concat(NormalKeyUpHandlers.Keys)
                .Concat(LowKeyUpHandlers.Keys).ToArray();
            
            for (var i = 0; i < keyDownHandlers.Length; i++)
            {
                UnregisterKeyDown(keyDownHandlers[i]);
            }

            for (var i = 0; i < keyUpHandlers.Length; i++)
            {
                UnregisterKeyUp(keyUpHandlers[i]);
            }

            var mouseMoved = MouseMoveHandlers.Keys.ToArray();
            foreach (var key in mouseMoved)
            {
                UnregisterMouseMove(key);
            }

            var mouseButtonDown = MouseButtonDownHandlers.Keys.ToArray();
            foreach (var key in mouseButtonDown)
            {
                UnregisterMouseDown(key);
            }

            var mouseButtonUp = MouseButtonUpHandlers.Keys.ToArray();
            foreach (var key in mouseButtonUp)
            {
                UnregisterMouseUp(key);
            }
        }
    }

    public class KeyEventArgs : EventArgs
    {
        public KeyboardKeyEventArgs Event { get; private set; }

        public KeyEventArgs(KeyboardKeyEventArgs Event)
        {
            this.Event = Event;
        }

        public void Cancel()
        {
            this.Event = new KeyboardKeyEventArgs();
        }

        public Key Key => Event.Key;
        public char KeyChar => Event.ScanCode;
        public bool Alt => Event.Alt;
        public bool Control => Event.Control;
        public bool Shift => Event.Shift;
        public KeyModifiers Modifiers => Event.Modifiers;
    }

    public enum EventPriority
    {
        Low,
        Normal,
        High
    }

}