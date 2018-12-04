using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class StructureWatcher : IDisposable
    {
        public CollidableStructure Structure { get; }
        private readonly object _lock = new object();
        private readonly Dictionary<CachedVertexData, Chunk> _modelsAdded;
        private readonly Dictionary<InstanceData, Chunk> _instancesAdded;

        public StructureWatcher(CollidableStructure Structure)
        {
            this.Structure = Structure;
            _modelsAdded = new Dictionary<CachedVertexData, Chunk>();
            _instancesAdded = new Dictionary<InstanceData, Chunk>();
            Structure.ModelAdded += this.ModelAdded;
            Structure.InstanceAdded += this.InstanceAdded;
            World.OnChunkReady += this.OnChunkReady;
            World.OnChunkDisposed += this.OnChunkDisposed;
        }

        private void OnChunkReady(Chunk Object)
        {
            var models = Structure.Models;
            var instances = Structure.Instances;

            bool IsInRange(Vector3 Position)
            {
                var chunkSpace = World.ToChunkSpace(Position);
                return (Object.OffsetX == (int) chunkSpace.X && Object.OffsetZ == (int) chunkSpace.Y);
            }
            
            for (var i = 0; i < models.Length; i++)
            {
                if(IsInRange(models[i].Position))
                    AddModelIfNecessary(models[i], Object);          
            }
            for (var i = 0; i < instances.Length; i++)
            {
                if(IsInRange(instances[i].Position))
                    AddInstanceIfNecessary(instances[i], Object);    
            }
        }
        
        private void InstanceAdded(InstanceData Instance)
        {
            var chunkSpace = World.ToChunkSpace(Instance.Position);
            var chunk = World.GetChunkByOffset(chunkSpace);
            if (chunk != null)
            {
                AddInstanceIfNecessary(Instance, chunk);
            }    
        }

        private void ModelAdded(CachedVertexData Model)
        {
            var chunkSpace = World.ToChunkSpace(Model.Position);
            var chunk = World.GetChunkByOffset(chunkSpace);
            if (chunk != null)
            {
                AddModelIfNecessary(Model, chunk);
            }    
        }

        private void AddModelIfNecessary(CachedVertexData Model, Chunk Object)
        {
            AddIfNecessary(_modelsAdded, Model, Object, V => Object.AddStaticElement(V.VertexData));
        }
        
        private void AddInstanceIfNecessary(InstanceData Instance, Chunk Object)
        {
            AddIfNecessary(_instancesAdded, Instance, Object, I => Object.AddInstance(I));
        }

        private void AddIfNecessary<T>(Dictionary<T, Chunk> Map, T Value, Chunk Object, Action<T> Do)
        {
            lock (_lock)
            {
                if (!Object.Disposed && Object.Initialized && (!Map.ContainsKey(Value) || Map[Value] != Object))
                {
                    Do(Value);
                    if (Map.ContainsKey(Value))
                        Map[Value] = Object;
                    else
                        Map.Add(Value, Object);
                }
            }
        }

        private void OnChunkDisposed(Chunk Object)
        {
            KeyValuePair<CachedVertexData, Chunk>[] modelDict;
            KeyValuePair<InstanceData, Chunk>[] instanceDict;
            lock (_lock)
            {
                modelDict = _modelsAdded.ToArray();
                instanceDict = _instancesAdded.ToArray();
            }
            foreach (var pair in modelDict)
            {
                if (pair.Value == Object)
                    _modelsAdded[pair.Key] = null;
            }
            foreach (var pair in instanceDict)
            {
                if (pair.Value == Object)
                    _instancesAdded[pair.Key] = null;
            }   
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach(var pair in _modelsAdded)
                {
                    pair.Value?.RemoveStaticElement(pair.Key.VertexData);
                }
                _modelsAdded.Clear();
                foreach(var pair in _instancesAdded)
                {
                    pair.Value?.RemoveInstance(pair.Key);
                }
                _instancesAdded.Clear();
            }
            Structure.Dispose();
            Structure.ModelAdded -= this.ModelAdded;
            World.OnChunkReady -= this.OnChunkReady;
            World.OnChunkDisposed -= this.OnChunkDisposed;
        }
    }
}