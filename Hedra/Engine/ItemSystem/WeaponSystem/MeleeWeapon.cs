using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public abstract class MeleeWeapon : Weapon
    {
        public int WeaponCount { get; private set; }
        public override bool IsMelee { get; protected set; } = true;
        public Vector3 MainWeaponSize { get; private set; }
        protected TrailRenderer Trail { get; set; }
        private Dictionary<ObjectMesh, ObjectMeshCollider> _colliders { get; set; }
        private CollisionShape[] _shapesArray;
        private readonly float _weaponHeight;
        private Vector3 _previousPosition;

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
            if (InAttackStance)
            {
                base.SetToMainHand(MainMesh);
                MainMesh.BeforeLocalRotation = Vector3.TransformPosition(Vector3.UnitY * _weaponHeight * -.15F * this.Scale.Y + Vector3.UnitZ * .0f * this.Scale.Z,
                    Matrix4.CreateRotationX(-70 * Mathf.Radian) * Matrix4.CreateRotationY(25 * Mathf.Radian)  * Matrix4.CreateRotationZ(-90 * Mathf.Radian));
                MainMesh.TargetRotation = new Vector3(70, -25, 90);
            }
            if (PrimaryAttack)
            {
                base.SetToMainHand(MainMesh);
                MainMesh.BeforeLocalRotation = Vector3.TransformPosition(Vector3.UnitY * _weaponHeight * -.15f * this.Scale.Y,
                    Matrix4.CreateRotationX(-90 * Mathf.Radian) * Matrix4.CreateRotationZ(-90 * Mathf.Radian));
                MainMesh.TargetRotation = new Vector3(90, 0, 90);
            }

            if (SecondaryAttack)
            {
                base.SetToMainHand(MainMesh);
                MainMesh.BeforeLocalRotation = Vector3.TransformPosition(Vector3.UnitY * _weaponHeight * -.15f * this.Scale.Y,
                    Matrix4.CreateRotationX(-90 * Mathf.Radian) * Matrix4.CreateRotationZ(0 * Mathf.Radian));
                MainMesh.TargetRotation = new Vector3(90, 0, 0);

                if (_previousPosition != Human.BlockPosition && Human.IsGrounded)
                {
                    Chunk underChunk = World.GetChunkAt(Human.Position);
                    World.Particles.VariateUniformly = true;
                    World.Particles.Color = World.GetHighestBlockAt((int)Human.Position.X, (int)Human.Position.Z).GetColor(underChunk.Biome.Colors);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
                    World.Particles.Position = Human.Position - Vector3.UnitY;
                    World.Particles.Scale = Vector3.One * .5f;
                    World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                    World.Particles.Direction = (-Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
                    World.Particles.ParticleLifetime = 1;
                    World.Particles.GravityEffect = .1f;
                    World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);

                    if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                        World.Particles.Color = new Vector4(underChunk.Biome.Colors.GrassColor.Xyz, 1);

                    World.Particles.Emit();
                }
                _previousPosition = Human.BlockPosition;
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
