using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    public class TextureCommand
    {
        public TextureCommand(IRenderable Renderable, DrawOrder Order)
        {
            this.Renderable = Renderable;
            this.Order = Order;
        }

        public IRenderable Renderable { get; set; }
        public DrawOrder Order { get; set; }
    }

    public enum DrawOrder
    {
        Before,
        After
    }
}