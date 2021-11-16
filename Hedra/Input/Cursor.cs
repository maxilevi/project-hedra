using Hedra.Engine;
using Hedra.Game;
using System.Numerics;
using Silk.NET.Input;

namespace Hedra.Input
{
    public static class Cursor
    {
        public static IMouse Mouse { get; set; }

        private static bool _show = true;

        public static void Center()
        {
            Position = new Vector2((float) GameSettings.Width / 2, (float) GameSettings.Height / 2);
        }
        
        
        public static bool Show
        {
            get => _show;
            set
            {
                if (value == _show)
                {
                    return;
                }
                Program.GameWindow.CursorVisible = value;
                _show = value;
            }
        }

        public static Vector2 Position
        {
            get => Mouse.Position;
            private set => Mouse.Position = new Vector2((int)value.X, (int)value.Y);
        }
    }
}