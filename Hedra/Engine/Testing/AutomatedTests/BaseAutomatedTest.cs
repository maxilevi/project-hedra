using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Testing.AutomatedTests
{
    public class BaseAutomatedTest : BaseTest
    {
        protected KeyboardKeyEventArgs SimulateKeyEvent(Key Press)
        {
            var keyEvent = new KeyboardKeyEventArgs();
            keyEvent.GetType().GetField("key", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(keyEvent, Press);
            return keyEvent;
        }

        protected MouseButtonEventArgs SimulateMouseButtonEvent(MouseButton Button, Vector2 Position)
        {
            var mouseEvent = new MouseButtonEventArgs();
            mouseEvent.GetType().GetField("button", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(mouseEvent, Button);
            //mouseEvent.GetType().GetField("Position", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(mouseEvent, Position);
            return mouseEvent;
        }
    }
}
