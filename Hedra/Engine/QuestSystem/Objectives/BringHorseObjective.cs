/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/01/2017
 * Time: 06:04 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Objectives
{
    /// <inheritdoc />
    /// <summary>
    ///     Can only be called from a village.
    /// </summary>
    public class BringHorseObjective : Objective
    {
        private Entity _horse;
        private RideComponent _horseComponent;
        private ObjectMesh _previewMesh;
        private readonly Vector3 _stablePosition;
        private bool _stoleHorse;
        private Entity _horseRider;
        private bool _runningAway;
        private readonly List<Entity> _toRemove = new List<Entity>();

        public BringHorseObjective(Vector3 StablePosition)
        {
            _stablePosition = StablePosition;
            base.CenterHeight = 0;

            var underChunk = World.GetChunkAt(StablePosition.Xz.ToVector3());
            var region = underChunk != null
                ? underChunk.Biome
                : World.BiomePool.GetRegion(StablePosition.Xz.ToVector3());

            BlockType type;
            if(region.Generation.GetHeight(ObjectivePosition.X, ObjectivePosition.Z, null, out type) > 5)
                base.CenterRadius = 0;
        }

        public override string Description => _stoleHorse
            ? "Take the horse back to the stable."
            : _runningAway ? "The bandit is running away, catch him before it's too late!" :
            "A bandit has stolen the horse I use for my town trips. Please help me get him back.";

        public override Vector3 IconPosition => !_stoleHorse ?  _runningAway ? _horseRider.Position : _horse.Position  : _stablePosition;

        public override uint QuestLogIcon
        {
            get
            {
                if (_stoleHorse)
                {
                    //GameManager.Player.UI.DrawPreview(_previewMesh, UserInterface.QuestFbo);
                    return UserInterface.QuestFbo.TextureID[0];
                }
                if (!_runningAway)
                {
                   /* var model = _horse.Model as QuadrupedModel;
                    if(model == null) return UserInterface.QuestFbo.TextureID[0];
                    model.Model.Scale *= 2f;

                    //GameManager.Player.UI.DrawPreview(_horse.Model, UserInterface.QuestFbo);

                    model.Model.Scale /= 2f;
                    */
                }
                else
                {
                    var model = _horseRider.Model as HumanoidModel;
                    if (model == null) return UserInterface.QuestFbo.TextureID[0];
                    model.Model.Scale *= 2f;

                    //GameManager.Player.UI.DrawPreview(_horseRider.Model, UserInterface.QuestFbo);

                    model.Model.Scale /= 2f;
                }
                return UserInterface.QuestFbo.TextureID[0];
            }
        }

        public override bool ShouldDisplay =>
            (GameManager.Player.Position.Xz - ObjectivePosition.Xz).LengthSquared > 16 * 16;

        public override void SetOutObjectives()
        {
            AvailableOuts.Add(new VillageObjective());
        }

        public override void Setup(Chunk UnderChunk){}

        public override void Recreate()
        {
            this.SetQuestParams();
            this.RunCoroutine();
            CoroutineManager.StartCoroutine(this.Update);

            var rng = new Random(World.Seed + 2314);

            _horse = World.SpawnMob(MobType.Horse, ObjectivePosition, rng);
            _horse.BlockPosition = ObjectivePosition;
            _horse.Model.Position = ObjectivePosition;
            _horse.Physics.CanCollide = false;
            //(_horse.Model as QuadrupedModel).IsMountable = true;

            World.RemoveEntity(_horse);

            int banditCount = rng.Next(2, 6);
            for (var i = 0; i < banditCount; i++)
            {
                var offset = new Vector3(rng.NextFloat() * 200f - 100f, 0, rng.NextFloat() * 200f - 100f);
                Humanoid villager = World.QuestManager.SpawnHumanoid(HumanType.Villager, ObjectivePosition + offset);
                villager.Physics.CanCollide = false;
                _toRemove.Add(villager);
            }

            _horseRider = _toRemove[0];

            _horseComponent = _horse.SearchComponent<RideComponent>();
            _horseComponent.Ride(_horseRider as Humanoid);
            _toRemove.Add(_horse);
        }

        public IEnumerator Update()
        {
            while (!Disposed)
            {
                _horse?.Update();

                if (LocalPlayer.Instance.IsRiding && (_horseRider.Position - LocalPlayer.Instance.Position).LengthSquared < 72 * 72 && !_runningAway)
                {
                    _runningAway = true;
                    if(LocalPlayer.Instance.QuestLog.Show)
                        LocalPlayer.Instance.QuestLog.UpdateText();
                    LocalPlayer.Instance.MessageDispatcher.ShowMessage("[T] TO OPEN THE QUEST LOG", 3f);
                    _horseRider.RemoveComponent(_horseRider.SearchComponent<WarriorAIComponent>());
                    _horseRider.RemoveComponent(_horseRider.SearchComponent<ArcherAIComponent>());
                    _horseRider.AddComponent(new EscapeAIComponent(_horseRider, LocalPlayer.Instance));
                }

                if (_horseComponent.Rider == LocalPlayer.Instance && !_stoleHorse)
                {
                    _stoleHorse = true;
                    if (LocalPlayer.Instance.QuestLog.Show)
                        LocalPlayer.Instance.QuestLog.UpdateText();
                    //_previewMesh = VillageGenerator.GenerateStableIcon(Vector3.One * .35f, _stablePosition);
                }

                if (_horse.IsDead)
                {
                    GameManager.Player.CanInteract = false;
                    LocalPlayer.Instance.MessageDispatcher.ShowNotification("YOU KILLED THE HORSE", Color.DarkRed, 3f);

                    IsLost = true;
                    yield break;
                }

                if ((_horse.Model.Position - _stablePosition).Xz.LengthSquared < 48 * 48 && _stoleHorse)
                {
                    _horseComponent.Rider.IsRiding = false;
                    _horseComponent.UnRidable = true;
                    LocalPlayer.Instance.Model.Idle();//the animation was mounting so set it back to idle
                    TaskManager.After(1500, () => _horse.RemoveComponent(_horseComponent));
                    this.NextObjective();
                }

                yield return null;
            }
        }

        public override void Dispose()
        {
            foreach (Entity entity in _toRemove)
                entity.Dispose();
            Disposed = true;
        }
    }
}