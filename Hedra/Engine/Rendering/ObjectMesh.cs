/*
 * Author: Zaphyk
 * Date: 03/03/2016
 * Time: 09:08 p.m.
 *
 */
using System;
using Hedra.Core;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering
{
    public class ObjectMesh : IRenderable, IDisposable, ICullableModel, IUpdatable
    {
        public float AnimationSpeed { get; set; } = 1f;
        public bool PrematureCulling { get; set; } = true;
        public Box CullingBox { get; set; }
        public Vector3 Max => CullingBox?.Max ?? Vector3.Zero;
        public Vector3 Min => CullingBox?.Min ?? Vector3.Zero;
        public ChunkMesh Mesh { get; }
        private readonly ObjectMeshBuffer _buffer;
        private bool _enabled;
        private Chunk _underChunk;
        private Timer _underChunkTimer;

        public ObjectMesh()
        {
           _buffer = new ObjectMeshBuffer();
        }

        private ObjectMesh(Vector3 Position)
        {
            this.Enabled = true;
            this._buffer = new ObjectMeshBuffer();
            this.Mesh = new ChunkMesh(Position, _buffer);
            this.Position = Position;
            this.LocalRotation = Vector3.Zero;
            Mesh.Enabled = true;
            Enabled = true;
            _underChunkTimer = new Timer(4 + Utils.Rng.NextFloat() * 2);
            DrawManager.Add(this);
            UpdateManager.Add(this);
        }

        public void Draw()
        {
            if (Enabled)
               Mesh.Draw();
        }

        public void Update()
        {
            if (_underChunkTimer.Tick())
                _underChunk = World.GetChunkAt(Position);
            _buffer.LocalRotation = LocalRotation;//Mathf.Lerp(_buffer.LocalRotation, LocalRotation, Time.IndependantDeltaTime * 8f);
        }

        public bool ApplyNoiseTexture
        {
            get => _buffer.UseNoiseTexture;
            set => _buffer.UseNoiseTexture = value;
        }

        public Vector3 TransformPoint(Vector3 Point)
        {
            return _buffer.TransformPoint(Point);
        }

        public Matrix4 TransformationMatrix
        {
            get => _buffer.TransformationMatrix;
            set => _buffer.TransformationMatrix = value;
        }
        
        public Vector4 Tint
        {
            get => _buffer.Tint;
            set => _buffer.Tint = value;
        }
        
        public Vector4 BaseTint
        {
            get => _buffer.BaseTint;
            set => _buffer.BaseTint = value;
        }

        public Vector4 OutlineColor
        {
            get => _buffer.OutlineColor;
            set => _buffer.OutlineColor = value;
        }

        public bool Outline
        {
            get => _buffer.Outline;
            set => _buffer.Outline = value;
        }

        public bool Dither
        {
            get => _buffer.Dither;
            set => _buffer.Dither = value;
        }

        public Vector3 Position
        {
            get => _buffer.Position;
            set => _buffer.Position = value;
        }
        
        public Vector3 LocalPosition
        {
            get => _buffer.LocalPosition;
            set => _buffer.LocalPosition = value;
        }
        
        public Vector3 RotationPoint
        {
            get => _buffer.RotationPoint;
            set => _buffer.RotationPoint = value;
        }

        public bool Pause
        {
            get => _buffer.Pause;
            set => _buffer.Pause = value;
        }

        public bool Enabled
        {
            get => _enabled && !(_underChunk?.Mesh?.Occluded ?? false);
            set => _enabled = value;
        }
        
        public Vector3 LocalRotation { get; set; }

        public Vector3 BeforeRotation
        {
            get => _buffer.BeforeRotation;
            set => _buffer.BeforeRotation = value;
        }
        
        public Vector3 Rotation
        {
            get => _buffer.Rotation;
            set
            {
                if(_buffer.Rotation == value) return;
                _buffer.Rotation = value;
            }
        }
        
        public Vector3 LocalRotationPoint
        {
            get => _buffer.LocalRotationPoint;
            set => _buffer.LocalRotationPoint = value;
        }
        
        public bool ApplyFog
        {
            get => _buffer.ApplyFog;
            set => _buffer.ApplyFog = value;
        }
        
        public float Alpha
        {
            get => _buffer.Alpha;
            set => _buffer.Alpha = value;
        }
        
        
        public Vector3 Scale
        {
            get => _buffer.Scale;
            set => _buffer.Scale = value;
        }

        public static ObjectMesh FromVertexData(VertexData Data, bool CullPrematurely = true)
        {
            var mesh = new ObjectMesh(Vector3.Zero)
            {
                PrematureCulling = CullPrematurely
            };
            Executer.ExecuteOnMainThread( delegate
            {                                                  
                mesh.Mesh.BuildFrom(Data, false);
                mesh.Mesh.IsGenerated = true;
                mesh.Mesh.IsBuilded = true;
                mesh.Mesh.Enabled = true;
                mesh.CullingBox = Physics.BuildBroadphaseBox(Data);
            });
            
            return mesh;
            
        }

        public void Dispose()
        {
            DrawManager.Remove(this);
            UpdateManager.Remove(this);
            _buffer?.Dispose();
        }
    }
}
