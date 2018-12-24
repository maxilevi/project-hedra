using System;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.CraftingSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryItemInfo : InventoryInterfaceItemInfo
    {
        private readonly Button _craftButton;
        private Item _currentRecipe;
        private readonly IPlayer _player;
        private string _lastRecipeChecked;        
        private bool _canCraft;
        private readonly Timer _cooldownTimer;
        private readonly Vector4 _normalTint;
        private readonly Vector4 _cooldownTint;

        public CraftingInventoryItemInfo(IPlayer Player, InventoryItemRenderer Renderer) : base(Renderer)
        {
            _player = Player;
            _normalTint = new Vector4(Color.Orange.ToVector4().Xyz * 5f, 1);
            _cooldownTint = new Vector4(Color.Orange.ToVector4().Xyz * 2.5f, 1);
            _cooldownTimer = new Timer(1.75f)
            {
                AutoReset = false,
            };
            _cooldownTimer.MakeReady();
            HintTexture.TextureElement.Grayscale = true;
            HintTexture.Position = Vector2.UnitY * -.45f;
            HintTexture.Scale *= 1.5f;
            HintText.TextColor = Color.White;
            HintText.Position = HintTexture.Position;
            HintText.TextFont = FontCache.Get(AssetManager.BoldFamily, 14, FontStyle.Bold);
            _craftButton = new Button(HintTexture.Position, HintTexture.Scale, GUIRenderer.TransparentTexture);
            _craftButton.Click += (O, E) => Craft(); 
            _player.Inventory.InventoryUpdated += () =>
            {
                if(_currentRecipe != null)
                    UpdateCanCraft();
            };
        }


        public bool Craft()
        {
            if (_canCraft && _cooldownTimer.Ready)
            {
                _player.Crafting.Craft(_currentRecipe, _player.Position);
                UpdateView();
                _cooldownTimer.Reset();
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
                return true;
            }
            _player.MessageDispatcher.ShowNotification(_canCraft 
                ? Translations.Get("is_on_cooldown") 
                : Translations.Get("insufficient_ingredients"), Color.Red, 3, true);
            return false;
        }

        public void Update()
        {
            var ready = _cooldownTimer.Tick();
            HintText.TextColor = ready ? Color.White : Color.Gray;
            HintTexture.TextureElement.Tint = Mathf.Lerp(HintTexture.TextureElement.Tint, ready ? _normalTint : _cooldownTint, Time.DeltaTime * 8f);
        }
        
        public void Show(Item Output, Item Recipe)
        {
            _currentRecipe = Recipe;
            base.Show(Output);
        }

        protected override void AddLayout()
        {
            AddNormalLayout();
            if (_currentRecipe != null && _lastRecipeChecked != _currentRecipe.Name)
            {
                UpdateCanCraft();
            }
            ItemAttributes.Color = _canCraft ? Color.LawnGreen : Color.Red;
        }

        protected override void AddAttributes()
        {
            var ingredients = CraftingInventory.GetIngredients(_currentRecipe);
            ItemAttributes.Text = $@"{Translations.Get("ingredients")}:{Environment.NewLine}{ 
                ingredients.Select(
                    I => $@"{new string(' ', 4)}• {
                        _player.Inventory.Search(T => T.Name == I.Name)?.GetAttribute<int>(CommonAttributes.Amount) ?? 0
                    }/{I.Amount} {ItemPool.Grab(I.Name).DisplayName}"
                ).Aggregate((S1,S2) => $"{S1}{Environment.NewLine}{S2}")
            }";
        }

        protected override void AddHint()
        {
            HintText.Text = Translations.Get("craft");
            HintTexture.Enable();
            HintText.Enable();
        }

        private void UpdateCanCraft()
        {
            _canCraft = _player.Crafting.CanCraft(_currentRecipe);
            _lastRecipeChecked = _currentRecipe.Name;
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                if(value) _craftButton.Enable();
                else _craftButton.Disable();
                base.Enabled = value;
            }
        }

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                _craftButton.Position = _craftButton.Position - Position + value;
                base.Position = value;
            }
        }
    }
}