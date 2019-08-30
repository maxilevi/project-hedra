using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Scenes
{
    public class SceneSettings
    {
        public Vector3 LightColor { get; set; } = WorldLight.DefaultColor;
        public float LightRadius { get; set; } = PointLight.DefaultRadius * 2;
        public bool DisableLightsAtNight { get; set; } = false;
    }
}