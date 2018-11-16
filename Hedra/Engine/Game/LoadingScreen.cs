using System;
using System.Drawing;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Game
{
    public class LoadingScreen : IUpdatable, IDisposable
    {
        private Texture _loadingScreen;
        private GUIText _playerText;
        private IPlayer _player;
        private float _time;
        private string _text;

        public LoadingScreen(IPlayer Player)
        {
            _player = Player;
            _loadingScreen = new Texture(Color.FromArgb(255, 30, 30, 30), Color.FromArgb(255, 60, 60, 60),
                Vector2.Zero, Vector2.One, GradientType.Diagonal);
            _playerText = new GUIText(string.Empty, new Vector2(0, 0), Color.White, 
                FontCache.Get(AssetManager.NormalFamily, 16, FontStyle.Bold));
            _loadingScreen.TextureElement.Opacity = 0;
            _playerText.UIText.Opacity = 0;
            Log.WriteLine("Created loading screen.");
            
            UpdateManager.Add(this);
        }
        
        public void Update()
        {
            if (IsLoading && IsLoaded()) IsLoading = false;
            this.HandleText();
        }

        private void HandleText()
        {
            if (IsLoading)
            {
                _time += Time.IndependantDeltaTime;
                if (_time >= .75f)
                {
                    _text += ".";
                    _time = 0;
                    if (_text.Contains("...."))
                        _text = Translations.Get("loading");
                }
                _playerText.Text = _text;
                _loadingScreen.TextureElement.Opacity = 1;
                _playerText.UIText.Opacity = 1;
                _playerText.Enable();
                _loadingScreen.Enable();
            }
            else
            {
                _loadingScreen.TextureElement.Opacity = 0;
                _playerText.UIText.Opacity = 0;
            }
        }
        
        private bool IsLoaded()
        {
            var chunkOffset = World.ToChunkSpace(_player.BlockPosition);
            var minRange = -1;
            var maxRange = 2;
#if DEBUG
            minRange = 0;
            maxRange = 1;
#endif
            for (var x = minRange; x < maxRange; x++)
            {
                for (var z = minRange; z < maxRange; z++)
                {
                    var chunk = World.GetChunkByOffset((int) chunkOffset.X + x * Chunk.Width, (int) chunkOffset.Y + z * Chunk.Width);
                    if (!chunk?.BuildedWithStructures ?? true)
                        return false;
                } 
            }
            return true;
        }

        public void Show()
        {
            if(IsLoaded() || (!Program.GameWindow?.SplashScreen?.FinishedLoading ?? true)) return;
            IsLoading = true;
            _text = Translations.Get("loading");
        }
        
        public bool IsLoading { get; private set; }

        public void Dispose()
        {
            UpdateManager.Remove(this);
        }
    }
}