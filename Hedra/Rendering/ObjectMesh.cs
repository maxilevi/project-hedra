/*
 * Author: Zaphyk
 * Date: 03/03/2016
 * Time: 09:08 p.m.
 *
 */

using System;
using System.Diagnostics;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Numerics;

namespace Hedra.Rendering
{
    public class ObjectMesh : IRenderable, IDisposable, ICullableModel, IUpdatable
    {
        private readonly ObjectMeshBuffer _buffer;
        private readonly Timer _underChunkTimer;
        private readonly bool _updateInBackground;
        private bool _enabled;
        private Vector3 _lastPosition;
        private StackTrace _trace = new StackTrace();
        private Chunk _underChunk;

        public ObjectMesh(VertexData Data)
        {
            _buffer = new ObjectMeshBuffer(Data);
        }

        private ObjectMesh(Vector3 Position, VertexData Data, bool UpdateInBackground)
        {
            Enabled = true;
            _buffer = new ObjectMeshBuffer(Data);
            _updateInBackground = UpdateInBackground;
            Mesh = new ChunkMesh(Position, _buffer)
            {
                IsGenerated = true,
                IsBuilded = true,
                Enabled = true
            };
            CullingBox = Physics.BuildBroadphaseBox(Data);

            this.Position = Position;
            LocalRotation = Vector3.Zero;
            Mesh.Enabled = true;
            Enabled = true;
            _underChunkTimer = new Timer(4 + Utils.Rng.NextFloat() * 2)
            {
                UseTimeScale = false
            };
            DrawManager.AddObjectMesh(this);
            if (_updateInBackground)
                BackgroundUpdater.Add(this);
            else
                UpdateManager.Add(this);
        }

        public ChunkMesh Mesh { get; }
        public bool IsInWater { get; private set; }

        public bool ApplyNoiseTexture
        {
            get => _buffer.UseNoiseTexture;
            set => _buffer.UseNoiseTexture = value;
        }

        public Matrix4x4 TransformationMatrix
        {
            get => _buffer.TransformationMatrix;
            set => _buffer.TransformationMatrix = value;
        }

        public bool Dither
        {
            get => _buffer.Dither;
            set => _buffer.Dither = value;
        }

        public bool ApplySSAO
        {
            get => _buffer.ApplySSAO;
            set => _buffer.ApplySSAO = value;
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
                if (_buffer.Rotation == value) return;
                _buffer.Rotation = value;
            }
        }

        public Vector3 LocalRotationPoint
        {
            get => _buffer.LocalRotationPoint;
            set => _buffer.LocalRotationPoint = value;
        }

        public bool UseLegacyRotation
        {
            get => _buffer.UseLegacyRotation;
            set => _buffer.UseLegacyRotation = value;
        }

        public float AnimationSpeed { get; set; } = 1f;
        public bool PrematureCulling { get; set; } = true;
        public Box CullingBox { get; set; }
        public bool WasCulled { private get; set; }
        public Vector3 Max => CullingBox?.Max ?? Vector3.Zero;
        public Vector3 Min => CullingBox?.Min ?? Vector3.Zero;

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

        public Vector3 Position
        {
            get => _buffer.Position;
            set => _buffer.Position = value;
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

        public void Dispose()
        {
            DrawManager.RemoveObjectMesh(this);
            if (_updateInBackground)
                BackgroundUpdater.Remove(this);
            else
                UpdateManager.Remove(this);
            _buffer?.Dispose();
        }

        public void Draw()
        {
            if (Enabled)
                Mesh.Draw();
        }

        public void Update()
        {
            if (_lastPosition != Position || _underChunkTimer.Tick())
            {
                _lastPosition = Position;
                _underChunk = World.GetChunkAt(Position);
                IsInWater = _underChunk?.HasWater ?? false;
            }

            _buffer.LocalRotation = LocalRotation;
        }

        public Vector3 TransformPoint(Vector3 Point)
        {
            return _buffer.TransformPoint(Point);
        }

        public static ObjectMesh FromVertexData(VertexData Data, bool CullPrematurely = true,
            bool UpdateInBackground = true)
        {
            var mesh = new ObjectMesh(Vector3.Zero, Data, UpdateInBackground)
            {
                PrematureCulling = CullPrematurely
            };
            return mesh;
        }
    }
}