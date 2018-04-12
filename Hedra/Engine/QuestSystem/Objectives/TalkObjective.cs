/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/12/2016
 * Time: 05:27 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.QuestSystem.Objectives
{
    /// <inheritdoc />
    /// <summary>
    ///     Creates
    /// </summary>
    public class TalkObjective : Objective
    {
        private readonly bool _logHouse;
        private readonly Entity _talkTarget;
        private Vector3 _stablePosition;
        private bool _isFollowing;
        private bool _talked;

        public override string Description
        {
            get
            {
                string name = _talkTarget.Name;
                if (name == "Villager")
                    name = NameGenerator.PickMaleName(new Random(World.Seed + 21412));
                return "Talk to " + name;
            }
        }

        public override Vector3 IconPosition => _talkTarget?.Position ?? base.IconPosition;

        public override uint QuestLogIcon
        {
            get
            {
                var model = _talkTarget.Model as HumanModel;
                if (model == null) return UserInterface.QuestFbo.TextureID[0];
                model.Model.Scale *= 2f;

                GameManager.Player.UI.DrawPreview(_talkTarget.Model, UserInterface.QuestFbo);

                model.Model.Scale /= 2f;
                return UserInterface.QuestFbo.TextureID[0];
            }
        }

        public override bool ShouldDisplay =>
            (GameManager.Player.Position - IconPosition).LengthSquared > 96 * 96;


        public TalkObjective(Entity TalkTarget)
        {
            _talkTarget = TalkTarget;    

            if (_talkTarget.SearchComponent<DamageComponent>() != null)
                _talkTarget.SearchComponent<DamageComponent>().Immune = true;

            _talkTarget.Physics.HasFallDamage = false;
        }

        public override void SetOutObjectives()
        {
            AvailableOuts.Add(new RecoverItemObjective(TempleType.RandomTemple));
            AvailableOuts.Add(new BossObjective());
            AvailableOuts.Add(new RescueHumanObjective());
            AvailableOuts.Add(new BringHorseObjective( _stablePosition ));
        }

        public override void Recreate()
        {
            base.Recreate();

            var bar = _talkTarget.SearchComponent<HealthBarComponent>();
            if (bar != null) bar.Hide = true;

            _talkTarget.AddComponent(new HeadIconComponent(_talkTarget));
            _talkTarget.SearchComponent<HeadIconComponent>().ShowIcon(CacheItem.AttentionIcon);
            _stablePosition = this.ObjectivePosition + VillageDesign.StablePosition;

            CoroutineManager.StartCoroutine(this.Update);
        }
 
        private IEnumerator Update()
        {
            while (!_talked)
            {
                if(!_talkTarget.InUpdateRange)
                    _talkTarget.Update();

                if (!_isFollowing && (GameManager.Player.Position - IconPosition).LengthSquared <
                    96 * 96)
                {
                    _talkTarget.AddComponent(new FollowAIComponent(_talkTarget, GameManager.Player));               
                    _isFollowing = true;
                }

                _talkTarget.Model.Tint = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

                if ((_talkTarget.Position - GameManager.Player.Position).LengthSquared < 10 * 10
                    && Vector3.Dot((_talkTarget.Position - GameManager.Player.Position).NormalizedFast(),
                        GameManager.Player.View.LookAtPoint.NormalizedFast()) > .6f)
                {

                    _talkTarget.Model.Tint = new Vector4(1.5f, 1.5f, 1.5f, 1.0f);
                    LocalPlayer.Instance.MessageDispatcher.ShowMessage("PRESS [E] TO INTERACT", .5f);
                    if (EventDispatcher.LastKeyDown == Key.E)
                        _talked = true;

                }
                yield return null;
            }
            _talkTarget.SearchComponent<HeadIconComponent>().Dispose();
            _talkTarget.RemoveComponent(_talkTarget.SearchComponent<HeadIconComponent>());
            _talkTarget.RemoveComponent(_talkTarget.SearchComponent<FollowAIComponent>());
            this.NextObjective();
            if(_talkTarget.SearchComponent<TalkComponent>() != null)
                _talkTarget.RemoveComponent(_talkTarget.SearchComponent<TalkComponent>());
            _talkTarget.AddComponent(new TalkComponent(_talkTarget, World.QuestManager.Quest.Description));
            _talkTarget.SearchComponent<TalkComponent>().Talk(true);
        }

        public override void Dispose()
        {
            Disposed = true;
        }

        public override void Setup(Chunk UnderChunk){}
    }
}