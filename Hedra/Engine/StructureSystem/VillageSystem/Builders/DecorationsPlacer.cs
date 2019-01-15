using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class DecorationsPlacer
    {
        public static void PlaceLamp(Vector3 TargetPosition, CollidableStructure Structure,
            VillageRoot Root, float Radius, Random Rng)
        {
            void Place(Vector3 Position)
            {
                var template = SelectTemplate(Root.Template.Decorations.Lamps.ToArray(), Rng);
                var lampPost = Root.Cache.GrabModel(template.Path);
                lampPost.Translate(Position);
                var shapes = Root.Cache.GrabShapes(template.Path).Select(S => S.Transform(Position)).ToList();
                Structure.AddCollisionShape(shapes.ToArray());
                Structure.AddStaticElement(lampPost);
                Structure.WorldObject.AddChildren(new WorldLight(Position + template.LightOffset * template.Scale)
                {
                    Radius = Radius,
                    LightColor = HandLamp.LightColor
                });
            }
            CoroutineManager.StartCoroutine(DoPlace, TargetPosition, (Action<Vector3>) Place);
        }
        
        public static void PlaceBench(Vector3 TargetPosition, bool IsInIntersection, 
            Vector3 Orientation, CollidableStructure Structure, VillageRoot Root, Random Rng)
        {
            void Place(Vector3 Position)
            {
                var rotation = Vector3.UnitY * (IsInIntersection ? 90 : 0);
                var rotationMatrix = Matrix4.CreateRotationY(rotation.Y * Mathf.Radian);
                var offset = Orientation * VillageDesign.PathWidth * .75f;
                var template = SelectTemplate(Root.Template.Decorations.Benches.ToArray(), Rng);
                var transMatrix = rotationMatrix * Matrix4.CreateTranslation(Position + offset);
                var bench = Root.Cache.GrabModel(template.Path).Transform(transMatrix);
                var shapes = Root.Cache.GrabShapes(template.Path).Select(S => S.Transform(transMatrix)).ToList();
                Structure.AddCollisionShape(shapes.ToArray());
                Structure.AddStaticElement(bench);
                Structure.WorldObject.AddChildren(
                    new Bench(
                        Vector3.TransformPosition(Vector3.Zero, transMatrix),
                        Physics.DirectionToEuler(-Orientation),
                        template.SitOffset * template.Scale
                    )
                );
            }
            CoroutineManager.StartCoroutine(DoPlace, TargetPosition, (Action<Vector3>) Place);
        }

        private static T SelectTemplate<T>(IList<T> Templates, Random Rng) where T : DesignTemplate
        {
            return Templates[Rng.Next(0, Templates.Count)];
        }
        
        private static IEnumerator DoPlace(object[] Params)
        {
            var position = (Vector3) Params[0];
            var lambda = (Action<Vector3>) Params[1];
            var waiter = new WaitForChunk(position);
            
            while (waiter.MoveNext()) yield return null;            
            if (waiter.Disposed) yield break;

            lambda(new Vector3(position.X, Physics.HeightAtPosition(position), position.Z));
        }       
    }
}