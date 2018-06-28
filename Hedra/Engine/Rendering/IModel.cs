using System;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    internal interface IModel
    {
        Vector4 Tint { get; set; }
        Vector4 BaseTint { get; set; }
        Vector3 Scale { get; set; }
        Vector3 Position { get; set; }
        Vector3 Rotation { get; set; }
        bool Enabled { get; set; }
        bool ApplyFog { get; set; }
        bool Pause { get; set; }
        float Alpha { get; set; }
        float AnimationSpeed { get; set; }

        void Dispose();
    }

    internal interface ICullableModel : IModel, ICullable
    {
        Vector3 Position { get; set; }
        bool Enabled { get; set; }
    }
}