using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.ModelHandlers
{
    public class BeetleHandler : ModelHandler
    {
        public override void Process(IEntity Entity, AnimatedUpdatableModel Model)
        {
            Model.Paint(BeetleColors[Utils.Rng.Next(0, BeetleColors.Length)]);
        }

        private static readonly Vector4[] BeetleColors = new[]
        {
            Colors.FromHtml("#F24353"),
            Colors.FromHtml("#7CF23C"),
            Colors.FromHtml("#619CF2")
        };
    }
}