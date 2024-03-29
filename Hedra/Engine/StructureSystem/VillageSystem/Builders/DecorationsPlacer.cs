using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Numerics;

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
                Structure.AddStaticElement(lampPost, false);
                Structure.WorldObject.AddChildren(new WorldLight(Position + template.LightOffset * template.Scale)
                {
                    Radius = Radius,
                    LightColor = WorldLight.DefaultColor
                });
            }

            PlaceWhenWorldReady(TargetPosition, Place, () => Structure.Disposed);
        }

        public static void PlaceBench(Vector3 TargetPosition, bool IsInIntersection,
            Vector3 Orientation, CollidableStructure Structure, VillageRoot Root, Random Rng)
        {
            var template = SelectTemplate(Root.Template.Decorations.Benches.ToArray(), Rng);
            var bench = Root.Cache.GrabModel(template.Path);
            var shapes = Root.Cache.GrabShapes(template.Path);

            void Place(Vector3 Position)
            {
                var rotation = Vector3.UnitY * (IsInIntersection ? 90 : 0);
                var rotationMatrix = Matrix4x4.CreateRotationY(rotation.Y * Mathf.Radian);
                var offset = Orientation * VillageDesign.PathWidth * .75f;
                var transMatrix = rotationMatrix * Matrix4x4.CreateTranslation(Position + offset);
                Structure.AddCollisionShape(shapes.Select(S => S.Transform(transMatrix)).ToArray());
                Structure.AddStaticElement(bench.Transform(transMatrix), false);
                Structure.WorldObject.AddChildren(
                    new Bench(
                        Vector3.Transform(Vector3.Zero, transMatrix),
                        Physics.DirectionToEuler(-Orientation),
                        template.SitOffset * template.Scale
                    )
                );
            }

            PlaceWhenWorldReady(TargetPosition, Place, () => Structure.Disposed);
        }

        public static void PlaceWhenWorldReady(Vector3 TargetPosition, Action<Vector3> Place, Func<bool> ShouldDispose)
        {
            if (GameSettings.TestingMode)
                Place(TargetPosition);
            else
                RoutineManager.StartRoutine(DoPlace, TargetPosition, Place, ShouldDispose);
        }

        private static T SelectTemplate<T>(IList<T> Templates, Random Rng) where T : DesignTemplate
        {
            return Templates[Rng.Next(0, Templates.Count)];
        }

        private static IEnumerator DoPlace(object[] Params)
        {
            var position = (Vector3)Params[0];
            var lambda = (Action<Vector3>)Params[1];
            var shouldDispose = (Func<bool>)Params[2];
            var waiter = new WaitForChunk(position)
            {
                DisposeCondition = shouldDispose
            };

            while (waiter.MoveNext())
            {
                if (waiter.Disposed) yield break;
                yield return null;
            }

            lambda(new Vector3(position.X, Physics.HeightAtPosition(position), position.Z));
        }
    }
}