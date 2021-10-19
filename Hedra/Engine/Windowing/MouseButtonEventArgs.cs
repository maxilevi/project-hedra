using System.Numerics;
using Silk.NET.GLFW;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Hedra.Engine.Windowing
{
    public readonly struct MouseButtonEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseButtonEventArgs"/> struct.
        /// </summary>
        /// <param name="button">The mouse button for the event.</param>
        /// <param name="action">The action of the mouse button.</param>
        /// <param name="modifiers">The key modifiers held during the mouse button's action.</param>
        public MouseButtonEventArgs(MouseButton button, InputAction action, Vector2 position)
        {
            Button = button;
            Action = action;
            Position = position;
        }

        /// <summary>
        /// Gets the <see cref="MouseButton" /> that triggered this event.
        /// </summary>
        public MouseButton Button { get; }

        /// <summary>
        /// Gets the <see cref="InputAction"/> of the pressed button.
        /// </summary>
        public InputAction Action { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Button"/> which triggered this event was pressed or released.
        /// </summary>
        public bool IsPressed => Action != InputAction.Release;
        public Vector2 Position { get; }
    }
}