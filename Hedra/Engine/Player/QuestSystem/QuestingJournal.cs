using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.PagedInterface;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player.QuestSystem
{
    public sealed class QuestingJournal : PagedInventoryArrayInterface
    {
        private readonly Texture _journalBackground;
        private readonly GUIText _descriptionText;
        private readonly Texture _renderTexture;
        private readonly Vector2 _descriptionPosition;
        private readonly Button _abandonButton;
        private readonly GUIText _abandonText;
        private ObjectMesh _currentItemMesh;
        private float _currentItemMeshHeight;
        
        public QuestingJournal(IPlayer Player) 
            : base(Player, null, 0, 0, Vector2.One)
        {
            _journalBackground = 
                new Texture("Assets/UI/InventoryItemInfo.png", Position, Vector2.One * .55f);
            _journalBackground.SendBack();
            _descriptionText = new GUIText(
                string.Empty,
                _descriptionPosition = -Vector2.UnitY * _journalBackground.Scale.Y * .65f,
                Color.White,
                FontCache.Get(AssetManager.NormalFamily, 11)
            );
            _renderTexture = new Texture(
                0,
                _journalBackground.Position + Vector2.UnitY * _journalBackground.Scale.Y * .25f,
                _journalBackground.Scale * .4f
            );
            _renderTexture.TextureElement.IdPointer = () => Renderer.Draw(_currentItemMesh, false,
                false, _currentItemMeshHeight * InventoryItemRenderer.ZOffsetFactor);
            var abandonSize = Graphics2D.SizeFromAssets("Assets/UI/AbandonButton.png") * .4f;
            _abandonButton = new Button(
                _journalBackground.Position - Vector2.UnitY * (_journalBackground.Scale.Y + abandonSize.Y),
                abandonSize,
                Graphics2D.LoadFromAssets("Assets/UI/AbandonButton.png")
            );
            _abandonButton.Click += (O, E) =>
            {
                Player.Questing.Abadon(CurrentQuest);
                UpdateView();
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            };
            _abandonText = new GUIText(
                Translation.Create("abandon_quest"),
                _abandonButton.Position,
                Color.White,
                FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold)
            );
            Panel.AddElement(_abandonButton);
            Panel.AddElement(_abandonText);
            Panel.AddElement(_renderTexture);
            Panel.AddElement(_descriptionText);
            Panel.AddElement(_journalBackground);

            var leftJournalTopCorner = -Vector2.UnitX * _journalBackground.Scale.X 
                                       + Vector2.UnitY * (_journalBackground.Scale.Y + TitleText.Scale.Y * .75f);
            TitleText.Position += Vector2.UnitX * TitleText.Scale.X * 1.25f;
            TitleText.Position += leftJournalTopCorner;
            var rightJournalCorner = leftJournalTopCorner * new Vector2(-1, 1) 
                - Vector2.UnitX * 1.25f * (PageSelector.Scale.X + NextPageText.Scale.X + PreviousPageText.Scale.X + CurrentPageText.Scale.X);
            PageSelector.Position += rightJournalCorner;
            NextPageText.Position += rightJournalCorner;
            PreviousPageText.Position += rightJournalCorner;
            CurrentPageText.Position += rightJournalCorner;
        }

        public override void UpdateView()
        {
            UpdatePages(Quests.Length);
            UpdateItemMesh();
            Title.Disable();
            _descriptionText.Text = CurrentQuest.Description;
            _descriptionText.Position = Position + _descriptionPosition;
        }

        private void UpdateItemMesh()
        {
            _currentItemMesh?.Dispose();
            _currentItemMesh = 
                InventoryItemRenderer.BuildModel(CurrentQuest.BuildPreview(), out _currentItemMeshHeight);
        }
        
        private QuestObject CurrentQuest => Quests[CurrentPage];

        protected override Translation TitleTranslation => Translation.Create("quests");

        private QuestObject[] Quests => Player.Questing.ActiveQuests;
    }
}