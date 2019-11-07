using System;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using System.Numerics;
using Hedra.EntitySystem;

namespace Hedra.Engine.Scenes
{
    public class SceneSettings
    {
        public Vector3 LightColor { get; set; } = WorldLight.DefaultColor;
        public float LightRadius { get; set; } = PointLight.DefaultRadius;
        public bool IsNightLight { get; set; } = true;
        public Func<Vector3, IEntity> Npc1Creator { get; set; }
        public Func<Vector3, IEntity> Npc2Creator { get; set; }
        public Func<Vector3, VertexData, BaseStructure> Structure1Creator { get; set; }
        public Func<Vector3, VertexData, BaseStructure> Structure2Creator { get; set; }
    }
}