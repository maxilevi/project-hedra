using System;
using System.Numerics;
using BulletSharp;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Loader
{
    public class SplashScreen
    {
        private readonly GUITexture _studioBackground;
        private readonly GUIText _loadingText;
        private float _splashOpacity = 1;
        private int _loadingCounter;

        public SplashScreen()
        {
            _studioBackground = new GUITexture(Graphics2D.LoadFromAssets("Assets/splash-background.png"), Vector2.One,
                Vector2.Zero)
            {
                Enabled = true,
                Opacity = 1
            };

            _loadingText = new GUIText("Loading...", new Vector2(0, -0.5f), Color.White, FontCache.GetBold(32));
            _loadingText.Enable();
        }

        public void Disable()
        {
            FinishedLoading = true;
        }

        public bool FinishedLoading { get; private set; }

        public void Update()
        {
        }

        private string GetText()
        {
            var dotCount = _loadingCounter++;
            _loadingCounter %= 4;
            var dots = String.Empty;
            for (var i = 0; i < dotCount; ++i)
            {
                dots += ".";
            }
            return Translations.Get("loading") + dots;
        }

        public void Draw()
        {
            Renderer.Viewport(0, 0, GameSettings.DeviceWidth, GameSettings.DeviceHeight);
            DrawManager.UIRenderer.Draw(_studioBackground);
            
            _loadingText.Text = GetText();
            _loadingText.RemoveFromRenderer();
            DrawManager.UIRenderer.Draw(_loadingText.UIText);
        }
    }
}