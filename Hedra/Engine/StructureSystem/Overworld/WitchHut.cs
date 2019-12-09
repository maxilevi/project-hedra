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
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHut : BaseStructure, ICompletableStructure
    {
        public IHumanoid[] Enemies { get; set; }
        public int EnemiesLeft => Enemies.Count(E => !E.IsDead);
        public bool Completed => EnemiesLeft == 0;
        public Vector3 StealPosition { get; set; }
        public Vector3 Witch0Position { get; set; }
        public Vector3 Witch1Position { get; set; }

        public WitchHut(Vector3 Position) : base(Position)
        {
        }

        public void EnsureHutIsEmpty()
        {
            if(Enemies == null) return;
            for(var i = 0; i < Enemies.Length; ++i)
                Enemies[i].Dispose();
        }
        
        /* Used from python */
        public void EnsureWitchesSpawned()
        {
            EnsureHutIsEmpty();
            CreateNPCs(Utils.Rng);
        }
        
        private void CreateNPCs(Random Rng)
        {
            var enemies = new List<IHumanoid>();
            IHumanoid female = null, male = null;
            if (Rng.Next(0, 8) != 1)
            {
                female = NPCCreator.SpawnHumanoid(
                    HumanType.Witch,
                    Witch0Position
                );
                HumanoidFactory.AddAI(female, false);
            }
            if (female == null || Rng.Next(0, 8) != 1)
            {
                male = NPCCreator.SpawnHumanoid(
                    HumanType.Witch,
                    Witch1Position
                );
                HumanoidFactory.AddAI(male, false);
            }

            if (female != null && male != null)
            {
                female.SearchComponent<DamageComponent>().Ignore(E => E == male);
                male.SearchComponent<DamageComponent>().Ignore(E => E == female);
            }
            if(female != null) enemies.Add(female);
            if(male != null) enemies.Add(male);
            Enemies = enemies.ToArray();
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