using System;
using Hedra.Game;

namespace Hedra.Engine.Rendering
{
    public interface IModelData
    {
        uint[] Indices { get; }
    }

    public static class IModelDataExtensions
    {
        public static void AssertTriangulated(this IModelData Model)
        {
            if (GameSettings.TestingMode) return;
            if (Model.Indices.Length % 3 != 0)
                throw new ArgumentOutOfRangeException(
                    $"ModelData with '{Model.Indices.Length}' indices is not triangulated correctly");
        }
    }
}