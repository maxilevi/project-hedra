using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Hedra.Engine.Events;
using OpenTK;
using OpenTK.Graphics;

namespace Hedra.Engine.Loader
{
    public abstract class HedraWindow : NativeWindow, IEventProvider, IHedraWindow
    {
        private IGraphicsContext _glContext;
        private bool _isExiting;
        private readonly Stopwatch _watch;
        private SpinWait _spinner;
        private bool _dryRun;
        public double TargetFramerate { get; set; }

        protected HedraWindow(int Width, int Height, GraphicsMode Mode, string Title, GameWindowFlags Options,
            DisplayDevice Device, int Major, int Minor, GraphicsContextFlags Flags)
            : base(Width, Height, Title, Options, Mode, Device)
        {
            try
            {
                _watch = new Stopwatch();
                _spinner = new SpinWait();
#if DEBUG
                _glContext = new GraphicsContext(Mode, WindowInfo, 2, 1, GraphicsContextFlags.Debug);
#else
                _glContext = new GraphicsContext(Mode, WindowInfo, Major, Minor, Flags);
#endif
                _glContext.MakeCurrent(WindowInfo);
                _glContext.LoadAll();
            }
            catch (Exception ex)
            {
                base.Dispose();
                throw;
            }
        }

        private IGraphicsContext Context
        {
            get
            {
                EnsureUndisposed();
                return _glContext;
            }
        }

        public bool IsExiting
        {
            get
            {
                EnsureUndisposed();
                return _isExiting;
            }
        }

        public VSyncMode VSync
        {
            get
            {
                EnsureUndisposed();
                GraphicsContext.Assert();
                if (Context.SwapInterval < 0)
                    return VSyncMode.Adaptive;
                return Context.SwapInterval == 0 ? VSyncMode.Off : VSyncMode.On;
            }
            set
            {
                EnsureUndisposed();
                GraphicsContext.Assert();
                switch (value)
                {
                    case VSyncMode.Off:
                        Context.SwapInterval = 0;
                        break;
                    case VSyncMode.On:
                        Context.SwapInterval = 1;
                        break;
                    case VSyncMode.Adaptive:
                        Context.SwapInterval = -1;
                        break;
                }
            }
        }

        public override WindowState WindowState
        {
            get => base.WindowState;
            set
            {
                base.WindowState = value;
                Context?.Update(WindowInfo);
            }
        }

        public virtual void Exit()
        {
            Close();
        }

        public void Run()
        {
            EnsureUndisposed();
            Visible = true;
            OnLoad(EventArgs.Empty);
            OnResize(EventArgs.Empty);
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

        protected void SwapBuffers()
        {
            EnsureUndisposed();
            Context.SwapBuffers();
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (e.Cancel) return;
            _isExiting = true;
            OnUnload(EventArgs.Empty);
        }

        protected virtual void Dispose(bool Manual)
        {
        }

        protected abstract void OnLoad(EventArgs E);

        protected abstract void OnUnload(EventArgs E);

        protected override void OnResize(EventArgs E)
        {
            base.OnResize(E);
            _glContext.Update(WindowInfo);
        }


        public override void Dispose()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                try
                {
                    if (_glContext != null)
                    {
                        _glContext.Dispose();
                        _glContext = null;
                    }
                }
                finally
                {
                    base.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}