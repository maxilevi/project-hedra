using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Localization;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Mission.Blocks
{
    public class FishItemMission : ItemMission
    {
        private const float Chance = 0.5f;
        private FishingZone _zone;
        private Item _item;
        public override void Setup()
        {
            _zone = new FishingZone(Zone, Colors.OrangeRed, Radius, Chance, Item);
            World.FishingZoneHandler.AddZone(_zone);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            World.FishingZoneHandler.RemoveZone(_zone);
            ConsumeItems();
        }

        public Vector3 Zone { get; set; }
        public float Radius { get; set; }

        public Item Item
        {
            get => _item;
            set
            {
                Items = new[]
                {
                    new ItemCollect
                    {
                        Amount = 1,
                        Name = value.Name
                    }
                };
                _item = value;
            }
        }
        public override string ShortDescription => Translations.Get("quest_fish_item_short", Giver.Name, _item.DisplayName);
        public override string Description => Translations.Get("quest_fish_item_description", Giver.Name, _item.DisplayName);
        public override DialogObject DefaultOpeningDialog => new DialogObject
        {
            Keyword = "quest_fish_item_dialog",
            Arguments = new object[]
            {
                _item.DisplayName
            }
        };
    }
}