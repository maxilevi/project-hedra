using System;
using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering;
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

        private void FirstUpdate()
        {
            World.AddWorldObject(this);
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
    }
}