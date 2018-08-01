using System;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Events
{
    public interface IEventProvider
    {
        event EventHandler<MouseButtonEventArgs> MouseUp;
        event EventHandler<MouseButtonEventArgs> MouseDown;
        event EventHandler<MouseWheelEventArgs> MouseWheel;
        event EventHandler<MouseMoveEventArgs> MouseMove;
        event EventHandler<KeyboardKeyEventArgs> KeyDown;
        event EventHandler<KeyboardKeyEventArgs> KeyUp;
        event EventHandler<KeyPressEventArgs> KeyPress;
    }
}