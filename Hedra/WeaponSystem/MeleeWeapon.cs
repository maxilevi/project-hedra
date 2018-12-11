using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public abstract class MeleeWeapon : Weapon
    {
        public int WeaponCount { get; private set; }
        public override bool IsMelee => true;
        public Vector3 MainWeaponSize { get; protected set; }
        protected TrailRenderer Trail { get; set; }
        private CollisionShape[] _shapesArray;
        private readonly float _weaponHeight;
        private Vector3 _previousPosition;

        protected MeleeWeapon(VertexData MeshData) : base(MeshData)
        {
            if (MeshData != null)
            {
                _weaponHeight = MeshData.SupportPoint(Vector3.UnitY).Y - MeshData.SupportPoint(-Vector3.UnitY).Y;
                MainWeaponSize = new Vector3(
                    MeshData.SupportPoint(Vector3.UnitX).X - MeshData.SupportPoint(-Vector3.UnitX).X,
                    MeshData.SupportPoint(Vector3.UnitY).Y - MeshData.SupportPoint(-Vector3.UnitY).Y,
                    MeshData.SupportPoint(Vector3.UnitZ).Z - MeshData.SupportPoint(-Vector3.UnitZ).Z
               );
            }
        }

        protected override void OnSheathed()
        {
            this.SetToChest(MainMesh);
            MainMesh.BeforeLocalRotation =
                (this.SheathedPosition + Vector3.UnitX * 2.25f + Vector3.UnitZ * 1.5f - Vector3.UnitY * 1.5f) * this.Scale;
        }

        protected override void OnAttackStance()
        {
            base.SetToMainHand(MainMesh);
            MainMesh.BeforeLocalRotation = Vector3.TransformPosition(Vector3.UnitY * _weaponHeight * -.15F * this.Scale.Y + Vector3.UnitZ * .0f * this.Scale.Z,
                Matrix4.CreateRotationX(-70 * Mathf.Radian) * Matrix4.CreateRotationY(25 * Mathf.Radian)  * Matrix4.CreateRotationZ(-90 * Mathf.Radian));
            MainMesh.TargetRotation = new Vector3(70, -25, 90);
        }

        protected override void OnPrimaryAttack()
        {
            base.SetToMainHand(MainMesh);
            MainMesh.BeforeLocalRotation = Vector3.TransformPosition(Vector3.UnitY * _weaponHeight * -.15f * this.Scale.Y,
                Matrix4.CreateRotationX(-90 * Mathf.Radian) * Matrix4.CreateRotationZ(-90 * Mathf.Radian));
            MainMesh.TargetRotation = new Vector3(90, 0, 90);
        }

        protected override void OnSecondaryAttack()
        {
            base.SetToMainHand(MainMesh);
            MainMesh.BeforeLocalRotation = Vector3.TransformPosition(Vector3.UnitY * _weaponHeight * -.15f * this.Scale.Y,
                Matrix4.CreateRotationX(-90 * Mathf.Radian) * Matrix4.CreateRotationZ(0 * Mathf.Radian));
            MainMesh.TargetRotation = new Vector3(90, 0, 0);

            if (_previousPosition != Owner.BlockPosition && Owner.IsGrounded)
            {
                Chunk underChunk = World.GetChunkAt(Owner.Position);
                World.Particles.VariateUniformly = true;
                World.Particles.Color = World.GetHighestBlockAt((int)Owner.Position.X, (int)Owner.Position.Z).GetColor(underChunk.Biome.Colors);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
                World.Particles.Position = Owner.Position - Vector3.UnitY;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = (-Owner.Orientation + Vector3.UnitY * 2.75f) * .15f;
                World.Particles.ParticleLifetime = 1;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);

                if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                    World.Particles.Color = new Vector4(underChunk.Biome.Colors.GrassColor.Xyz, 1);

                World.Particles.Emit();
            }
            _previousPosition = Owner.BlockPosition;
        }

        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            if (Trail == null)
            {
                this.Trail = new TrailRenderer(
                    () => this.WeaponTip,
                    Vector4.One);
            }
            this.Trail.Emit &= this.Owner?.IsAttacking ?? false;
            this.Trail.Update();
        }

        public override Vector3 WeaponTip => Vector3.Zero;//MainMesh.TransformPoint(Vector3.UnitY * _swordHeight);

        public override void Dispose()
        {
            Trail?.Dispose();
            base.Dispose();
        }
    }
}
