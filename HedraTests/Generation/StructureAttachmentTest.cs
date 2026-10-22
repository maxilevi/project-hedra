using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using Hedra;
using Hedra.Engine.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [NonParallelizable]
    public class StructureAttachmentTest
    {
        private sealed class Item : ISearchable { public Vector3 Position => Vector3.Zero; }
        private sealed class Watcher : Hedra.Engine.StructureSystem.ChunkWatcher<Item>
        {
            public readonly List<Item> Items = new();
            public int Reads;
            public int Adds;
            protected override Item[] Get() { Reads++; return Items.ToArray(); }
            protected override void Add(Chunk Object, Item Value) => Adds++;
            protected override void Delete(Chunk Object, Item Value) { }
        }

        [Test]
        public void IncrementalAttachmentsImportOldObjectsOnceAndDoNotDuplicateThem()
        {
            var previous = World.Provider;
            try
            {
                var provider = new WorldProvider();
                World.Provider = provider;
                var chunk = (Chunk)FormatterServices.GetUninitializedObject(typeof(Chunk));
                provider.SearcheableChunks.Add(Vector2.Zero, chunk);
                using var watcher = new Watcher();
                watcher.Items.Add(new Item());
                for (var i = 0; i < 100; i++)
                {
                    var item = new Item();
                    watcher.Items.Add(item);
                    watcher.OnValueAdded(chunk, item);
                }
                Assert.AreEqual(1, watcher.Reads);
                Assert.AreEqual(101, watcher.Adds);
                watcher.OnChunkReady(chunk);
                Assert.AreEqual(101, watcher.Adds);
                var replacement = (Chunk)FormatterServices.GetUninitializedObject(typeof(Chunk));
                watcher.OnChunkReady(replacement);
                Assert.AreEqual(202, watcher.Adds);
            }
            finally { World.Provider = previous; }
        }
    }
}
