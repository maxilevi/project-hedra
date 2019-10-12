using System;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering
{
    public interface IModel
    {
        Vector4 Tint { get; set; }
        Vector4 BaseTint { get; set; }
        Vector3 Scale { get; set; }
        Vector3 Position { get; set; }
        Vector3 LocalRotation { get; set; }
        bool Enabled { get; set; }
        bool ApplyFog { get; set; }
        bool Pause { get; set; }
        float Alpha { get; set; }
        float AnimationSpeed { get; set; }
        Vector4 OutlineColor { get; set; }
        bool Outline { get; set; }
        void Dispose();
    }

    public interface ICullableModel : IModel, ICullable
    {
        Vector3 Position { get; set; }
        bool Enabled { get; set; }
        Box CullingBox { get; set; }
    }
}