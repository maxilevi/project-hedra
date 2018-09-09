using System;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Loader
{
    public class SplashScreen
    {
        private readonly GUITexture _studioLogo;
        private readonly GUITexture _studioBackground;
        private bool _finishedLoading;
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
            
#if !DEBUG
            TaskManager.After(6000, () => _splashOpacity = 0);
#endif
#if DEBUG
            this._finishedLoading = true;
#endif

        }

        public void Update()
        {
            if (!this._finishedLoading)
            {
                _studioBackground.Opacity = Mathf.Lerp(_studioBackground.Opacity, _splashOpacity, Time.IndependantDeltaTime);
                _studioLogo.Opacity = Mathf.Lerp(_studioLogo.Opacity, _splashOpacity, Time.IndependantDeltaTime);

                if (_splashOpacity < 0.05f && Math.Abs(_studioLogo.Opacity - _splashOpacity) < 0.05f)
                {
                    this._finishedLoading = true;
                }
            }
        }

        public void Draw()
        {
            DrawManager.UIRenderer.Draw(_studioBackground);
            DrawManager.UIRenderer.Draw(_studioLogo);
        }

        public bool FinishedLoading => _finishedLoading;
    }
}