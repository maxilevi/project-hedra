using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHut : BaseStructure, ICompletableStructure
    {
        public WitchHut(Vector3 Position) : base(Position)
        {
        }

        public IHumanoid[] Enemies { get; set; }
        public int EnemiesLeft => Enemies.Count(E => !E.IsDead);
        public Vector3 StealPosition { get; set; }
        public Vector3 Witch0Position { get; set; }
        public Vector3 Witch1Position { get; set; }
        public bool Completed => EnemiesLeft == 0;

        public override void Dispose()
        {
            base.Dispose();
            if (Enemies == null) return;
            for (var i = 0; i < Enemies.Length; i++) Enemies[i].Dispose();
        }

        public void EnsureHutIsEmpty()
        {
            if (Enemies == null) return;
            for (var i = 0; i < Enemies.Length; ++i)
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

            if (female != null) enemies.Add(female);
            if (male != null) enemies.Add(male);
            Enemies = enemies.ToArray();
        }
    }
}