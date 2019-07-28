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
        protected WorldObject(IEntity Parent) : base(Parent)
        {
            World.AddWorldObject(this);
            UpdateManager.Add(this);
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            World.RemoveWorldObject(this);
            UpdateManager.Remove(this);
        }
    }
}