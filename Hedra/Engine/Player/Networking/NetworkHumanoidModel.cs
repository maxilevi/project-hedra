using Hedra.Engine.ModuleSystem.Templates;
using Hedra.EntitySystem;

namespace Hedra.Engine.Player.Networking
{
    public class NetworkHumanoidModel : HumanoidModel
    {
        public NetworkHumanoidModel(IHumanoid Human, HumanoidModelTemplate Template) : base(Human, Template)
        {
        }

        public NetworkHumanoidModel(IHumanoid Human, HumanType Type) : base(Human, Type)
        {
        }

        public NetworkHumanoidModel(IHumanoid Human) : base(Human)
        {
        }

        protected override HumanoidModelAnimationState BuildAnimationHandler(IHumanoid Humanoid)
        {
            return new NetworkHumanoidModelAnimationState(Humanoid, this);
        }
    }
}