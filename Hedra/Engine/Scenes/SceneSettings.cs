using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Scenes
{
    public class SceneSettings
    {
        public Vector3 LightColor { get; set; } = WorldLight.DefaultColor;
        public float LightRadius { get; set; } = PointLight.DefaultRadius;
        public bool IsNightLight { get; set; } = true;
    }
}