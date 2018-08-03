using System;
using System.Reflection;
using Hedra.Engine.Events;
using OpenTK;
using OpenTK.Input;

namespace HedraTests
{
    public class SimpleEventProvider : IEventProvider
    {
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        public event EventHandler<KeyboardKeyEventArgs> KeyDown;
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;
        public event EventHandler<KeyPressEventArgs> KeyPress;

        public void SimulateKeyDown(Key Key)
        {
            KeyDown.Invoke(this, CreateKeyEventArgs(Key));
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