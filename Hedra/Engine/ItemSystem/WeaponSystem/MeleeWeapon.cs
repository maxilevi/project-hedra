using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    internal abstract class MeleeWeapon : Weapon
    {
        public int WeaponCount { get; private set; }
        public override bool IsMelee { get; protected set; } = true;
        public Vector3 MainWeaponSize { get; private set; }
        protected TrailRenderer Trail { get; set; }
        private Dictionary<ObjectMesh, ObjectMeshCollider> _colliders { get; set; }
        private CollisionShape[] _shapesArray;
        private readonly float _weaponHeight;

        protected MeleeWeapon(VertexData MeshData) : base(MeshData)
        {
            _colliders = new Dictionary<ObjectMesh, ObjectMeshCollider>();
            if (MeshData != null)
            {
                _weaponHeight = MeshData.SupportPoint(Vector3.UnitY).Y - MeshData.SupportPoint(-Vector3.UnitY).Y;
            }
        }

        public override void Update(Humanoid Human)
        {
            if (!this.WeaponRegistered(this.MainMesh))
            {
                this.RegisterWeapon(this.MainMesh, this.MeshData);
                var lastCollider = _colliders.Last().Value;
                this.MainWeaponSize = lastCollider.Collider.Size;
            }
            if (Trail == null)
            {
                this.Trail = new TrailRenderer(
                    () => this.WeaponTip,
                    Vector4.One);
            }
            this.Trail.Emit &= this.Owner?.IsAttacking ?? false;
            this.Trail.Update();
            base.Update(Human);

            if (Sheathed)
            {
                this.SetToChest(MainMesh);
                MainMesh.BeforeLocalRotation =
                    (this.SheathedPosition + Vector3.UnitX * 2.25f + Vector3.UnitZ * 2.5f - Vector3.UnitY * (_weaponHeight * .5f - 1.25f)) * this.Scale;
            }

            if (PrimaryAttack)
            {
                base.SetToMainHand(MainMesh);
                MainMesh.BeforeLocalRotation = Vector3.TransformPosition(Vector3.UnitY * _weaponHeight * -.15f * this.Scale.Y,
                    Matrix4.CreateRotationX(-90 * Mathf.Radian) * Matrix4.CreateRotationZ(0 * Mathf.Radian));
                MainMesh.TargetRotation = new Vector3(90, 0, 90);
            }
        }

        public override Vector3 WeaponTip => Vector3.Zero;//MainMesh.TransformPoint(Vector3.UnitY * _swordHeight);

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
            Trail?.Dispose();
            base.Dispose();
        }
    }
}
