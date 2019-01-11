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
        private const int MaxIngredients = 4;
        private readonly Button _craftButton;
        private Item _currentRecipe;
        private readonly IPlayer _player;
        private string _lastRecipeChecked;        
        private bool _canCraft;
        private readonly Timer _cooldownTimer;
        private readonly Vector4 _normalTint;
        private readonly Vector4 _cooldownTint;
        private readonly GUIText[] _ingredientsText;
        private readonly Panel _panel;
        private float _descriptionHeight;

        public CraftingInventoryItemInfo(IPlayer Player, InventoryItemRenderer Renderer) : base(Renderer)
        {
            _player = Player;
            _normalTint = new Vector4(Color.Orange.ToVector4().Xyz * 5f, 1);
            _cooldownTint = new Vector4(Color.Orange.ToVector4().Xyz * 2f, 1);
            _cooldownTimer = new Timer(.5f)
            {
                AutoReset = false,
            };
            _cooldownTimer.MakeReady();
            _panel = new Panel();
            _ingredientsText = new GUIText[MaxIngredients+1];
            for (var i = 0; i < _ingredientsText.Length; i++)
            {
                _ingredientsText[i] = new GUIText(
                    string.Empty, ItemAttributes.Position, ItemAttributes.Color, ItemAttributes.TextFont
                );
                _panel.AddElement(_ingredientsText[i]);
            }
            HintTexture.TextureElement.Grayscale = true;
            HintTexture.Position = BackgroundTexture.Position - DefaultSize.Y * Vector2.UnitY * .75f;
            HintTexture.Scale *= 1.5f;
            HintText.TextColor = Color.White;
            HintText.Position = HintTexture.Position;
            HintText.TextFont = FontCache.Get(AssetManager.BoldFamily, 14, FontStyle.Bold);
            _craftButton = new Button(HintTexture.Position, HintTexture.Scale, GUIRenderer.TransparentTexture);
            _craftButton.Click += (O, E) => Craft(); 
            _panel.AddElement(_craftButton);
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
                _player.Crafting.CraftItem(_currentRecipe, _player.Position);
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
            var ready = _cooldownTimer.Tick() && _canCraft;
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
            if (_currentRecipe != null && _lastRecipeChecked != _currentRecipe.Name)
            {
                UpdateCanCraft();
            }
        }

        protected override void AddAttributes()
        {
            _descriptionHeight = 0;
            AddNormalLayout();
            var ingredients = CraftingInventory.GetIngredients(_currentRecipe);
            ItemAttributes.Text = $@"{Translations.Get("ingredients")}:";
            ItemAttributes.Position -= Vector2.UnitX * ItemAttributes.Scale.X;
            if (ingredients.Length > MaxIngredients) 
                throw new ArgumentOutOfRangeException($"Cannot display a recipe with more than {MaxIngredients} ingredients, has {ingredients.Length}");

            DisableIngredientsText();
            var offset = -ItemAttributes.Scale.Y * 2f;
            for (var i = 0; i < ingredients.Length; i++)
            {
                var k = i;
                var asItem = _player.Inventory.Search(T => T.Name == ingredients[k].Name);
                var currentAmount = asItem?.GetAttribute<int>(CommonAttributes.Amount) ?? 0;
                var ingredientName = asItem?.DisplayName ?? ItemPool.Grab(ingredients[i].Name).DisplayName;
                _ingredientsText[i].Position = ItemAttributes.Position + Vector2.UnitY * offset;
                _ingredientsText[i].Text =
                    $@"{new string(' ', 4)}• {currentAmount}/{ingredients[i].Amount} {ingredientName}";
                _ingredientsText[i].Position += Vector2.UnitX * _ingredientsText[i].Scale.X - Vector2.UnitX * ItemAttributes.Scale.X;
                _ingredientsText[i].TextColor = currentAmount >= ingredients[i].Amount ? Color.LawnGreen : Color.Red;
                _ingredientsText[i].Enable();
                offset -= _ingredientsText[i].Scale.Y * 2;
            }
            AddStationRequirement(Vector2.UnitY * offset);
            _descriptionHeight = _ingredientsText.Where(I => I.UIText.Enabled).Sum(I => I.Scale.Y) + base.DescriptionHeight;
            AccomodateScale(ItemTexture);
            AccomodatePosition(ItemTexture);
            AccomodatePosition(ItemAttributes);
            for (var i = 0; i < _ingredientsText.Length; i++)
            {
                AccomodatePosition(_ingredientsText[i]);
            }
        }

        private void AddStationRequirement(Vector2 Offset)
        {
            if(_currentRecipe.GetAttribute<CraftingStation>(CommonAttributes.CraftingStation) == CraftingStation.None) return;
            StationRequirementText.Position = ItemAttributes.Position + Offset;
            StationRequirementText.Enable();
            StationRequirementText.Text =
                $"• {Translations.Get($"requires_{_currentRecipe.GetAttribute<string>(CommonAttributes.CraftingStation).ToLowerInvariant()}")}";
            StationRequirementText.TextColor =
                CraftingInventory.IsInStation(_currentRecipe, _player.Position) ? Color.LawnGreen : Color.Red;
            StationRequirementText.Position += Vector2.UnitX * StationRequirementText.Scale.X - Vector2.UnitX * ItemAttributes.Scale.X;
        }

        private GUIText StationRequirementText => _ingredientsText[MaxIngredients - 1]; 
        
        protected override float DescriptionHeight => _descriptionHeight;

        private void DisableIngredientsText()
        {
            for (var i = 0; i < _ingredientsText.Length; i++)
            {
                _ingredientsText[i].Disable();
            }
        }

        protected override void AddHint()
        {
            HintText.Text = Translations.Get("craft");
            HintTexture.Enable();
            HintText.Enable();
        }

        private void UpdateCanCraft()
        {
            _canCraft = _player.Crafting.CanCraft(_currentRecipe, _player.Position);
            _lastRecipeChecked = _currentRecipe.Name;
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                if(value) _panel.Enable();
                else _panel.Disable();
                base.Enabled = value;
            }
        }

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                var elements = _panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Position = elements[i].Position - base.Position + value;
                }
                base.Position = value;
            }
        }
    }
}