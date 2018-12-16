using System;
using System.Collections;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class DecorationsPlacer
    {
        public void PlaceLamp(Vector3 Position, CollidableStructure Structure, VillageRoot Root)
        {
            void Place()
            {
                /*
                VertexData lampPost = AssetManager.PLYLoader("Assets/Env/Lamp0.ply", Vector3.One * 3.25f * 1.5f);
                lampPost.Translate(lightPosition);
                lampPost.GraduateColor(Vector3.UnitY);
                lampPost.FillExtraData(WorldRenderer.NoHighlightFlag);
                List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Assets/Env/Lamp0.ply", 1, Vector3.One * 3.25f * 1.5f);
                for (int l = 0; l < shapes.Count; l++)
                {
                    shapes[l].Transform(lightPosition);
                }
                Structure.AddCollisionShape(shapes.ToArray());
                Structure.AddStaticElement(lampPost);
    */
                Structure.WorldObject.AddChildren(new LampPost(Position + Vector3.UnitY * 6)
                {
                    Radius = 180
                });
            }
            CoroutineManager.StartCoroutine(DoPlace, Position, (Action) Place);
        }
        
        public void PlaceBench(Vector3 Position, CollidableStructure Structure, VillageRoot Root)
        {
            void Place()
            {
                
            }
            CoroutineManager.StartCoroutine(DoPlace, Position, (Action) Place);
        }
        
        private static IEnumerator DoPlace(object[] Params)
        {
            var position = (Vector3) Params[0];
            var lambda = (Action) Params[1];
            var waiter = new WaitForChunk(position);
            
            while (waiter.MoveNext()) yield return null;            
            if (waiter.Disposed) yield break;

            lambda();
        }       
    }
}