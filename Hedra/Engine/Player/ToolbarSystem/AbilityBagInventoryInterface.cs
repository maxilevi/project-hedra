﻿using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class AbilityBagInventoryInterface : InventoryArrayInterface
    {
        private readonly Panel _panel;
        private readonly LocalPlayer _player;

        public AbilityBagInventoryInterface(LocalPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine, Vector2 Spacing, string[] CustomIcons = null) : base(Array, Offset, Length, SlotsPerLine, Spacing, CustomIcons)
        {
            _panel = new Panel();
            _player = Player;
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                this.Buttons[i].Texture.IdPointer = null;
                this.Buttons[i].Texture.TextureId = GUIRenderer.TransparentTexture;
                this.Buttons[i].Scale *= 1.25f;

                this.Array[i] = new Item
                {
                    Model = new VertexData(),
                };
                this.Array[i].SetAttribute("ImageId", 0);
                this.Textures[i].Scale = Vector2.Zero;
            }
        }

        public sealed override void UpdateView()
        {

        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                if (value) _panel.Enable();
                else _panel.Disable();
            }
        }
    }
}