using Hedra.Engine.Events;
using Hedra.Engine.Game;
using OpenTK;

namespace Hedra.Engine.Input
{
    public static class Cursor
    {
        private static bool _show = true;
        private static Vector2 _position;

        static Cursor()
        {
            EventDispatcher.RegisterMouseMove(
                typeof(Cursor),
                (sender, args) => _position = new Vector2(args.Position.X, args.Position.Y)
            );
        }
        
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
            get => _position;
            set => OpenTK.Input.Mouse.SetPosition(_position.X, _position.Y);
        }
    }
}