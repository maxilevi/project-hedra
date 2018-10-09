using System;
using System.ComponentModel;
using System.Diagnostics;
using Hedra.Engine.Events;
using OpenTK;
using OpenTK.Graphics;

namespace Hedra.Engine
{
    public class HedraWindow : NativeWindow, IEventProvider
    {
        private IGraphicsContext _glContext;
        private bool _isExiting;
        private readonly FrameEventArgs _renderArgs;
        private readonly FrameEventArgs _updateArgs;
        private readonly Stopwatch _watch;

        protected HedraWindow(int Width, int Height, GraphicsMode Mode, string Title, GameWindowFlags Options,
            DisplayDevice Device, int Major, int Minor, GraphicsContextFlags Flags)
            : base(Width, Height, Title, Options, Mode, Device)
        {
            try
            {
                _watch = new Stopwatch();
                _renderArgs = new FrameEventArgs();
                _updateArgs = new FrameEventArgs();
                _glContext = new GraphicsContext(Mode, WindowInfo, Major, Minor, Flags);
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
            Visible = true;
            OnLoad(EventArgs.Empty);
            OnResize(EventArgs.Empty);
            _watch.Start();
            var lastTick = _watch.Elapsed.TotalSeconds;
            var elapsed = .0;
            var frames = 0;
            const int targetFramerate = 60;
            while (this.Exists && !this.IsExiting)
            {
                ProcessEvents();
                if (this.Exists && !this.IsExiting)
                {
                    var totalSeconds = _watch.Elapsed.TotalSeconds;
                    var time = totalSeconds - lastTick;
                    lastTick = totalSeconds;
                    this.DispatchUpdateFrame(time);
                    this.DispatchRenderFrame(time);
                }
            }
        }
               
        protected virtual void OnRenderFrame(double Delta)
        {
        }

        protected virtual void OnUpdateFrame(double Delta)
        {           
        }

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

        protected virtual void OnLoad(EventArgs E)
        {          
        }

        protected virtual void OnUnload(EventArgs E)
        {       
        }

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