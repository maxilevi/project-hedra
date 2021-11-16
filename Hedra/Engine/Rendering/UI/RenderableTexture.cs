/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 18/03/2017
 * Time: 09:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     IRenderable wrapper
    /// </summary>
    public class RenderableTexture : IRenderable, UIElement, IAdjustable
    {
        public RenderableTexture(BackgroundTexture BaseTexture, DrawOrder Order)
        {
            this.BaseTexture = BaseTexture;
            DrawManager.UIRenderer.Remove(BaseTexture.TextureElement);
            DrawManager.UIRenderer.Add(this, Order);
        }

        public BackgroundTexture BaseTexture { get; set; }

        public void Adjust()
        {
            BaseTexture.TextureElement.Adjust();
        }

        public void Draw()
        {
            if (!BaseTexture.Enabled || BaseTexture.TextureElement.IdPointer == null &&
                BaseTexture.TextureElement.TextureId == GUIRenderer.TransparentTexture) return;
            DrawManager.UIRenderer.Draw(BaseTexture.TextureElement);
        }

        public void Enable()
        {
            BaseTexture.Enable();
        }

        public void Disable()
        {
            BaseTexture.Disable();
        }

        public Vector2 Position
        {
            get => BaseTexture.Position;
            set => BaseTexture.Position = value;
        }

        public Vector2 Scale
        {
            get => BaseTexture.Scale;
            set => BaseTexture.Scale = value;
        }

        public void Dispose()
        {
            BaseTexture?.Dispose();
        }
    }
}