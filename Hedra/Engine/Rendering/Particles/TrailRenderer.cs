/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/06/2016
 * Time: 03:41 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Particles
{
    /// <summary>
    ///     Description of TrailRenderer.
    /// </summary>
    public class TrailRenderer : IRenderable, IDisposable
    {
        private static readonly Shader Shader;
        public Func<Vector3> Tip { get; set; }
        private readonly List<TrailPoint> _tipPoints;
        private VBO<Vector3> _points;
        private VBO<Vector4> _colors;
        private VAO<Vector3, Vector4> _data;
        public float Thickness { get; set; } = 1f;
        public int UpdateRate { get; set; } = 4;
        public Vector4 Color { get; set; }
        public float MaxLifetime { get; set; } = 1f;
        public Vector3 Orientation { get; set; } = Vector3.UnitY;
        private bool _buffersCreated;
        private bool _disposed;
        private bool _emit;
        private int _times;

        static TrailRenderer()
        {
            Shader = Shader.Build("Shaders/TrailRenderer.vert", "Shaders/TrailRenderer.frag");
        }

        public TrailRenderer(Func<Vector3> Tip, Vector4 Color)
        {
            this.Tip = Tip;
            this._tipPoints = new List<TrailPoint>();
            this.Color = Color;

            Executer.ExecuteOnMainThread(delegate
            {
                _points = new VBO<Vector3>(new Vector3[1], HedraSize.Vector3, VertexAttribPointerType.Float,
                    BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
                _colors = new VBO<Vector4>(new Vector4[1], HedraSize.Vector4, VertexAttribPointerType.Float,
                    BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
                _data = new VAO<Vector3, Vector4>(_points, _colors);
                _buffersCreated = true;
            });
               
            DrawManager.TrailRenderer.Add(this);
        }

        public bool Emit
        {
            get => _emit;
            set
            {
                if(!_emit && value)
                    _tipPoints.Clear();
                _emit = value;
                
            }
        }

        public void Update()
        {
            if(Time.Paused || !_buffersCreated || _disposed || _points.Count == 0 && !Emit) return;

            for (var i = _tipPoints.Count - 1; i > -1; i--)
            {
                _tipPoints[i] -= Time.DeltaTime;

                if (_tipPoints[i].Lifetime < 0) _tipPoints.RemoveAt(i);
            }

            var smoothPoints = new List<TrailPoint>();
            var points = new List<TrailPoint>();
            var pointsTemp = new List<TrailPoint>();
            var colors = new List<Vector4>();

            for (var i = 1; i < _tipPoints.Count; i++)
            {
                this.SmoothPoint(_tipPoints[i - 1], _tipPoints[i], pointsTemp);            
            }

            for (var i = 1; i < pointsTemp.Count; i++)
            {
                this.SmoothPoint(pointsTemp[i - 1], pointsTemp[i], points);
            }

            for (var i = 1; i < points.Count; i++)
            {
                this.SmoothPoint(points[i - 1], points[i], smoothPoints);
            }
            points.Clear();

            if (smoothPoints.Count >= 1)
            {
                points.Add(smoothPoints[0]);
                colors.Add(new Vector4(Color.Xyz(), smoothPoints[0].Alpha));
            }

            for (var i = 1; i < smoothPoints.Count-1; i++)
            {
                var a = smoothPoints[i - 1].Point;
                var b = smoothPoints[i].Point;
                var dir = b - a;
                var ndir = dir.NormalizedFast();

                var perp = ndir.Cross(Orientation);
                var c = b - perp * (Thickness / 2);
                var d = b + perp * (Thickness / 2);

                points.Add( new TrailPoint(d, smoothPoints[i-1].Lifetime, smoothPoints[i - 1].MaxLifetime, 0f));
                points.Add( new TrailPoint(c, smoothPoints[i].Lifetime, smoothPoints[i].MaxLifetime, .3f));
                
                colors.Add(new Vector4(Color.Xyz(), Color.W * points[points.Count - 2].Alpha));
                colors.Add(new Vector4(Color.Xyz(), Color.W * points[points.Count - 1].Alpha));
            }
            if (smoothPoints.Count >= 1)
            {
                points.Add(smoothPoints[smoothPoints.Count - 1]);
                colors.Add(new Vector4(Color.Xyz(), Color.W * points[points.Count-1].Alpha ));
            }
            Executer.ExecuteOnMainThread(() =>
            {
                if(_disposed) return;
                _points.Update(points.Select(p => p.Point).ToArray(), points.Count * HedraSize.Vector3);
                _colors.Update(colors.ToArray(), colors.Count * HedraSize.Vector4);
            });
            if (!Emit) return;
            
            if(_times % UpdateRate == 0)
            {
                var maxLifetime = 0.35f * MaxLifetime;
                _tipPoints.Add( new TrailPoint(Tip(), maxLifetime * .75f, maxLifetime, .0f) );
                _times = 0;
            }
            _times++;
        }

        private void Smooth(Vector3 p0, Vector3 p1, List<Vector3> output)
        {
            var q = new Vector3(0.75f * p0.X + 0.25f * p1.X, 0.75f * p0.Y + 0.25f * p1.Y, 0.75f * p0.Z + 0.25f * p1.Z);
            var r = new Vector3(0.25f * p0.X + 0.75f * p1.X, 0.25f * p0.Y + 0.75f * p1.Y, 0.25f * p0.Z + 0.75f * p1.Z);

            output.Add(q);
            output.Add(r);
        }

        private void SmoothPoint(TrailPoint tp0, TrailPoint tp1, List<TrailPoint> output)
        {
            var p0 = tp0.Point;
            var p1 = tp1.Point;

            var q0 = new Vector3(0.75f * p0.X + 0.25f * p1.X, 0.75f * p0.Y + 0.25f * p1.Y, 0.75f * p0.Z + 0.25f * p1.Z);
            var r0 = new Vector3(0.25f * p0.X + 0.75f * p1.X, 0.25f * p0.Y + 0.75f * p1.Y, 0.25f * p0.Z + 0.75f * p1.Z);

            output.Add(new TrailPoint(q0, tp0.Lifetime, tp0.MaxLifetime, tp0.AlphaOffset));
            output.Add(new TrailPoint(r0, tp1.Lifetime, tp1.MaxLifetime, tp1.AlphaOffset));
        }

        public void Draw()
        {
            if(_tipPoints.Count <= 4 || !_buffersCreated) return;
            Renderer.Disable(EnableCap.CullFace);
            Renderer.Enable(EnableCap.Blend);

            Shader.Bind();        
            _data.Bind();

            Renderer.DrawArrays(PrimitiveType.TriangleStrip, 0, _points.Count);

            _data.Unbind();
            Shader.Unbind();

            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.CullFace);
        }

        public void Dispose()
        {
            _disposed = true;
            void DisposeBuffers()
            {
                this._points.Dispose();
                this._colors.Dispose();
                this._data.Dispose();
            }
            if(_buffersCreated) DisposeBuffers();
            else Executer.ExecuteOnMainThread(DisposeBuffers);
            DrawManager.TrailRenderer.Remove(this);
            Tip = null;
        }
    }

    public struct TrailPoint
    {
        public Vector3 Point;
        public float Lifetime { get; }
        public float MaxLifetime { get; }
        public float AlphaOffset { get; }

        public TrailPoint(Vector3 Point, float Lifetime, float MaxLifetime, float AlphaOffset)
        {
            this.Point = Point;
            this.Lifetime = Lifetime;
            this.MaxLifetime = MaxLifetime;
            this.AlphaOffset = AlphaOffset;
        }

        public static TrailPoint operator -(TrailPoint Item, float Time)
        {
            return new TrailPoint(Item.Point, Item.Lifetime - Time, Item.MaxLifetime, Item.AlphaOffset);
        }

        public float Alpha => Lifetime / MaxLifetime - AlphaOffset;
    }
}