using System.Numerics;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering.UI;

namespace Hedra.Engine.Player.Inventory
{
    public abstract class BaseBackground
    {
        protected readonly Panel Panel;
        protected readonly BackgroundTexture Texture;
        private bool _enabled;

        protected BaseBackground(Vector2 Position, Vector2 Scale)
        {
            this.Scale = Scale;
            Texture = new BackgroundTexture(InventoryBackground.DefaultId, Position,
                InventoryBackground.DefaultSize * Scale);
            Panel = new Panel();
            Panel.AddElement(Texture);
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (_enabled)
                    Panel.Enable();
                else
                    Panel.Disable();
            }
        }

        public Vector2 Scale { get; }

        protected Vector2 Position
        {
            get => Texture.Position;
            private set => Texture.Position = value;
        }
    }
}