using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class WorldObject : UpdatableModel<ObjectMesh>, IWorldObject
    {
        private bool _added;

        protected WorldObject(IEntity Parent) : base(Parent)
        {
            UpdateManager.Add(this);
        }

        public override void Update()
        {
            if (!_added)
            {
                FirstUpdate();
                _added = true;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            World.RemoveWorldObject(this);
            UpdateManager.Remove(this);
        }

        private void FirstUpdate()
        {
            World.AddWorldObject(this);
        }
    }
}