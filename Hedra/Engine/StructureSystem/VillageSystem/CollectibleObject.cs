using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class CollectibleObject : InteractableStructure
    {
        private readonly Item _drop;
        private readonly InstanceData _model;
        private readonly InstanceDataChunkWatcher _watcher;

        public CollectibleObject(Vector3 Position, InstanceData Model, Item Drop) : base(Position)
        {
            _drop = Drop;
            _model = Model;
            _watcher = new InstanceDataChunkWatcher(() => new[]
            {
                _model
            });
        }

        protected override bool AllowThroughCollider => true;

        public override string Message => Translations.Get("to_collect", _drop.DisplayName);

        public override int InteractDistance => 12;

        protected override void Interact(IHumanoid Humanoid)
        {
            if (!Humanoid.Inventory.AddItem(_drop)) World.DropItem(_drop, Position);
            SoundPlayer.PlaySound(SoundType.NotificationSound, Humanoid.Position);
        }

        public override void Dispose()
        {
            base.Dispose();
            _watcher.Dispose();
        }
    }
}