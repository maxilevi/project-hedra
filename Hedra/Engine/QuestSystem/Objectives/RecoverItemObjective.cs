/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/09/2016
 * Time: 03:09 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using OpenTK;
using Hedra.Engine.QuestSystem.Objectives;

namespace Hedra.Engine.QuestSystem.Objectives
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of RecoverItemObjective.
    /// </summary>
    public class RecoverItemObjective : Objective
    {
        private WorldItem _worldItem;
        private ObjectMesh _previewMesh;
        private TempleType _type;

        public override string Description
        {
            get
            {
                var rng = new Random(World.Seed + 2113);
                return "Recover the lost item from the " + NameGenerator.Generate(rng.Next(0, 9999999)) + " ruins.";
            }
        }

        public override uint QuestLogIcon
        {
            get
            {
                Vector3 oldScale = _previewMesh.Scale;
                _previewMesh.Scale *= .75f;
                if (_type == TempleType.GreekTemple)
                    _previewMesh.Scale *= .33f;
                //GameManager.Player.UI.DrawPreview(_previewMesh, UserInterface.QuestFbo);
                _previewMesh.Scale = oldScale;
                return UserInterface.QuestFbo.TextureID[0];
            }
        }

        public override bool ShouldDisplay => true;

        public RecoverItemObjective(TempleType Type)
        {
            _type = Type;
        }

        public override void SetOutObjectives()
        {
            AvailableOuts.Add(new VillageObjective());
        }

        public override void Recreate()
        {
            base.Recreate();

            int structureIndex;
            var buildingFile = string.Empty;
            var colliderCount = 0;
            var scale = 1f;
            Vector3 relativePosition = Vector3.Zero;

            if (_type == TempleType.RandomTemple)
            {
                var rng = new Random(World.Seed + 421242);
                _type = (TempleType) rng.Next(1, (int) TempleType.MaxEnums);
            }

            if (_type == TempleType.RuinsTemple)
            {
                buildingFile = "Assets/Env/Ruins1.ply";
                colliderCount = 12;
                scale = 1f;
            }

            else if (_type == TempleType.GreekTemple)
            {
                buildingFile = "Assets/Env/Temple0.ply";
                colliderCount = 25;
                scale = 1.5f;
                relativePosition = .5f * Vector3.UnitY;
            }

            Model = AssetManager.PlyLoader(buildingFile, Vector3.One * 8 * new Vector3(scale, scale + .05f, scale));
            Model.GraduateColor(Vector3.UnitY);
            Shapes = AssetManager.LoadCollisionShapes(buildingFile, colliderCount,
                Vector3.One * 8 * new Vector3(scale, scale + .05f, scale));


            VertexData previewData = AssetManager.PlyLoader(buildingFile, Vector3.One * .65f * scale, relativePosition,
                Vector3.Zero, true);
            previewData.GraduateColor(Vector3.UnitY);
            _previewMesh = ObjectMesh.FromVertexData(previewData);
        }

        public override void Setup(Chunk UnderChunk)
        {
            Vector3 spawnPosition = ObjectivePosition +
                                    Vector3.UnitY * Physics.HeightAtPosition(ObjectivePosition.X, ObjectivePosition.Z) -
                                    Vector3.UnitY * 125.5f;

            Model.Transform(spawnPosition);
            Model.VariateColors(10f / 255f, Utils.Rng);

            for (int i = Shapes.Count - 1; i >= 0; i--)
                Shapes[i].Transform(spawnPosition);

            var rng = new Random(World.Seed);

            var enemies = new List<Entity>();
            int count = rng.Next(2, 5);

            for (var i = 0; i < count; i++)
            {
                Entity mob = null;
                var addon = new Vector3((rng.NextFloat() * 48f - 24f) * Chunk.BlockSize, 0,
                    (rng.NextFloat() * 48f - 24f) * Chunk.BlockSize);

                mob = World.QuestManager.SpawnBandit(spawnPosition + addon, false, true);

                mob.Physics.HasFallDamage = false;
                mob.Physics.CanCollide = false;
                enemies.Add(mob);
            }
            Item item = ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon));
            Chest prizeChest =
                World.QuestManager.SpawnChest(
                    spawnPosition + Vector3.UnitY * 8f - Vector3.UnitX * .5f + Vector3.UnitZ * .5f, item);
            prizeChest.OnPickup += delegate { this.NextObjective(); };
            prizeChest.Condition = delegate
            {
                if (enemies.Any(t => t != null && !t.IsDead &&
                                     (t.Position.Xz - ObjectivePosition.Xz).LengthSquared < 48 * 48))
                {
                    LocalPlayer.Instance.MessageDispatcher.ShowNotification("THERE ARE STILL ENEMIES AROUND.", Color.DarkRed, 2f);
                    return false;
                }
                return true;
            };

            UnderChunk.AddStaticElement(Model);
            World.AddGlobalCollider(Shapes.ToArray());
        }

        public override void Dispose()
        {
            Disposed = true;
            if (Model != null)
                if (World.GetChunkAt(ObjectivePosition) != null)
                {
                    World.GetChunkAt(ObjectivePosition).RemoveStaticElement(Model);
                    for (int i = Shapes.Count - 1; i > -1; i--)
                    {
                        World.RemoveGlobalCollider(Shapes[i]);
                    }
                }
            _worldItem?.Dispose();
            _worldItem = null;
            _previewMesh?.Dispose();
            _previewMesh = null;
        }
    }
}