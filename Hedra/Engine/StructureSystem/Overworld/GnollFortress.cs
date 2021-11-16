using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GnollFortress : BaseStructure, ICompletableStructure
    {
        public GnollFortress(List<BaseStructure> Children, List<IEntity> Npcs) : base(Children, Npcs)
        {
        }

        public GnollFortress(Vector3 Position) : base(Position)
        {
        }

        public IEntity[] Enemies { get; set; }

        public bool Completed => Enemies.All(E => E.IsDead);
    }
}