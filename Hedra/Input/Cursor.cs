using System.Numerics;
using Hedra.Engine;
using Hedra.Game;
using Silk.NET.Input;

namespace Hedra.Input
{
    public static class Cursor
    {
        private static bool _show = true;
        public static IMouse Mouse { get; set; }


        public static bool Show
        {
            get => _show;
            set
            {
                if (value == _show) return;
                Program.GameWindow.CursorVisible = value;
                _show = value;
            }
        }

        public static Vector2 Position
        {
            get => Mouse.Position;
            private set => Mouse.Position = new Vector2((int)value.X, (int)value.Y);
        }

        public static void Center()
        {
            Position = new Vector2((float)GameSettings.Width / 2, (float)GameSettings.Height / 2);
        }
    }
}