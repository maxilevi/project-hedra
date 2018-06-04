using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using System.Collections.Generic;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public abstract class MeleeWeapon : Weapon
    {
        public int WeaponCount { get; private set; }
        public override bool IsMelee { get; protected set; } = true;
        private Dictionary<ObjectMesh, ObjectMeshCollider> _colliders { get; set; }
        private CollisionShape[] _shapesArray;

        protected MeleeWeapon(VertexData MeshData) : base(MeshData)
        {
            _colliders = new Dictionary<ObjectMesh, ObjectMeshCollider>();
        }

        public override void Update(Humanoid Human)
        {
            if (!this.WeaponRegistered(this.MainMesh))
            {
                this.RegisterWeapon(this.MainMesh, this.MeshData);
            }
            base.Update(Human);
        }

        private bool WeaponRegistered(ObjectMesh Mesh) 
        {
            return _colliders.ContainsKey(Mesh);
        }

        protected void RegisterWeapon(ObjectMesh Mesh, VertexData MeshData)
        {
            _colliders.Add(Mesh, new ObjectMeshCollider(Mesh, MeshData));
            WeaponCount++;
        }

        public CollisionShape[] Shapes {
            get{
                if(_shapesArray?.Length != WeaponCount){
                    _shapesArray = new CollisionShape[WeaponCount]; 
                }
                var i = 0;
                foreach(var collider in _colliders.Values)
                {
                    _shapesArray[i] = collider.Shape;
                    i++;
                }
                return _shapesArray;
            }
        }

        public override void Dispose()
        {
            _colliders.Clear();
            base.Dispose();
        }
    }
}
