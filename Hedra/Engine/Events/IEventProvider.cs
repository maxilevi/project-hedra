using System;
using System.Windows.Forms;
using Hedra.Engine.Windowing;


namespace Hedra.Engine.Events
{
    public interface IEventProvider
    {
        event Action<MouseButtonEventArgs> MouseUp;
        event Action<MouseButtonEventArgs> MouseDown;
        event Action<MouseWheelEventArgs> MouseWheel;
        event Action<MouseMoveEventArgs> MouseMove;
        event Action<KeyboardKeyEventArgs> KeyDown;
        event Action<KeyboardKeyEventArgs> KeyUp;
        event Action<string> CharWritten;
    }
}