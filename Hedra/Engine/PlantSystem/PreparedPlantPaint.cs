using System.Numerics;
using System.Runtime.CompilerServices;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    // Immutable value keys capture every input to deterministic paint operations.
    public abstract record PreparedPlantPaint
    {
        public abstract void Apply(NativeVertexData Data);

        public sealed record Grass(Vector4 Color, Vector3 Lowest, float Span) : PreparedPlantPaint
        {
            public override void Apply(NativeVertexData Data)
            {
                Data.Paint(Color);
                for (var i = 0; i < Data.Vertices.Count; i++)
                {
                    var shade = Vector3.Dot(Data.Vertices[i] - Lowest, Vector3.UnitY) / Span;
                    Data.Colors[i] += new Vector4(.3f, .3f, .3f, 0) * shade;
                }
            }
        }

        public sealed record Fern(Vector4 Color) : PreparedPlantPaint
        {
            public override void Apply(NativeVertexData Data)
            {
                Data.AddWindValues();
                Data.Paint(Color);
                Data.GraduateColor(Vector3.UnitY);
            }
        }

        public sealed class Bounds
        {
            private static readonly ConditionalWeakTable<VertexData, Bounds> Cache = new();
            private Bounds(VertexData Model)
            {
                Highest = Model.SupportPoint(Vector3.UnitY);
                Lowest = Model.SupportPoint(-Vector3.UnitY);
            }
            public Vector3 Highest { get; }
            public Vector3 Lowest { get; }
            public static Bounds Get(VertexData Model) => Cache.GetValue(Model, model => new Bounds(model));
        }
    }
}
