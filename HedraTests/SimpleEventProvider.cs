using System;
using System.Reflection;
using Hedra.Engine.Events;
using Hedra.Engine.Windowing;
using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Input;
using MouseButton = Silk.NET.Input.MouseButton;

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
        public event Action<string> CharWritten;

        public void SimulateKeyDown(Key Key)
        {
            KeyDown.Invoke(CreateKeyEventArgs(Key));
        }

        private static KeyboardKeyEventArgs CreateKeyEventArgs(Key Press)
        {
            var keyEvent = new KeyboardKeyEventArgs(Press, default);
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