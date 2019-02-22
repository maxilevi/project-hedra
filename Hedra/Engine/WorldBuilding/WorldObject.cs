using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class WorldObject : UpdatableModel<ObjectMesh>, IWorldObject
    {
        public event OnDisposedEvent OnDispose;
        private bool _isIn;
        
        protected WorldObject(IEntity Parent) : base(Parent)
        {
        }

        public override void Update()
        {
            AssertIsWorldObject();
        }

        private void AssertIsWorldObject()
        {
            if (!_isIn)
            {
                if(Array.IndexOf(World.WorldObjects, this) == -1)
                    throw new ArgumentOutOfRangeException("Orphan world object");
                _isIn = true;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            OnDispose?.Invoke();
        }
    }
}