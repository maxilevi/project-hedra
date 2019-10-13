using System;
using Hedra.Engine.Events;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using NativeWindow = OpenToolkit.Windowing.Desktop.NativeWindow;
using GLDrawBuffersEnum = OpenToolkit.Graphics.GL33.DrawBufferMode;

namespace Hedra.Engine.Loader
{
    public abstract class HedraWindow : NativeWindow, IEventProvider, IHedraWindow
    {
        protected HedraWindow(int Width, int Height) : base(new NativeWindowSettings
        {
            Size = new Vector2i(Width, Height)
        }) { }
        public double TargetFramerate { get; set; }
        
        public void Exit()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void RunOnce()
        {
            throw new NotImplementedException();
        }
    }
}