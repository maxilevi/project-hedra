/*
 * Author: Zaphyk
 * Date: 16/02/2016
 * Time: 07:54 p.m.
 *
 */

using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Particles
{
    public class Particle3D
    {
        private bool _collides;
        public Vector4 Color;
        public float GravityEffect;
        private float HeightAtY;
        public float Lifetime;
        private readonly float MaxLifetime;
        private readonly float OriginalAlpha;
        private readonly Vector3 OriginalScale;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public Vector3 Velocity;

        public Particle3D(Vector3 Position, Vector3 Velocity, Vector3 Rotation, Vector4 Color, Vector3 Scale,
            float GravityEffect, float Lifetime, bool Collides = false)
        {
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.Color = Color;
            this.Velocity = Velocity;
            this.GravityEffect = GravityEffect;
            this.Lifetime = Lifetime;
            MaxLifetime = Lifetime;
            OriginalAlpha = Color.W;
            OriginalScale = Scale;
            HeightAtY = 0;
            _collides = false;
            this.Collides = Collides;
        }

        public static Vector4 FireColor { get; } = new Vector4(1f, .1f, 0f, 1f);
        public static Vector4 AshColor { get; } = new Vector4(.2f, .2f, .2f, 1f);
        public static Vector4 BloodColor { get; } = new Vector4(.8f, .0f, 0, 1f);
        public static Vector4 IceColor { get; } = new Vector4(0.2f, 0.514f, 0.714f, 1f) * new Vector4(1, 1, 2, 1) * .7f;
        public static Vector4 PoisonColor { get; } = new Vector4(0.282f, 0.725f, 0.373f, 1f) * new Vector4(1, 3, 1, 1);

        public bool Collides
        {
            get => _collides;
            set
            {
                _collides = value;
                if (value)
                    HeightAtY = Physics.HeightAtPosition(new Vector3(Position.X, Position.Y / Chunk.BlockSize + 1,
                        Position.Z));
            }
        }

        public static int SizeInBytes => 4 * 4 * sizeof(float) + HedraSize.Vector4;

        public bool Update(float DeltaTime, Vector3 randomRotation)
        {
            Color = new Vector4(Color.Xyz(), OriginalAlpha * 1);
            Scale = OriginalScale * (Lifetime / MaxLifetime);

            Position += Velocity * DeltaTime;
            Velocity.Y += 60 * Physics.Gravity * GravityEffect * DeltaTime;
            Lifetime -= DeltaTime;
            Rotation += randomRotation;

            if (Collides && Position.Y < HeightAtY + .5f)
                Position = new Vector3(Position.X, HeightAtY + .5f, Position.Z);

            return !(Lifetime < 0);
        }
    }
}