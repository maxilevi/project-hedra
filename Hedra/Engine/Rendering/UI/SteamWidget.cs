using System;
using OpenTK;

namespace Hedra.Engine.Rendering.UI
{
    public class SteamWidget : Panel
    {
        private readonly Texture _storeTexture;
        private readonly Button _openInAppButton;
        private readonly Button _openInStore;
        
        public SteamWidget()
        {
            _storeTexture = new Texture(Graphics2D.LoadFromAssets("Assets/UI/SteamTexture.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/SteamTexture.png").As1920x1080() * .75f);
            var buttonSize = Graphics2D.SizeFromAssets("Assets/UI/SteamButtonOpenLink.png").As1920x1080() * .85F;
            _openInAppButton = new Button(
                _storeTexture.Position - _storeTexture.Scale.Y * Vector2.UnitY * 1.25f + buttonSize.X * Vector2.UnitX * 1.25f,
                buttonSize,
                Graphics2D.LoadFromAssets("Assets/UI/SteamButtonOpenApp.png")
            );
            _openInAppButton.HoverEnter += (A, B) => _openInAppButton.Scale *= 1.1f;
            _openInAppButton.HoverExit += (A, B) => _openInAppButton.Scale *= 1.0f / 1.1f;          
            _openInAppButton.Click += (A, B) => OpenStoreWithApp();
            
            _openInStore = new Button(_openInAppButton.Position * new Vector2(-1, 1), buttonSize, Graphics2D.LoadFromAssets("Assets/UI/SteamButtonOpenLink.png"));
            _openInStore.Click += (A, B) => OpenStore();
            _openInStore.HoverEnter += (A, B) => _openInStore.Scale *= 1.1f;
            _openInStore.HoverExit += (A, B) => _openInStore.Scale *= 1.0f / 1.1f;
            
            AddElement(_storeTexture);
            AddElement(_openInStore);
            AddElement(_openInAppButton);
            for (var i = 0; i < Elements.Count; ++i)
            {
                Elements[i].Position += Vector2.UnitX * .6f + Vector2.UnitY * .15F;
            }
        }

        private static void OpenStoreWithApp()
        {
            OpenLink("steam://store/1009960");
        }
        
        public static void OpenStore()
        {
            OpenLink("https://store.steampowered.com/app/1009960/Project_Hedra/");
        }
        
        private static void OpenLink(string Link)
        {
            System.Diagnostics.Process.Start(Link);
        }
    }
}