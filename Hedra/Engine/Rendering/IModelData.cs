using System;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public interface IModelData
    {
        Vector3[] Vertices { get; }
        Vector3[] Colors { get; }
        Vector3[] Normals { get; }
        uint[] Indices { get; }
    }

    public static class IModelDataExtensions
    {
        public static void AssertTriangulated(this IModelData Model)
        {
            if(Model.Vertices.Length % 3 != 0)
                throw new ArgumentOutOfRangeException($"ModelData with '{Model.Vertices.Length}' vertices is not triangulated correctly");
            if(Model.Colors.Length % 3 != 0)
                throw new ArgumentOutOfRangeException($"ModelData with '{Model.Colors.Length}' colors is not triangulated correctly");
            if(Model.Normals.Length % 3 != 0)
                throw new ArgumentOutOfRangeException($"ModelData with '{Model.Normals.Length}' normals is not triangulated correctly");
            if(Model.Indices.Length % 3 != 0)
                throw new ArgumentOutOfRangeException($"ModelData with '{Model.Indices.Length}' indices is not triangulated correctly");
        }
    }
}