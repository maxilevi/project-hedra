using System.Numerics;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.PagedInterface;
using Hedra.Engine.Rendering.UI;
using Hedra.Localization;
using Hedra.Mission;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.Sound;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.QuestSystem
{
    public sealed class QuestingJournal : PagedInventoryArrayInterface
    {
        private readonly Button _abandonButton;
        private readonly Vector2 _descriptionPosition;
        private readonly GUIText _descriptionText;
        private readonly BackgroundTexture _journalBackground;
        private readonly IPlayer _player;
        private readonly BackgroundTexture _renderBackground;
        private readonly BackgroundTexture _renderTexture;
        private readonly GUIText _storylineLabel;

        public QuestingJournal(IPlayer Player)
            : base(Player, null, 0, 0, Vector2.One)
        {
            _player = Player;
            _journalBackground =
                new BackgroundTexture(InventoryInterfaceItemInfo.DefaultId, Position,
                    InventoryInterfaceItemInfo.DefaultSize * .55f);
            _journalBackground.SendBack();
            _descriptionText = new GUIText(
                string.Empty,
                _descriptionPosition = -Vector2.UnitY * _journalBackground.Scale.Y * .8f,
                Color.White,
                FontCache.GetNormal(11)
            );
            _renderBackground = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/QuestTextureBackground.png"),
                Vector2.Zero,
                Graphics2D.SizeFromAssets("Assets/UI/QuestTextureBackground.png").As1920x1080()
            );
            _renderTexture = new BackgroundTexture(
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
            _storylineLabel = new GUIText(
                Translation.Create("storyline_quest"),
                Vector2.Zero,
                Color.Gold,
                FontCache.GetBold(10)
            );
            _storylineLabel.Position = _journalBackground.Position -
                                       Vector2.UnitY * (_journalBackground.Scale.Y - _storylineLabel.Scale.Y);
            var abandonSize = Graphics2D.SizeFromAssets("Assets/UI/AbandonButton.png") * .4f;
            _abandonButton = new Button(
                _journalBackground.Position - Vector2.UnitY * (_journalBackground.Scale.Y - abandonSize.Y),
                abandonSize,
                Translation.Create("abandon_quest"),
                Color.Red,
                FontCache.GetBold(10)
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
            Panel.AddElement(_storylineLabel);

            var leftJournalTopCorner = -Vector2.UnitX * _journalBackground.Scale.X
                                       + Vector2.UnitY * (_journalBackground.Scale.Y + TitleText.Scale.Y * .5f);
            TitleText.Position += Vector2.UnitX * TitleText.Scale.X * 1.25f;
            TitleText.Position += leftJournalTopCorner;
            var rightJournalCorner = leftJournalTopCorner * new Vector2(-1, 1)
                                     - Vector2.UnitX * 1.25f * (PageSelector.Scale.X + NextPageText.Scale.X +
                                                                PreviousPageText.Scale.X + CurrentPageText.Scale.X);
            PageSelector.Position += rightJournalCorner;
            NextPageText.Position += rightJournalCorner;
            PreviousPageText.Position += rightJournalCorner;
            CurrentPageText.Position += rightJournalCorner;
        }

        private MissionObject CurrentQuest =>
            Quests.Length != 0 ? Quests[Mathf.Modulo(CurrentPage, Quests.Length)] : null;

        protected override Translation TitleTranslation => Translation.Create("quests");

        private MissionObject[] Quests => Player.Questing.ActiveQuests;

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
                if (Panel.Enabled)
                {
                    _storylineLabel.Disable();
                    if (CurrentQuest.IsStoryline)
                    {
                        _abandonButton.Disable();
                        _storylineLabel.Enable();
                    }
                }
            }
            else
            {
                UpdatePages(1);
                _descriptionText.Text = Translations.Get("empty_journal");
                _renderTexture.Disable();
                _abandonButton.Disable();
                _renderBackground.Disable();
                _storylineLabel.Disable();
            }

            UpdateMarkedQuest();
        }

        private void UpdateMarkedQuest()
        {
            if (Quests.Length > 0)
            {
                var quest = CurrentQuest;
                if (quest.HasLocation)
                    _player.Minimap.MarkQuest(
                        () =>
                        {
                            if (!quest.Disposed) return quest.Location;
                            _player.Minimap.UnMarkQuest();
                            return Vector3.Zero;
                        });
                else
                    _player.Minimap.UnMarkQuest();
            }
            else
            {
                _player.Minimap.UnMarkQuest();
            }
        }
    }
}