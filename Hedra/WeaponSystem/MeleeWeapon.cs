using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    public abstract class MeleeWeapon : Weapon
    {
        private readonly float _weaponHeight;
        private Vector3 _previousPosition;
        private CollisionShape[] _shapesArray;

        protected MeleeWeapon(VertexData MeshData) : base(MeshData)
        {
            if (MeshData != null)
            {
                _weaponHeight = MeshData.SupportPoint(Vector3.UnitY).Y - MeshData.SupportPoint(-Vector3.UnitY).Y;
                MainWeaponSize = new Vector3(
                    MeshData.SupportPoint(Vector3.UnitX).X - MeshData.SupportPoint(-Vector3.UnitX).X,
                    _weaponHeight,
                    MeshData.SupportPoint(Vector3.UnitZ).Z - MeshData.SupportPoint(-Vector3.UnitZ).Z
                );
            }
        }

        public int WeaponCount { get; private set; }
        public sealed override bool IsMelee => true;
        public Vector3 MainWeaponSize { get; protected set; }
        protected TrailRenderer Trail { get; set; }

        protected virtual Vector3 SheathedOffset => -Vector3.UnitY * _weaponHeight * .5f;

        public override Vector3 WeaponTip => MainMesh.TransformPoint(Vector3.UnitY * _weaponHeight);

        protected override void OnSheathed()
        {
            SetToChest(MainMesh);
            MainMesh.BeforeRotation =
                (SheathedPosition + Vector3.UnitX * 2.25f + Vector3.UnitZ * 1.5f + SheathedOffset) * Scale;
        }

        protected override void OnAttackStance()
        {
            SetToMainHand(MainMesh);
            MainMesh.BeforeRotation = Vector3.Transform(
                Vector3.UnitY * _weaponHeight * -.15f * Scale.Y + Vector3.UnitZ * .0f * Scale.Z,
                Matrix4x4.CreateRotationX(-70 * Mathf.Radian) * Matrix4x4.CreateRotationY(25 * Mathf.Radian) *
                Matrix4x4.CreateRotationZ(-90 * Mathf.Radian));
            MainMesh.LocalRotation = new Vector3(70, -25, 90);
        }

        protected override void OnPrimaryAttack()
        {
            SetToMainHand(MainMesh);
            MainMesh.BeforeRotation = Vector3.Transform(Vector3.UnitY * _weaponHeight * -.15f * Scale.Y,
                Matrix4x4.CreateRotationX(-90 * Mathf.Radian) * Matrix4x4.CreateRotationZ(-90 * Mathf.Radian));
            MainMesh.LocalRotation = new Vector3(90, 0, 90);
        }

        protected override void OnSecondaryAttack()
        {
            SetToMainHand(MainMesh);
            MainMesh.BeforeRotation = Vector3.Transform(Vector3.UnitY * _weaponHeight * -.15f * Scale.Y,
                Matrix4x4.CreateRotationX(-90 * Mathf.Radian) * Matrix4x4.CreateRotationZ(0 * Mathf.Radian));
            MainMesh.LocalRotation = new Vector3(90, 0, 0);

            if (_previousPosition != Owner.Position && Owner.IsGrounded)
            {
                var underChunk = World.GetChunkAt(Owner.Position);
                World.Particles.VariateUniformly = true;
                World.Particles.Color = World.GetHighestBlockAt((int)Owner.Position.X, (int)Owner.Position.Z)
                    .GetColor(underChunk.Biome.Colors); // * new Vector4(.8f, .8f, 1.0f, 1.0f);
                World.Particles.Position = Owner.Position - Vector3.UnitY;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = (-Owner.Orientation + Vector3.UnitY * 2.75f) * .15f;
                World.Particles.ParticleLifetime = 1;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);

                if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                    World.Particles.Color = new Vector4(underChunk.Biome.Colors.GrassColor.Xyz(), 1);

                World.Particles.Emit();
            }

            _previousPosition = Owner.Position;
        }

        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            if (Trail == null)
                Trail = new TrailRenderer(
                    () => WeaponTip,
                    Vector4.One);
            Trail.Emit &= Owner?.IsAttacking ?? false;
            Trail.Update();
        }

        public override void Dispose()
        {
            Trail?.Dispose();
            base.Dispose();
        }
    }
}