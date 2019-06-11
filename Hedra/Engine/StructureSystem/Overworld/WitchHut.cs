using System;
using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHut : BaseStructure, ICompletableStructure
    {
        public IEntity[] Enemies { get; set; }
        public bool Completed => throw new NotImplementedException();
        public ItemDescription DeliveryItem => throw new NotImplementedException();

        public WitchHut(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Enemies == null) return;
            for (var i = 0; i < Enemies.Length; i++)
            {
                Enemies[i].Dispose();   
            }
        }
    }
}