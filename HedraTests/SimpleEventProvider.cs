using System;
using System.Reflection;
using Hedra.Engine.Events;
using Hedra.Engine.Windowing;
using OpenToolkit.Mathematics;
using Silk.NET.Input.Common;

namespace HedraTests
{
    public class SimpleEventProvider : IEventProvider
    {
        public event Action<MouseButtonEventArgs> MouseUp;
        public event Action<MouseButtonEventArgs> MouseDown;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<KeyboardKeyEventArgs> KeyDown;
        public event Action<KeyboardKeyEventArgs> KeyUp;

        public void SimulateKeyDown(Key Key)
        {
            KeyDown.Invoke(CreateKeyEventArgs(Key));
        }

        private static KeyboardKeyEventArgs CreateKeyEventArgs(Key Press)
        {
            var keyEvent = new KeyboardKeyEventArgs();
            var prop = keyEvent.GetType().GetProperty("Key", BindingFlags.Instance | BindingFlags.Public);
            if(prop == null) throw new ArgumentException($"Couldn't find property 'Key' of KeyboardKeyEventArgs");
            prop.SetValue(keyEvent, Press);
            return keyEvent;
        }

        private MouseButtonEventArgs SimulateMouseButtonEvent(MouseButton Button, Vector2 Position)
        {
            throw new NotImplementedException();
            var mouseEvent = new MouseButtonEventArgs();
            mouseEvent.GetType().GetField("button", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(mouseEvent, Button);
            //mouseEvent.GetType().GetField("Position", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(mouseEvent, Position);
            return mouseEvent;
        }
    }
}