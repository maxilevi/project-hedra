using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering
{
    public class CapHandler : StateHandler<EnableCap>
    {
        protected override void DoEnable(EnableCap Index)
        {
            Renderer.Provider.Enable(Index);
#if DEBUG
            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine($"GLError: '{error}' when enabling cap '{Index}'");
#endif
        }

        protected override void DoDisable(EnableCap Index)
        {
            Renderer.Provider.Disable(Index);
        }
    }
}