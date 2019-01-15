using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public abstract class BaseBackground
    {
        protected readonly Texture Texture;
        protected readonly Panel Panel;
        private bool _enabled;

        protected BaseBackground(Vector2 Position, Vector2 Scale)
        {
            this.Scale = Scale;
            Texture = new Texture("Assets/UI/InventoryBackground.png", Position, Scale);                       
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