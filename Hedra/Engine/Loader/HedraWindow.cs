using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Hedra.Engine.Events;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using NativeWindow = OpenToolkit.Windowing.Desktop.NativeWindow;

namespace Hedra.Engine.Loader
{
    public abstract class HedraWindow : NativeWindow, IEventProvider, IHedraWindow
    {
        public event Action<KeyPressEventArgs> KeyPress;
        private readonly Stopwatch _watch;
        private SpinWait _spinner;
        private bool _dryRun;
        private VSyncMode _vSync;
        public double TargetFramerate { get; set; }

        protected HedraWindow(int Width, int Height) : base(new NativeWindowSettings
        {
            Size = new Vector2i(Width, Height)
        })
        {
            _watch = new Stopwatch();
            _spinner = new SpinWait();
        }

        public void Run()
        {
            IsVisible = true;
            OnLoad();
            OnResize(new ResizeEventArgs(Size));
            _watch.Start();
            var lastTick = _watch.Elapsed.TotalSeconds;
            var elapsed = .0;
            var frames = 0;
            while (this.Exists && !this.IsExiting)
            {
                ProcessEvents();
                if (this.Exists && !this.IsExiting)
                {
                    var totalSeconds = _watch.Elapsed.TotalSeconds;
                    var time = Math.Min(1.0, totalSeconds - lastTick);
                    lastTick = totalSeconds;
                    this.DispatchUpdateFrame(time);
                    this.DispatchRenderFrame(time);
                    while (_watch.Elapsed.TotalSeconds - lastTick < TargetFramerate)
                    {
                        _spinner.SpinOnce();
                    }
                }
                if(_dryRun) break;
            }
        }

        public void RunOnce()
        {
            _dryRun = true;
            Run();
        }

        protected abstract void OnRenderFrame(double Delta);

        protected abstract void OnUpdateFrame(double Delta);

        private void DispatchUpdateFrame(double Delta)
        {
            this.OnUpdateFrame(Delta);
        }

        private void DispatchRenderFrame(double Delta)
        {
            this.OnRenderFrame(Delta);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (e.Cancel) return;
            OnUnload();
        }

        protected virtual void Dispose(bool Manual)
        {
        }

        protected abstract void OnLoad();

        protected abstract void OnUnload();

        public VSyncMode VSync
        {
            get => _vSync;
            set
            {
                switch (value)
                {
                    case VSyncMode.On:
                        Glfw.SwapInterval(1);
                        break;

                    case VSyncMode.Off:
                        Glfw.SwapInterval(0);
                        break;

                    case VSyncMode.Adaptive:
                        Glfw.SwapInterval(-1);
                        break;
                }

                _vSync = value;
            }
        }

        public int Width
        {
            get => Size.X;
            set => Size = new Vector2i(value, Size.Y);
        }
        public int Height
        {
            get => Size.Y;
            set => Size = new Vector2i(Size.X, value);
        }
    }
}