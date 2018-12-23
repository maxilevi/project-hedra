using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryArrayInterface : InventoryArrayInterface
    {
        private readonly RenderableTexture[] _recipeSelectedTextures;
        private readonly Panel _panel;
        
        public CraftingInventoryArrayInterface(InventoryArray Array, int Length) : base(Array, 0, Length, 4, Vector2.One)
        {
            _panel = new Panel();
            _recipeSelectedTextures = new RenderableTexture[Buttons.Length];
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                _recipeSelectedTextures[i] = 
                    new RenderableTexture(
                        new Texture(Graphics2D.LoadFromAssets("Assets/UI/SelectedInventorySlot.png"), this.Textures[i].Position, this.Textures[i].Scale),
                        DrawOrder.After
                    );
                _panel.AddElement(_recipeSelectedTextures[i]);
            }
        }

        public override void UpdateView()
        {
            if (Enabled)
            {
                for (var i = 0; i < _recipeSelectedTextures.Length; i++)
                {
                    _recipeSelectedTextures[i].Disable();
                }
                _recipeSelectedTextures[SelectedRecipeIndex].Enable();
            }
            Renderer.UpdateView();           
        }
        
        public int SelectedRecipeIndex { get; set; }
        
        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (this.Enabled) _panel.Enable();
                else _panel.Disable();            
            }
        }

        public override Vector2 Scale
        {
            get => base.Scale;
            set
            {
                for (var i = 0; i < _recipeSelectedTextures.Length; i++)
                {
                    _recipeSelectedTextures[i].Scale = new Vector2(_recipeSelectedTextures[i].Scale.X / base.IndividualScale.X,
                                                       _recipeSelectedTextures[i].Scale.Y / base.IndividualScale.Y) * value;
                    var relativePosition = _recipeSelectedTextures[i].Position - Position;
                    _recipeSelectedTextures[i].Position = new Vector2(relativePosition.X / base.Scale.X,
                                                                    relativePosition.Y / base.Scale.Y) * value + Position;
                }
                base.Scale = value;
            }
        }

        public override Vector2 IndividualScale
        {
            get => base.IndividualScale;
            set
            {
                for (var i = 0; i < _recipeSelectedTextures.Length; i++)
                {
                    _recipeSelectedTextures[i].Scale = new Vector2(_recipeSelectedTextures[i].Scale.X / base.IndividualScale.X,
                                                       _recipeSelectedTextures[i].Scale.Y / base.IndividualScale.Y) * value;
                }
                base.IndividualScale = value;
            }
        }

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                for (var i = 0; i < _recipeSelectedTextures.Length; i++)
                {
                    _recipeSelectedTextures[i].Position = _recipeSelectedTextures[i].Position - base.Position + value;
                }
                base.Position = value;
            }
        }      
    }
}