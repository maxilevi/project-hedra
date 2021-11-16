using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Mission.Blocks
{
    public class FishItemMission : ItemMission
    {
        private const float Chance = 0.5f;
        private Item _item;
        private FishingZone _zone;

        public Vector3 Zone { get; set; }
        public float Radius { get; set; }

        public override Vector3 Location => Zone;
        public override bool HasLocation => true;

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

        public override string ShortDescription =>
            Translations.Get("quest_fish_item_short", Giver.Name, _item.DisplayName);

        public override string Description =>
            Translations.Get("quest_fish_item_description", Giver.Name, _item.DisplayName);

        public override DialogObject DefaultOpeningDialog => new DialogObject
        {
            Keyword = "quest_fish_item_dialog",
            Arguments = new object[]
            {
                _item.DisplayName
            }
        };

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

        public override void Dispose()
        {
            base.Dispose();
            if (_zone != null) World.FishingZoneHandler.RemoveZone(_zone);
        }
    }
}