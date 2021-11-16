using System.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;

namespace Hedra.Engine.Rendering.UI
{
    public class HelpUI : Panel
    {
        private readonly BackgroundTexture _helpTexture;

        public HelpUI()
        {
            _helpTexture = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/HelpTexture.png"), Vector2.Zero,
                Graphics2D.SizeFromAssets("Assets/UI/HelpTexture.png").As1920x1080() * new Vector2(0.75f, 0.75f)
            );
            AddElement(_helpTexture);
        }
    }
}