using System;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class PantsPiece : ArmorPiece
    {
        public override Matrix4 PlacementMatrix => throw new ArgumentOutOfRangeException();

        public PantsPiece(VertexData Model) : base(Model)
        {
        }
    }
}