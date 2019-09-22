using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using System;
using System.Collections;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class CompressedBuildingOutput : IDisposable
    {
        public List<CompressedVertexData> Models { get; set; }
        public List<InstanceData> Instances { get; set; }
        public List<CollisionShape> Shapes { get; set; }
        public List<BaseStructure> Structures { get; set; }

        public void Dispose()
        {
            for (var i = 0; i < Structures.Count; i++)
                Structures[i].Dispose();
        }
      
        public IEnumerator Place(Vector3 Position)
        {
            var height = Physics.HeightAtPosition(Position);
            Structures.ForEach(S => S.Position += Vector3.UnitY * height);
            var transMatrix = Matrix4.CreateTranslation(Vector3.UnitY * height);
            Shapes.ForEach(S => S.Transform(transMatrix));
            Models.ForEach(M => M.Transform(transMatrix));
            for (var i = Instances.Count - 1; i > -1; --i)
            {
                var position = Instances[i].Position;
                var waiter = new WaitForChunk(position);
                while (waiter.MoveNext())
                {
                    if (waiter.Disposed) yield break;
                    yield return null;
                }
                
                var block = World.GetHighestBlockAt(position.X, position.Z);
                if (Instances[i].PlaceCondition == null || Instances[i].PlaceCondition(block.Type))
                {
                    var instanceHeight = Physics.HeightAtPosition(position);
                    Instances[i].Apply(Matrix4.CreateTranslation(Vector3.UnitY * instanceHeight));
                }
                else
                {
                    Instances.RemoveAt(i);
                }
            }
        }

        public bool IsEmpty => Models.Count == 0 && Instances.Count == 0 && Shapes.Count == 0 && Structures.Count == 0; 
    }
}