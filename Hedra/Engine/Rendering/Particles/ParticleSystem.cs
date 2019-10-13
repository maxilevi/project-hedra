/*
 * Author: Zaphyk
 * Date: 12/02/2016
 * Time: 05:45 a.m.
 *
 */
using System;
using Hedra.Engine.Player;
using System.Collections.Generic;
using System.Reflection;
using Hedra.Core;
using Hedra.Engine.Game;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Rendering.Particles;

namespace Hedra.Engine.Rendering.Particles
{
    public class ParticleSystem : IRenderable, IDisposable, ITickable
    {
        public const int MaxParticleCount = 15000;
        private static readonly Shader Shader = Shader.Build("Shaders/Particle.vert","Shaders/Particle.frag");
        private readonly object _lock = new object();

        private readonly List<Particle3D> _particles;
        private VBO<Vector4> _particleVbo;
        private ParticleVAO _vao;
        public int UpdatesPerSecond => 60;
        public bool Disposed { get; private set; }
        public int MaxParticles { get; set; } = MaxParticleCount;
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public Vector4 Color { get; set; }
        public float ParticleLifetime { get; set; }
        public float GravityEffect { get; set; }
        public Vector3 PositionErrorMargin { get; set; }
        public Vector3 ScaleErrorMargin { get; set; }
        public Vector3 Scale { get; set; }
        public bool RandomRotation { get; set; } = true;
        public ParticleShape Shape { get; set; } = ParticleShape.Square;
        public float ConeAngle { get; set; }
        public float ConeSpeed { get; set; } = 1;
        public bool Grayscale { get; set; }
        public bool VariateUniformly { get; set; }
        public bool HasMultipleOutputs { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Collides { get; set; }
        public bool UseTimeScale { get; set; }

        public int ParticleCount
        {
            get
            {
                lock (_lock)
                    return _particles.Count;
            }
        }
        private int _particlesInMemory;
        
        
        public ParticleSystem(): this(Vector3.Zero)
        {
        }
        
        public ParticleSystem(Vector3 Position)
        {
            this.Position = Position;
            _particles = new List<Particle3D>();
            ParticleCreator.Load();
            Executer.ExecuteOnMainThread(delegate
            {
                this.BuildBuffers();

                DrawManager.ParticleRenderer.Add(this);
                BackgroundUpdater.Add(this);
            });
        }
        
        public void Emit()
        {
            if((this.Position - LocalPlayer.Instance.Position).LengthSquared > GeneralSettings.DrawDistanceSquared) return;
            
            if(ParticleCount == MaxParticles || !Enabled || (Time.Paused && UseTimeScale)) return;
            
            var localPositionX = PositionErrorMargin.X * Utils.Rng.NextFloat() * 2f - PositionErrorMargin.X;
            var localPositionY = PositionErrorMargin.Y * Utils.Rng.NextFloat() * 2f - PositionErrorMargin.Y;
            var localPositionZ = PositionErrorMargin.Z * Utils.Rng.NextFloat() * 2f - PositionErrorMargin.Z;
            var particlePosition = new Vector3(localPositionX, localPositionY, localPositionZ);
            
            var scaleMargin = Utils.Rng.NextFloat();
            var localScaleX = ScaleErrorMargin.X * scaleMargin * 2f - ScaleErrorMargin.X;
            var localScaleY = ScaleErrorMargin.Y * scaleMargin * 2f - ScaleErrorMargin.Y;
            var localScaleZ = ScaleErrorMargin.Z * scaleMargin * 2f - ScaleErrorMargin.Z;
            
            var particleScale = new Vector3(localScaleX, localScaleY, localScaleZ);
            Vector4 newColor;
            if(Grayscale)
            {
                var shade = Color.Xyz.Average() + Utils.Rng.NextFloat() * .2f -.1f;
                newColor = new Vector4(shade, shade, shade, Color.W);
            }
            else
            {
                if(VariateUniformly)
                {
                    var shade = Utils.Rng.NextFloat() * .2f -.1f;
                    newColor = new Vector4(Color.X + shade, Color.Y + shade, Color.Z + shade, Color.W);
                }
                else
                {
                    newColor = Utils.VariateColor(Color, 50);
                }
            }
            
            if(Shape == ParticleShape.Cone)
            {
                lock (_lock)
                {
                    _particles.Add(new Particle3D(Position,
                        ParticleCreator.UnitWithinCone(Direction, ConeAngle) * 25 * ConeSpeed,
                        Mathf.RandomVector3(Utils.Rng) * 360,
                        newColor,
                        Scale + particleScale, GravityEffect, ParticleLifetime, Collides));
                }
            }
            else if(Shape == ParticleShape.Sphere)
            {

                if (particlePosition.X * particlePosition.X + particlePosition.Y * particlePosition.Y +
                    particlePosition.Z * particlePosition.Z <=
                    (PositionErrorMargin.X * PositionErrorMargin.X + PositionErrorMargin.Y * PositionErrorMargin.Y +
                     PositionErrorMargin.Z * PositionErrorMargin.Z) / 4.0)
                {
                    lock (_lock)
                    {
                        _particles.Add(new Particle3D(this.Position + particlePosition, Direction * 25,
                            Mathf.RandomVector3(Utils.Rng) * 360,
                            newColor,
                            Scale + particleScale, GravityEffect, ParticleLifetime, Collides));
                    }
                }
            }
            else
            {
                lock (_lock)
                {
                    _particles.Add(new Particle3D(this.Position + particlePosition, Direction * 25,
                        Mathf.RandomVector3(Utils.Rng) * 360,
                        newColor,
                        Scale + particleScale, GravityEffect, ParticleLifetime, Collides));
                }
            }
        }
        
        
        public unsafe void Update(float DeltaTime)
        {
            if(!HasMultipleOutputs && (this.Position - LocalPlayer.Instance.Position).LengthSquared > GeneralSettings.DrawDistanceSquared) return;

            lock (_lock)
            {
                for (var i = 0; i < _particles.Count; i++)
                {
                    var randomRotation = RandomRotation
                        ? Mathf.RandomVector3(Utils.Rng) * 150 * Time.DeltaTime
                        : Vector3.Zero;
                    if (!_particles[i].Update(DeltaTime, randomRotation))
                    {
                        _particles.RemoveAt(i);
                    }
                }
                BuildBufferAndSendToGPU();
            }
        }

        private unsafe void BuildBufferAndSendToGPU()
        {
            if (_particles.Count <= 0) return;
            var count = _particles.Count;
            var arrayLength = count * 5;
            var vec4S = new Vector4[arrayLength];
            for (var i = count - 1; i > -1; i--)
            {
                var transMatrix = ConstructTransformationMatrix(_particles[i].Position, _particles[i].Rotation,
                    _particles[i].Scale);
                vec4S[i * 5 + 0] = _particles[i].Color;
                vec4S[i * 5 + 1] = transMatrix.Column0;
                vec4S[i * 5 + 2] = transMatrix.Column1;
                vec4S[i * 5 + 3] = transMatrix.Column2;
                vec4S[i * 5 + 4] = transMatrix.Column3;
            }
            Executer.ExecuteOnMainThread(() =>
            {
                UpdateVbo(vec4S, count);
            });
        }
        
        public void Draw()
        {
            if(!HasMultipleOutputs && (this.Position - LocalPlayer.Instance.Position).LengthSquared > GeneralSettings.DrawDistanceSquared) return;
            if (ParticleCount > 0)
            {
                Renderer.Enable(EnableCap.Blend);
                Renderer.Enable(EnableCap.DepthTest);
                Shader.Bind();
                Shader["PlayerPosition"] = GameManager.Player.Position;

                _vao.Bind();

                ParticleCreator.IndicesVBO.Bind();
                Renderer.DrawElementsInstanced(PrimitiveType.Triangles, ParticleCreator.IndicesVBO.Count,
                    DrawElementsType.UnsignedShort, IntPtr.Zero, _particlesInMemory);
                ParticleCreator.IndicesVBO.Unbind();

                _vao.Unbind();

                Shader.Unbind();
                Renderer.Disable(EnableCap.Blend);
            }
        }
        
        private void BuildBuffers()
        {
            _particleVbo = new VBO<Vector4>(new Vector4[0], 0, VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            _vao = new ParticleVAO(ParticleCreator.VerticesVBO, ParticleCreator.NormalsVBO, _particleVbo);
        }
        
        private void UpdateVbo(Vector4[] Vec4S, int Count)
        {
            if(Disposed) return;
            _particleVbo.Update(Vec4S, Vec4S.Length * Vector4.SizeInBytes);
            _particlesInMemory = Count;
        }
        
        private static Matrix4 ConstructTransformationMatrix(Vector3 Position, Vector3 Rotation, Vector3 Scale)
        {
            var axis = Rotation / Rotation.Y;
            var rotationMatrix = Matrix4.CreateFromAxisAngle(axis, Rotation.Y * Mathf.Radian);
            
            var transMatrix = Matrix4.CreateScale(Scale);
            transMatrix = Matrix4.Mult(transMatrix, rotationMatrix);
            transMatrix = Matrix4.Mult(transMatrix,  Matrix4.CreateTranslation(Position));
            return transMatrix;
        }
        
        public void Dispose()
        {
            if(Disposed) return;
            Disposed = true;
            void DoDispose()
            {
                _vao.Dispose();
                _particleVbo.Dispose();
                DrawManager.ParticleRenderer.Remove(this);
                BackgroundUpdater.Remove(this);
            };
            if(_vao == null) Executer.ExecuteOnMainThread(DoDispose);
            else DoDispose();
        }
    }
}
