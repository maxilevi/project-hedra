/*
 * Author: Zaphyk
 * Date: 16/02/2016
 * Time: 07:54 p.m.
 *
 */
using System;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.Rendering.Particles
{
    public class Particle3D
    {
        public static Vector4 FireColor { get; } = new Vector4(1f, .1f, 0f, 1f);
        public static Vector4 AshColor { get; } = new Vector4(.2f,.2f,.2f,1f);
        public static Vector4 BloodColor { get; } = new Vector4(.8f, .0f, 0, 1f);
        public static Vector4 IceColor { get; } = new Vector4(0.2f, 0.514f, 0.714f, 1f) * new Vector4(1, 1, 2, 1) * .7f;
        public static Vector4 PoisonColor { get; } = new Vector4(0.282f, 0.725f, 0.373f, 1f) * new Vector4(1, 3, 1, 1);
        public static bool UseTimeScale {get; set;}
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public Vector3 Velocity;
        public Vector4 Color;
        public float GravityEffect;
        public float Lifetime;
        public bool IsVisible;
        public bool SpreadOverLifetime = false;
        private float MaxLifetime, OriginalAlpha;
        private Vector3 OriginalScale;
        private float HeightAtY = 0;
        private bool _collides;

        public Particle3D(Vector3 Position, Vector3 Velocity, Vector3 Rotation, Vector4 Color, Vector3 Scale, float GravityEffect, float Lifetime, bool Collides = false){
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.Color = Color;
            this.Velocity = Velocity;
            this.GravityEffect = GravityEffect;
            this.Lifetime = Lifetime;
            this.MaxLifetime = Lifetime;
            this.IsVisible = true;
            this.OriginalAlpha = Color.W;
            this.OriginalScale = Scale;
            this.Collides = Collides;
            
        }
        
        public bool Update(){
            this.Color = new Vector4(Color.Xyz, OriginalAlpha * 1);//(Lifetime / MaxLifetime) );
            this.Scale = OriginalScale * (Lifetime / MaxLifetime);
                
            if(Collides && Position.Y < HeightAtY + 2){
                Velocity = Velocity * new Vector3(1,0,1);    
            }
            
            Position += Velocity * (UseTimeScale ? (float) Time.DeltaTime : Time.IndependantDeltaTime);
            Velocity.Y += 60 * Physics.Gravity * GravityEffect * (UseTimeScale ? (float) Time.DeltaTime : Time.IndependantDeltaTime);
            Lifetime -= UseTimeScale ? (float) Time.DeltaTime : Time.IndependantDeltaTime;
            return !(Lifetime < 0);
        }
        
        public bool Collides{
            get => _collides;
            set{ _collides = value;
                if(value)
                    HeightAtY = Physics.HeightAtPosition( new Vector3(Position.X, Position.Y / Chunk.BlockSize + 1, Position.Z) );
            }
        }
        
        public static int SizeInBytes => 4*4*sizeof(float) + Vector4.SizeInBytes;
    }
}
