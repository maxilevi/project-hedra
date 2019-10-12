using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenToolkit.Mathematics;

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
            this.RegisterStateItem(() => Player.View.WheelSpeed, O => Player.View.WheelSpeed = (float)O);
            this.RegisterStateItem(() => Player.View.TargetDistance, O => Player.View.TargetDistance = (float)O);
            this.RegisterStateItem(() => Player.View.Distance, O => Player.View.Distance = (float)O);
            this.RegisterStateItem(() => Player.Movement.CaptureMovement, O => Player.Movement.CaptureMovement = (bool)O);
            this.RegisterStateItem(() => Player.View.CaptureMovement, O => Player.View.CaptureMovement = (bool)O);
            this.RegisterStateItem(() => Player.View.LockMouse, O => Player.View.LockMouse = (bool)O);
            this.RegisterStateItem(() => Player.CanInteract, O => Player.CanInteract = (bool)O);
            this.RegisterStateItem(() => Player.Toolbar.Listen, O => Player.Toolbar.Listen = (bool)O);
            this.RegisterStateItem(() => WorldRenderer.Offset, O => WorldRenderer.Offset = (Vector3)O);
            this.RegisterStateItem(() => WorldRenderer.BakedOffset, O => WorldRenderer.BakedOffset = (Vector3)O);
            this.RegisterStateItem(() => WorldRenderer.Scale, O => WorldRenderer.Scale = (Vector3)O);
            this.RegisterStateItem(() => WorldRenderer.EnableCulling, O => WorldRenderer.EnableCulling = (bool)O);
            this.RegisterStateItem(() => WorldRenderer.TransformationMatrix, O => WorldRenderer.TransformationMatrix = (Matrix4)O);
            this.RegisterStateItem(() => WorldRenderer.WaterSmoothness, O => WorldRenderer.WaterSmoothness = (float)O);
            this.RegisterStateItem(() => SkyManager.UpdateDayColors, O => SkyManager.UpdateDayColors = (bool)O);
            this.RegisterStateItem(() => Player.Loader.ShouldUpdateFog, O => Player.Loader.ShouldUpdateFog = (bool)O);
        }
    }
}
