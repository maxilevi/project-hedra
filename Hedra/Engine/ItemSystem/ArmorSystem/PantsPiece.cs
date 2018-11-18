using System;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class PantsPiece : ArmorPiece
    {
        protected override Matrix4 PlacementMatrix => throw new ArgumentOutOfRangeException();

        protected override Vector3 PlacementPosition => throw new ArgumentOutOfRangeException();

        public PantsPiece(VertexData Model) : base(Model)
        {
        }
    }
}