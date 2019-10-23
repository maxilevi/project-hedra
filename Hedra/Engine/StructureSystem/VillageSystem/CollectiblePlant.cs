using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Sound;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class CollectiblePlant : InteractableStructure
    {
        private readonly Item _drop;
        private readonly InstanceData _model;
        private readonly InstanceDataChunkWatcher _watcher;
        private Village _parent;
        
        public CollectiblePlant(Vector3 Position, InstanceData Model, Item Drop) : base(Position)
        {
            _drop = Drop;
            _model = Model;
            _watcher = new InstanceDataChunkWatcher(() => new []
            {
                _model
            });
        }

        public override string Message => Translations.Get("to_collect");

        public override int InteractDistance => 12;
        
        protected override void Interact(IHumanoid Humanoid)
        {
            if (!Humanoid.Inventory.AddItem(_drop))
            {
                World.DropItem(_drop, Position);
            }
            SoundPlayer.PlaySound(SoundType.NotificationSound, Humanoid.Position);
        }

        public override void Dispose()
        {
            base.Dispose();
            _watcher.Dispose();
        }
    }
}