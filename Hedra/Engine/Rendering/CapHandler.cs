﻿using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    public class CapHandler : StateHandler<EnableCap>
    {
        protected override void DoEnable(EnableCap Index)
        {
            Renderer.Provider.Enable(Index);
        }

        protected override void DoDisable(EnableCap Index)
        {
            Renderer.Provider.Disable(Index);
        }
    }
}
