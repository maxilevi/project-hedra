using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapMarkerHandler
    {
        private readonly LocalPlayer _player;
        private readonly ObjectMesh _selectionBox;
        private readonly int _chunkSize;
        private readonly Panel _panel;
        private readonly GUIText _coordinatesText;
        private bool _enabled;
        public Vector3 Position { get; private set; }

        public MapMarkerHandler(LocalPlayer Player, int ChunkSize)
        {
            _player = Player;
            _chunkSize = ChunkSize;
            var cubeData = new CubeData();
            cubeData.AddFace(Face.ALL);
            cubeData.Color = CubeData.CreateCubeColor(Colors.Red);
            _selectionBox = ObjectMesh.FromVertexData(cubeData.ToVertexData());
            _panel = new Panel();
            _coordinatesText = new GUIText(string.Empty, Vector2.UnitY * .5f, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12f, FontStyle.Bold));
            _panel.AddElement(_coordinatesText);
            EventDispatcher.RegisterMouseDown(this, delegate 
            {
                _player.Minimap.Mark(this.Position);               
            });
        }

        public void Update(Vector3 MapOffset, float BaseHeight)
        {
            _selectionBox.Scale = new Vector3(_chunkSize, _chunkSize * 3f, _chunkSize);
            var targetPosition = _player.View.CameraPosition + 
                _player.View.CrossDirection * ( (_player.View.CameraPosition.Y - BaseHeight) / -_player.View.CrossDirection.Y);

            _selectionBox.Position = new Vector3( 
                (float) Math.Floor(targetPosition.X / _chunkSize) * _chunkSize - (MapOffset.X  / _chunkSize),
                BaseHeight,
                (float) Math.Floor(targetPosition.Z / _chunkSize) * _chunkSize - (MapOffset.Z /_chunkSize)
                );
            _selectionBox.Dither = true;

            Position = (new Vector2(
                (float)Math.Floor( (targetPosition.X- _player.View.CameraPosition.X) / _chunkSize) * _chunkSize,
                (float)Math.Floor( (targetPosition.Z- _player.View.CameraPosition.Z) / _chunkSize) * _chunkSize
            ) / _chunkSize * Chunk.Width + World.ToChunkSpace(_player.Position)).ToVector3();
            _coordinatesText.Text = $"{_selectionBox.Position}";
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                _panel.Enable();
            }
        }
    }
}
