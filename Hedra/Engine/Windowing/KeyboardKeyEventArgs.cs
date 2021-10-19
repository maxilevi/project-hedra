using System.Windows.Forms;
using Silk.NET.GLFW;
using Silk.NET.Input;

namespace Hedra.Engine.Windowing
{
    public readonly struct KeyboardKeyEventArgs
    { 
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardKeyEventArgs"/> struct.
        /// </summary>
        /// <param name="key">The key that generated this event.</param>
        /// <param name="modifiers">The key modifiers that were active when this event was generated.</param>
        public KeyboardKeyEventArgs(Key key, KeyModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Gets the key that generated this event.
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// Gets a bitwise combination representing the key modifiers were active when this event was generated.
        /// </summary>
        public KeyModifiers Modifiers { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="KeyModifiers.Alt" /> is pressed.
        /// </summary>
        /// <value><c>true</c> if pressed; otherwise, <c>false</c>.</value>
        public bool Alt => Modifiers.HasFlag(KeyModifiers.Alt);

        /// <summary>
        /// Gets a value indicating whether <see cref="System.Windows.Forms.Control" /> is pressed.
        /// </summary>
        /// <value><c>true</c> if pressed; otherwise, <c>false</c>.</value>
        public bool Control => Modifiers.HasFlag(KeyModifiers.Control);

        /// <summary>
        /// Gets a value indicating whether <see cref="KeyModifiers.Shift" /> is pressed.
        /// </summary>
        /// <value><c>true</c> if pressed; otherwise, <c>false</c>.</value>
        public bool Shift => Modifiers.HasFlag(KeyModifiers.Shift);
    }
}