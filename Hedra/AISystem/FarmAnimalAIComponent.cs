using System.Windows.Forms;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem
{
    public abstract class FarmAnimalAIComponent : CattleAIComponent
    {
        private readonly float _width;
        
        protected FarmAnimalAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent)
        {
            _width = Width;
            this.AlterBehaviour<RoamBehaviour>(new RoamAroundBehaviour(Parent, FarmPosition));
        }

        protected override float Radius => _width * 2;
    }
}