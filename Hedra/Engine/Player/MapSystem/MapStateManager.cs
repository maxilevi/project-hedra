using System.Numerics;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapStateManager : StateManager
    {
        public MapStateManager(LocalPlayer Player)
        {
            RegisterStateItem(() => Player.View.MinPitch, O => Player.View.MinPitch = (float)O);
            RegisterStateItem(() => Player.View.MaxPitch, O => Player.View.MaxPitch = (float)O);
            RegisterStateItem(() => Player.View.MaxDistance, O => Player.View.MaxDistance = (float)O);
            RegisterStateItem(() => Player.View.MinDistance, O => Player.View.MinDistance = (float)O);
            RegisterStateItem(() => Player.View.WheelSpeed, O => Player.View.WheelSpeed = (float)O);
            RegisterStateItem(() => Player.View.TargetDistance, O => Player.View.TargetDistance = (float)O);
            RegisterStateItem(() => Player.View.Distance, O => Player.View.Distance = (float)O);
            RegisterStateItem(() => Player.Movement.CaptureMovement, O => Player.Movement.CaptureMovement = (bool)O);
            RegisterStateItem(() => Player.View.CaptureMovement, O => Player.View.CaptureMovement = (bool)O);
            RegisterStateItem(() => Player.View.LockMouse, O => Player.View.LockMouse = (bool)O);
            RegisterStateItem(() => Player.CanInteract, O => Player.CanInteract = (bool)O);
            RegisterStateItem(() => Player.Toolbar.Listen, O => Player.Toolbar.Listen = (bool)O);
            RegisterStateItem(() => WorldRenderer.Offset, O => WorldRenderer.Offset = (Vector3)O);
            RegisterStateItem(() => WorldRenderer.BakedOffset, O => WorldRenderer.BakedOffset = (Vector3)O);
            RegisterStateItem(() => WorldRenderer.Scale, O => WorldRenderer.Scale = (Vector3)O);
            RegisterStateItem(() => WorldRenderer.EnableCulling, O => WorldRenderer.EnableCulling = (bool)O);
            RegisterStateItem(() => WorldRenderer.TransformationMatrix,
                O => WorldRenderer.TransformationMatrix = (Matrix4x4)O);
            RegisterStateItem(() => WorldRenderer.WaterSmoothness, O => WorldRenderer.WaterSmoothness = (float)O);
            RegisterStateItem(() => SkyManager.UpdateDayColors, O => SkyManager.UpdateDayColors = (bool)O);
            RegisterStateItem(() => Player.Loader.ShouldUpdateFog, O => Player.Loader.ShouldUpdateFog = (bool)O);
        }
    }
}