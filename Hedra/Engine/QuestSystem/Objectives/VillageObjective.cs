/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/12/2016
 * Time: 02:21 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Objectives
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of SaveVillageObjective.
    /// </summary>
    public class VillageObjective : Objective
    {

        private readonly ObjectMesh _iconMesh;
        private readonly float _scale = 3.5f;
        private int _seed;
        private bool _welcomeMsgShown;
        public Entity Escortee;

        public override string Description
        {
            get
            {
                if (Escortee == null)
                    return "Find and go to the town of " +
                           VillageGenerator.GenerateVillageName(World.Seed) + " located in the "
                           + ObjectiveDirection.ToLowerInvariant() + " of the map.";
                return "Escort " + Escortee.Name + " to the town of " +
                       VillageGenerator.GenerateVillageName(World.Seed) + " located in the "
                       + ObjectiveDirection.ToLowerInvariant() + " of the map.";
            }
        }

        public override uint QuestLogIcon
        {
            get
            {
                SceneManager.Game.Player.UI.DrawPreview(_iconMesh, UserInterface.QuestFbo);
                return UserInterface.QuestFbo.TextureID[0];
            }
        }

        public override bool ShouldDisplay => true;

        public VillageObjective() : this(null){}
        public VillageObjective(Entity Escortee)
        {
            //base.CenterRadius = (int) (StructureGenerator.CityRadius * 1.0f);
            this.Escortee = Escortee;
            _iconMesh = VillageGenerator.GenerateVillageHouseIcon(Vector3.One * .65f);
        }

        public override void Recreate()
        {
            base.Recreate();

            var rng = new Random(World.Seed+ 782423);
            //World.StructureGenerator.SetupTown(rng, ObjectivePosition.Xz, true);
        }

        public override void SetOutObjectives()
        {
            
        }

        public override void SetPartialObjectives()
        {
            var rng = new Random(World.Seed + 12412);
            string name = NameGenerator.Generate(rng.Next(0, 9999999));
            var addon = new Vector3(rng.NextFloat() * 256f - 128f, 0, rng.NextFloat() * 256f - 128f);

            Humanoid villager = World.QuestManager.SpawnVillager(ObjectivePosition + addon, true, name);
            villager.RemoveComponent(villager.SearchComponent<VillagerAIComponent>());
            AvailablePartials.Add(new TalkObjective(villager));
        }

        public override void Setup(Chunk UnderChunk)
        {
            Position = new Vector3(ObjectivePosition.X, Physics.HeightAtPosition( ObjectivePosition.X, ObjectivePosition.Z), ObjectivePosition.Z);
            CoroutineManager.StartCoroutine(Update);
        }

        private IEnumerator Update()
        {
            while (!Disposed)
            {
                if (Escortee != null && (Escortee.Position - Position).LengthSquared < 48 * 48)
                {
                    Escortee.RemoveComponent(Escortee.SearchComponent<FollowAIComponent>());
                    Escortee.Model.Idle();
                    this.NextObjective();
                    break;
                }
                if (Escortee == null && (SceneManager.Game.Player.Position - Position).LengthSquared < 64 * 64)
                {
                    this.NextObjective();
                    break;
                }
                if (!_welcomeMsgShown && (SceneManager.Game.Player.Position - Position).LengthSquared < 192 * 192)
                {
                    LocalPlayer.Instance.MessageDispatcher.ShowTitleMessage(
                        "Welcome to " + VillageGenerator.GenerateVillageName(World.Seed), 4f);
                    _welcomeMsgShown = true;
                }

                if (Escortee != null && Escortee.IsDead)
                {
                    IsLost = true;
                    break;
                }
                yield return null;
            }
        }

        public override void Dispose()
        {
            Disposed = true;
            _iconMesh?.Dispose();
        }
    }
}