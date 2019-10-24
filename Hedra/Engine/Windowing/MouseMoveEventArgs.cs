using System.Numerics;

namespace Hedra.Engine.Windowing
{
    public readonly struct MouseMoveEventArgs
    {
                /// <summary>
        /// Initializes a new instance of the <see cref="MouseMoveEventArgs"/> struct.
        /// </summary>
        /// <param name="position">The new mouse position.</param>
        /// <param name="delta">The change in position produced by this event.</param>
        public MouseMoveEventArgs(Vector2 position)
        {
            Position = position;
        }
                
        /// <summary>
        /// Gets the new X position produced by this event.
        /// This position is relative to the top-left corner of the contents of the window.
        /// </summary>
        public float X => Position.X;

        /// <summary>
        /// Gets the new Y position produced by this event.
        /// This position is relative to the top-left corner of the contents of the window.
        /// </summary>
        public float Y => Position.Y;

        /// <summary>
        /// Gets the new position produced by this event.
        /// This position is relative to the top-left corner of the contents of the window.
        /// </summary>
        public Vector2 Position { get; }
    }
}