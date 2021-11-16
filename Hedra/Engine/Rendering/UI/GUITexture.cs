/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 05:51 a.m.
 *
 */

using System;
using System.Diagnostics;
using System.Numerics;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     Description of GUITexture.
    /// </summary>
    public class GUITexture : DrawableTexture, IDisposable, ISimpleTexture, IAdjustable
    {
        private bool _disposed;
        private uint _id;
        private Vector2 _position;
        private StackTrace _stack;

        public GUITexture(uint Id, Vector2 Scale, Vector2 Position)
        {
            this.Scale = Scale;
            this.Position = Position;
            TextureId = Id;
            Opacity = 1;
            Tint = Vector4.One;
            _stack = new StackTrace();
        }

        public Vector2 AdjustedPosition { get; private set; }
        public bool Flipped { get; set; }
        public float Opacity { get; set; }
        public uint BackGroundId { get; set; }
        public bool Enabled { get; set; }
        public Vector4 Tint { get; set; }
        public bool Grayscale { get; set; }
        public float Angle { get; set; }
        public uint MaskId { get; set; }
        public Func<uint> IdPointer { get; set; }

        public uint Id => IdPointer?.Invoke() ?? TextureId;

        public bool UseMask => MaskId != 0;

        public Matrix4x4 RotationMatrix => Matrix4x4.CreateRotationZ(Angle * Mathf.Radian);

        public void Adjust()
        {
            AdjustedPosition = Adjust(Position);
        }

        public void Dispose()
        {
            if (_disposed) return;
            MaskId = DisposeId(MaskId, UseTextureCache);
            TextureId = DisposeId(TextureId, UseTextureCache);
            _disposed = true;
        }

        public Vector2 Scale { get; set; }

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                Adjust();
            }
        }

        public static Vector2 Adjust(Vector2 Position)
        {
            var pixelDistance = Mathf.FromNormalizedDeviceCoordinates(Position.X, Position.Y);
            return Mathf.ToNormalizedDeviceCoordinates(
                pixelDistance.X / GameSettings.DeviceWidth * GameSettings.SurfaceWidth,
                pixelDistance.Y / GameSettings.DeviceHeight * GameSettings.SurfaceHeight
            );
        }

        private static uint DisposeId(uint DisposeId, bool UseTextureCache)
        {
            if (IsImmortal(DisposeId) || DisposeId == 0) return DisposeId;

            if (UseTextureCache)
                TextureRegistry.Remove(DisposeId);

            return DisposeId;
        }

        private static bool IsImmortal(uint Id)
        {
            return Array.IndexOf(GUIRenderer.ImmortalTextures, Id) != -1;
        }

        ~GUITexture()
        {
            if (!_disposed)
            {
                if (GameManager.IsExiting || Program.IsDummy || GameSettings.TestingMode) return;
                Log.WriteLine($"Texture {Id} failed to dispose correctly.");
                Executer.ExecuteOnMainThread(Dispose);
            }
        }
    }
}