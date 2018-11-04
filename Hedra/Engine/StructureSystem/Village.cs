using System.Collections.Generic;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class Village : BaseStructure
    {
        private readonly List<IHumanoid> _humans;
        
        public Village(Vector3 Position) : base(Position)
        {
            _humans = new List<IHumanoid>();
        }

        public void AddHumanoid(IHumanoid Human)
        {
            _humans.Add(Human);
        }

        public override void Dispose()
        {
            base.Dispose();
            for (var i = 0; i < _humans.Count; i++)
            {
                _humans[i].Dispose();
            }
        }
    }
}