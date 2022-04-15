using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld;

public interface IStructureWithRadius
{
    float Radius { get; }
    bool Completed { get; }
    Vector3 Position { get; }
}