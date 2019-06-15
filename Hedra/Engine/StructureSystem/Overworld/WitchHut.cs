using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHut : BaseStructure, ICompletableStructure
    {
        public IEntity[] Enemies { get; set; }
        public int EnemiesLeft => Enemies.Count(E => !E.IsDead);
        public bool Completed => EnemiesLeft == 0;
        public Item PickupItem { get; set; }
        public Item RewardItem { get; set; }

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
        
        public ItemDescription DeliveryItem => ItemDescription.FromItem(PickupItem, null);

        public QuestReward Reward => (RewardItem != null)
            ? new QuestReward
            {
                Item = RewardItem
            }
            : null;
    }
}