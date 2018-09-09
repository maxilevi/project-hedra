using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Loader
{
    public class SpashScreen
    {
        private GUITexture _studioLogo, _studioBackground;
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
        }
        
    }
}