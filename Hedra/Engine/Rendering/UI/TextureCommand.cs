using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    internal class TextureCommand
    {
        public IRenderable Renderable { get; set; }
        public DrawOrder Order { get; set; }

        public TextureCommand(IRenderable Renderable, DrawOrder Order)
        {
            this.Renderable = Renderable;
            this.Order = Order;
        }
    }

    public enum DrawOrder
    {
        Before,
        After
    }
}
