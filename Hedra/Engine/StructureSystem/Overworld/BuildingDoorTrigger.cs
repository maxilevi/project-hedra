using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class BuildingDoorTrigger : BaseStructure
    {
        private readonly CollisionTrigger _outsideTrigger;
        private readonly CollisionTrigger _insideTrigger;
        private readonly Vector3 _offset;
        
        public BuildingDoorTrigger(Vector3 Position, VertexData Mesh) : base(Position)
        {
            _offset = DetectMeshDirection(Mesh).NormalizedFast() * 2;
            _outsideTrigger = new CollisionTrigger(Position + _offset, Mesh);
            _insideTrigger = new CollisionTrigger(Position - _offset, Mesh);

            _insideTrigger.OnCollision += OnInsideCollision;
            _outsideTrigger.OnCollision += OnOutsideCollision;
        }

        public void Leave(IEntity Entity)
        {
            OnOutsideCollision(Entity);
        }

        protected void OnOutsideCollision(IEntity Entity)
        {
            if (Entity.IsInsideABuilding)
            {
                Entity.IsInsideABuilding = false;
                OnLeave(Entity);
            }
        }
        
        protected void OnInsideCollision(IEntity Entity)
        {
            if (!Entity.IsInsideABuilding)
            {
                Entity.IsInsideABuilding = true;
                OnEnter(Entity);
            }
        }

        protected virtual void OnEnter(IEntity Entity)
        {
            
        }

        protected virtual void OnLeave(IEntity Entity)
        {
            
        }

        public bool IsInside(IEntity Entity)
        {
            if (Entity is IHumanoid humanoid)
                return humanoid.IsInsideABuilding;
            return false;
        }

        private static Vector3 DetectMeshDirection(VertexData Mesh)
        {
            if(Mesh.IsEmpty) throw new ArgumentOutOfRangeException();
            return Mesh.Normals.Aggregate((V1, V2) => V1 + V2) / Mesh.Normals.Count;
        }

        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                if(_insideTrigger != null)
                    _insideTrigger.Position = value + _offset;
                if(_insideTrigger != null)
                    _outsideTrigger.Position = value - _offset;
                base.Position = value;
                
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _outsideTrigger.Dispose();
            _insideTrigger.Dispose();
        }
    }
}