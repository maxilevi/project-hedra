using Hedra.Engine.Enviroment;
using Hedra.Engine.Management;

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
            this.RegisterStateItem(() => Player.View.Check, O => Player.View.Check = (float)O);
            this.RegisterStateItem(() => SkyManager.FogManager.MinDistance, O =>
            SkyManager.FogManager.UpdateFogSettings( (float) O, SkyManager.FogManager.MaxDistance));
            this.RegisterStateItem(() => SkyManager.FogManager.MaxDistance, O =>
            SkyManager.FogManager.UpdateFogSettings(SkyManager.FogManager.MinDistance, (float) O));
        }
    }
}
