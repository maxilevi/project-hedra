using System;
using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestDialog : InventoryInterfaceItemInfo
    {
        private Action _acceptCallback;
        private QuestObject _quest;
        private float _descriptionHeight;
        private readonly Vector2 _itemAttributesPosition;
        private readonly Vector2 _itemDescriptionPosition;
        private readonly IPlayer _player;
        
        public QuestDialog(IPlayer Player, InventoryItemRenderer Renderer) : base(Renderer)
        {
            _player = Player;
            Position = Vector2.UnitY * .075f;
            var boldFont = FontCache.Get(AssetManager.BoldFamily, 11f, FontStyle.Bold);
            var id = Graphics2D.LoadFromAssets("Assets/UI/InventoryBackground.png");
            var buttonScale = HintTexture.Scale * new Vector2(1f, 1.25f);
            var acceptButton = new Button(new Vector2(.1f, HintTexture.Position.Y - buttonScale.Y * 2), buttonScale, id);
            var acceptText = new GUIText(Translation.Create("accept_quest"), acceptButton.Position, Color.White, boldFont);
            var declineButton = new Button(acceptButton.Position * new Vector2(-1, 1), buttonScale, id);
            var declineText = new GUIText(Translation.Create("decline_quest"), declineButton.Position, Color.White, boldFont);
            HintTexture.Position += Vector2.UnitY * (BackgroundTexture.Scale.Y * 2 + HintTexture.Scale.Y * 2);
            HintText.Position = HintTexture.Position;
            HintText.TextFont = boldFont;
            ItemAttributes.TextFont = FontCache.Get(AssetManager.BoldFamily, 10f, FontStyle.Bold);
            ItemAttributes.Color = Color.White;
            ItemDescription.TextFont = FontCache.Get(AssetManager.BoldFamily, 10f, FontStyle.Bold);
            ItemDescription.Color = Color.White;
            ItemText.TextFont = FontCache.Get(AssetManager.BoldFamily, 13f, FontStyle.Bold);
            ItemDescription.Position = _itemDescriptionPosition = new Vector2(BackgroundTexture.Scale.X * .5f, ItemDescription.Position.Y);
            ItemAttributes.Position = _itemAttributesPosition = new Vector2(-1, 1) * ItemDescription.Position;
            Panel.AddElement(acceptButton);
            Panel.AddElement(acceptText);
            Panel.AddElement(declineButton);
            Panel.AddElement(declineText);
            var largerFont = FontCache.Get(acceptText.TextFont.FontFamily, 12f, acceptText.TextFont.Style);
            acceptButton.HoverEnter += (S, A) =>
            {
                acceptText.TextColor = Color.Orange;
                acceptText.TextFont = largerFont;
            };
            acceptButton.HoverExit += (S, A) =>
            {
                acceptText.TextColor = Color.White;
                acceptText.TextFont = boldFont;
            };
            declineButton.HoverEnter += (S, A) =>
            {
                declineText.TextColor = Color.Orange;
                declineText.TextFont = largerFont;
            };
            declineButton.HoverExit += (S, A) =>
            {
                declineText.TextColor = Color.White;
                declineText.TextFont = boldFont;
            };
            acceptButton.Click += (O1,O2) => AcceptQuest();
            declineButton.Click += (O1, O2) => DeclineQuest();
        }

        protected override void UpdateView()
        {
            UpdateItemMesh();
            ItemText.Text = _quest.DisplayName;
            ItemAttributes.Text = $"{Translations.Get("quest_difficulty")}: {Translations.Get($"quest_{_quest.Tier.ToString().ToLowerInvariant()}")}";
            ItemDescription.Text = _quest.Description;
            ItemTexture.Position = Position;
            ItemTexture.Scale = WeaponItemTextureScale;
            ItemDescription.Position = _itemDescriptionPosition;
            ItemAttributes.Position = _itemAttributesPosition;
            _descriptionHeight = ItemDescription.Scale.Y * 2;
            
            AccomodateScale(ItemTexture);
            AccomodatePosition(ItemTexture);
            //AccomodatePosition(ItemAttributes);
            //AccomodatePosition(ItemDescription);


            HintTexture.Enable();
            HintText.Enable();
            HintText.Text = Translations.Get("quest");
            AddTexture();
        }

        protected override float DescriptionHeight => _descriptionHeight;

        private void AcceptQuest()
        {
            _acceptCallback.Invoke();
            PlayerInterface.CloseCurrent();
            _player.Questing.Start(_quest);
        }

        private static void DeclineQuest()
        {
            PlayerInterface.CloseCurrent();
        }
        
        public void Show(QuestObject Object, Action AcceptCallback)
        {
            _acceptCallback = AcceptCallback;
            _quest = Object;
            base.Show(Object.ToItem());
        }
    }
}