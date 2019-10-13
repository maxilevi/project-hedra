using System;
using System.Windows.Forms;
using OpenToolkit.Windowing.Common;

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
    }
}