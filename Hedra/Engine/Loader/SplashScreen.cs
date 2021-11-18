using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.Loader
{
    public class SplashScreen
    {
        private readonly GUITexture _studioBackground;
        private readonly GUITexture _studioLogo;
        private float _splashOpacity = 1;

        public SplashScreen()
        {
            _studioLogo = new GUITexture(Graphics2D.LoadFromAssets("Assets/splash-logo.png"),
                Graphics2D.SizeFromAssets("Assets/splash-logo.png"), Vector2.Zero)
            {
                Enabled = true,
                Opacity = 0
            };

            _studioBackground = new GUITexture(Graphics2D.LoadFromAssets("Assets/splash-background.png"), Vector2.One,
                Vector2.Zero)
            {
                Enabled = true,
                Opacity = 0
            };

//#if !DEBUG
            //TaskScheduler.After(4, () => _splashOpacity = 0);
//#endif
//#if DEBUG
            //FinishedLoading = true;
//#endif
        }

        public void Disable()
        {
            _splashOpacity = 0;
        }

        public bool FinishedLoading { get; private set; }

        public void Update()
        {
            if (!FinishedLoading)
            {
                _studioBackground.Opacity =
                    Mathf.Lerp(_studioBackground.Opacity, _splashOpacity, Time.IndependentDeltaTime);
                _studioLogo.Opacity = Mathf.Lerp(_studioLogo.Opacity, _splashOpacity, Time.IndependentDeltaTime);

                if (_splashOpacity < 0.05f && Math.Abs(_studioLogo.Opacity - _splashOpacity) < 0.05f)
                    FinishedLoading = true;
            }
        }

        public void Draw()
        {
            Renderer.Viewport(0, 0, GameSettings.Width, GameSettings.Height);
            DrawManager.UIRenderer.Draw(_studioBackground);
        }
    }
}