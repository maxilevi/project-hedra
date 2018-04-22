using System;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.MapSystem
{ 
    public delegate void OnMapMoveEventHandler();

    public class MapInputHandler : IDisposable
    {
        public event OnMapMoveEventHandler OnMove;
        public Vector3 Position { get; private set; }
        public bool Enabled { get; set; }
        private readonly LocalPlayer _player;
        private Vector3 _targetPosition;

        public MapInputHandler(LocalPlayer Player)
        {
            _player = Player;
        }

        public void Update()
        {
            if (!Enabled) return;
            var cameraOrientation = _player.View.CrossDirection.Xz.ToVector3();
            var moved = Program.GameWindow.Keyboard[Key.D] ||
                        Program.GameWindow.Keyboard[Key.A] ||
                        Program.GameWindow.Keyboard[Key.W] ||
                        Program.GameWindow.Keyboard[Key.S];

            if (Program.GameWindow.Keyboard[Key.D])
                _targetPosition += -Vector3.Cross(Vector3.UnitY, cameraOrientation) * (float)Time.deltaTime * 250f;

            if (Program.GameWindow.Keyboard[Key.A])
                _targetPosition += Vector3.Cross(Vector3.UnitY, cameraOrientation) * (float)Time.deltaTime * 250f;

            if (Program.GameWindow.Keyboard[Key.W])
                _targetPosition += cameraOrientation * (float)Time.deltaTime * 250f;

            if (Program.GameWindow.Keyboard[Key.S])
                _targetPosition += -cameraOrientation * (float)Time.deltaTime * 250f;
            Position = Mathf.Lerp(Position, _targetPosition, Time.FrameTimeSeconds * 16f);
            if(moved) OnMove?.Invoke();
        }

        public void Reset()
        {
            _targetPosition = Vector3.Zero;
            Position = Vector3.Zero;
        }

        public void Dispose()
        {

        }
    }
}
