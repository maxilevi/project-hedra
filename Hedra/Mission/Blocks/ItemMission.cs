using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem;
using Hedra.Items;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Mission.Blocks
{
    public abstract class ItemMission : MissionBlock
    {
        public ItemCollect[] Items { get; set; }
        public sealed override bool IsCompleted => Items.All(I => I.IsCompleted(Owner, out _));
        public sealed override bool HasLocation => false;

        public sealed override QuestView BuildView()
        {
            var items = Items.Select(T => ItemPool.Grab(T.Name)).ToArray();
            var model = new VertexData();
            for (var i = 0; i < items.Length; i++)
            {
                var transform = Matrix4x4.CreateTranslation(Vector3.UnitZ);
                transform *= Matrix4x4.CreateRotationY(i * (360 / items.Length) * Mathf.Radian);
                model += items[i].Model.Clone().Transform(transform);
            }
            return new ModelView(model);
        }
        public void ConsumeItems()
        {
            for (var i = 0; i < Items.Length; ++i)
            {
                Items[i].Consume(Owner);
            }
        }
    }
}