using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapStateManager : StateManager
    {
        public MapStateManager(LocalPlayer Player)
        {
            this.RegisterStateItem(() => Player.View.MinPitch, O => Player.View.MinPitch = (float)O);
            this.RegisterStateItem(() => Player.View.MaxPitch, O => Player.View.MaxPitch = (float)O);
            this.RegisterStateItem(() => Player.View.MaxDistance, O => Player.View.MaxDistance = (float)O);
            this.RegisterStateItem(() => Player.View.MinDistance, O => Player.View.MinDistance = (float)O);
            this.RegisterStateItem(() => Player.Movement.Check, O => Player.Movement.Check = (bool)O);
            this.RegisterStateItem(() => Player.View.Check, O => Player.View.Check = (bool)O);
            this.RegisterStateItem(() => Player.View.LockMouse, O => Player.View.LockMouse = (bool)O);
            this.RegisterStateItem(() => Player.CanInteract, O => Player.CanInteract = (bool)O);
            this.RegisterStateItem(() => WorldRenderer.Offset, O => WorldRenderer.Offset = (Vector3)O);
            this.RegisterStateItem(() => WorldRenderer.BakedOffset, O => WorldRenderer.BakedOffset = (Vector3)O);
            this.RegisterStateItem(() => WorldRenderer.Scale, O => WorldRenderer.Scale = (Vector3)O);
            this.RegisterStateItem(() => WorldRenderer.EnableCulling, O => WorldRenderer.EnableCulling = (bool)O);
            this.RegisterStateItem(() => SkyManager.FogManager.MinDistance, O =>
            SkyManager.FogManager.UpdateFogSettings( (float) O, SkyManager.FogManager.MaxDistance));
            this.RegisterStateItem(() => SkyManager.FogManager.MaxDistance, O =>
            SkyManager.FogManager.UpdateFogSettings(SkyManager.FogManager.MinDistance, (float) O));
        }
    }
}
