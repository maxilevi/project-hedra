/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 05:51 a.m.
 *
 */
using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of GUITexture.
    /// </summary>
    public class GUITexture : IDisposable, ISimpleTexture, IAdjustable
    {
        public Vector2 AdjustedPosition { get; private set; }
        public bool Flipped { get; set; }
        public bool Fxaa { get; set; }
        public uint TextureId { get; set; }
        public float Opacity { get; set; }
        public uint BackGroundId { get; set; }
        public Vector2 Scale { get; set; }
        public bool Enabled {get; set;}
        public Vector4 Tint { get; set; }
        public bool Grayscale { get; set; }
        public float Angle { get; set; }
        public uint MaskId { get; set; }
        public Func<uint> IdPointer { get; set; }
        private Vector2 _position;
        private bool _disposed;
        private StackTrace _stack;

        public GUITexture(uint Id, Vector2 Scale, Vector2 Pos)
        {
            this.TextureId = Id;
            this.Position = Pos;
            this.Scale = Scale;
            this.Opacity = 1;
            this.Tint = Vector4.One;
            _stack = new StackTrace();
        }

        public void Adjust()
        {
            AdjustedPosition = Adjust(this.Position);
        }

        public static Vector2 Adjust(Vector2 Position)
        {
            var pixelDistance = Mathf.FromNormalizedDeviceCoordinates(Position.X, Position.Y);
            return Mathf.ToNormalizedDeviceCoordinates(
                pixelDistance.X / GameSettings.DeviceWidth * GameSettings.SurfaceWidth,
                pixelDistance.Y / GameSettings.DeviceHeight * GameSettings.SurfaceHeight
            );
        }

        public void Dispose()
        {
            if(_disposed) return;
            MaskId = this.DisposeId(MaskId);
            TextureId = this.DisposeId(TextureId);
            _disposed = true;
        }

        private uint DisposeId(uint DisposeId)
        {
            if (Array.IndexOf(GUIRenderer.InmortalTextures, DisposeId) != -1 || DisposeId == 0) return DisposeId;

            void DisposeProcess()
            {
                Graphics2D.Textures.Remove(DisposeId);
                Renderer.DeleteTexture(DisposeId);
            }
            if(System.Threading.Thread.CurrentThread.ManagedThreadId != Loader.Hedra.MainThreadId)
                Executer.ExecuteOnMainThread(DisposeProcess);
            else
                DisposeProcess();

            return DisposeId;
        }
                        
        public uint Id => IdPointer?.Invoke() ?? TextureId;
        
        public bool UseMask => MaskId != 0;
        
        public Matrix3 RotationMatrix => Matrix3.CreateFromAxisAngle(Vector3.UnitZ, Angle * Mathf.Radian);

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                this.Adjust();
            }
        }
        
        ~GUITexture()
        {
            if (!_disposed)
            {
                if(GameManager.IsExiting) return;
                Log.WriteLine($"Texture {Id} failed to dispose correctly.");
                Executer.ExecuteOnMainThread(this.Dispose);
            }
        }
    }
}
