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
        private readonly Texture _renderBackground;
        private readonly IPlayer _player;
        
        public QuestingJournal(IPlayer Player) 
            : base(Player, null, 0, 0, Vector2.One)
        {
            _player = Player;
            _journalBackground = 
                new Texture(InventoryInterfaceItemInfo.DefaultId, Position, InventoryInterfaceItemInfo.DefaultSize * .55f);
            _journalBackground.SendBack();
            _descriptionText = new GUIText(
                string.Empty,
                _descriptionPosition = -Vector2.UnitY * _journalBackground.Scale.Y * .8f,
                Color.White,
                FontCache.Get(AssetManager.NormalFamily, 11)
            );
            _renderBackground = new Texture(
                Graphics2D.LoadFromAssets("Assets/UI/QuestTextureBackground.png"),
                Vector2.Zero,
                Graphics2D.SizeFromAssets("Assets/UI/QuestTextureBackground.png").As1920x1080()
            );
            _renderTexture = new Texture(
                0,
                _journalBackground.Position,
                _journalBackground.Scale * .4f
            )
            {
                TextureElement =
                {
                    IdPointer = () => CurrentQuest?.View.GetTextureId() ?? GUIRenderer.TransparentTexture
                }
            };
            var abandonSize = Graphics2D.SizeFromAssets("Assets/UI/AbandonButton.png") * .4f;
            _abandonButton = new Button(
                _journalBackground.Position - Vector2.UnitY * (_journalBackground.Scale.Y - abandonSize.Y),
                abandonSize,
                Translation.Create("abandon_quest"),
                Color.Red,
                FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold)
            );
            _abandonButton.Click += (O, E) =>
            {
                Player.Questing.Abandon(CurrentQuest);
                UpdateView();
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            };
            _player.Questing.QuestAccepted += _ => UpdateMarkedQuest();
            _player.Questing.QuestAbandoned += _ => UpdateMarkedQuest();
            _player.Questing.QuestCompleted += _ => UpdateMarkedQuest();
            _player.Questing.QuestLoaded += _ => UpdateMarkedQuest();
            Panel.AddElement(_renderBackground);
            Panel.AddElement(_abandonButton);
            Panel.AddElement(_renderTexture);
            Panel.AddElement(_descriptionText);
            Panel.AddElement(_journalBackground);

            var leftJournalTopCorner = -Vector2.UnitX * _journalBackground.Scale.X 
                                       + Vector2.UnitY * (_journalBackground.Scale.Y + TitleText.Scale.Y * .5f);
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
            if (Quests.Length != 0)
            {
                UpdatePages(Quests.Length);
                Title.Disable();
                _descriptionText.Text = TextProvider.Wrap(CurrentQuest.Description, 38);
                _descriptionText.Position = Position + _descriptionPosition;
                _descriptionText.Position += Vector2.UnitY * _descriptionText.Scale.Y * 2;
                _renderTexture.Position = _journalBackground.Position;
                _renderTexture.Position += Vector2.UnitY * _descriptionText.Scale.Y * 2;
                _renderBackground.Position = _renderTexture.Position;
                _renderBackground.Scale = _renderTexture.Scale * 1.15f;
            }
            else
            {
                UpdatePages(1);
                _descriptionText.Text = Translations.Get("empty_journal");
                _renderTexture.Disable();
                _abandonButton.Disable();
                _renderBackground.Disable();
            }

            UpdateMarkedQuest();
        }

        private void UpdateMarkedQuest()
        {
            if (Quests.Length > 0)
            {
                if (CurrentQuest.HasLocation)
                    _player.Minimap.MarkQuest(CurrentQuest.Location);
                else
                    _player.Minimap.UnMarkQuest();
            }
            else
            {
                _player.Minimap.UnMarkQuest();
            }
        }

        private QuestObject CurrentQuest => Quests[Mathf.Modulo(CurrentPage, Quests.Length)];

        protected override Translation TitleTranslation => Translation.Create("quests");

        private QuestObject[] Quests => Player.Questing.ActiveQuests;
    }
}