using Hedra.Engine;
using Hedra.Game;
using System.Numerics;
using Forms = System.Windows.Forms;

namespace Hedra.Input
{
    public static class Cursor
    {
        private static bool _show = true;
        private static Vector2 _position;

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
            get => new Vector2(Forms.Cursor.Position.X, Forms.Cursor.Position.Y);
            set => Forms.Cursor.Position = new System.Drawing.Point((int)value.X, (int)value.Y);
        }
    }
}