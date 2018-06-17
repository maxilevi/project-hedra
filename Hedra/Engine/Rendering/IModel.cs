using System;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///  Now works as a base class for the HumanModel & QuadrupedModel
    /// </summary>
    public interface IModel
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
}