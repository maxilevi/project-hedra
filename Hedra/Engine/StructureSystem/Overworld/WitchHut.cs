using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Mission;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHut : BaseStructure, ICompletableStructure
    {
        public IEntity[] Enemies { get; set; }
        public int EnemiesLeft => Enemies.Count(E => !E.IsDead);
        public bool Completed => EnemiesLeft == 0;

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