using System;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class BanditCamp : BaseStructure, IUpdatable
    {
        private readonly int _passedTime = 0;
        private bool _restoreSoundPlayed;
        private Humanoid _rescuee;
        public Entity[] Enemies { get; set; }
        public bool Cleared { get; private set; }
        public bool Rescued { get; private set; }
        public float Radius { get; set; }
        private readonly Campfire _campfire;
        private bool _shouldRescue;
        private bool _canRescue;

        public BanditCamp(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
            this.BuildRescuee(new Random((int)(Position.X / 11 * (Position.Z / 13))));
            this._campfire = new Campfire(Position);
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs EventArgs)
            {
                _shouldRescue = EventArgs.Key == Controls.Interact && _canRescue;
            });
            AddChildren(_campfire);
            UpdateManager.Add(this);
        }

        public void Update()
        {

            if (Enemies != null)
            {
                var allDead = true;
                for (var i = 0; i < Enemies.Length; i++)
                {
                    if (Enemies[i] != null && !Enemies[i].IsDead && (Enemies[i].BlockPosition - Position).Xz.LengthSquared
                        < Radius * Radius * .9f * .9f)
                    {
                        allDead = false;
                    }
                }

                this.Cleared = allDead;
            }
            _campfire.Position = Position.Xz.ToVector3() + Vector3.UnitY * Physics.HeightAtPosition(this.Position);
            this.ManageOldMan();
        }

        private void BuildRescuee(Random Rng)
        {
            var randomTypes = new[]
            {
                HumanType.Warrior, HumanType.Archer, HumanType.Blacksmith, HumanType.Mage, HumanType.TravellingMerchant
            };
            _rescuee = World.WorldBuilding.SpawnHumanoid(randomTypes[Rng.Next(0, randomTypes.Length)],
                this.Position + Vector3.UnitY * 7f + Vector3.UnitZ * 3.0f);
            _rescuee.ResetEquipment();
            _rescuee.Physics.UsePhysics = false;
            _rescuee.Physics.CollidesWithEntities = false;
            _rescuee.Physics.CollidesWithStructures = false;
            _rescuee.MainWeapon = null;

            _rescuee.Name = Rng.Next(0, 10) == 1 ? "Deckard Cain" : NameGenerator.PickMaleName(Rng);
            var trade = _rescuee.SearchComponent<TradeComponent>();
            if(trade != null) _rescuee.RemoveComponent(trade);
            _rescuee.RemoveComponent(_rescuee.SearchComponent<HealthBarComponent>());
            _rescuee.AddComponent(new HealthBarComponent(_rescuee, _rescuee.Name, HealthBarType.Friendly));
            _rescuee.SearchComponent<DamageComponent>().Immune = true;
            _rescuee.IsTied = true;
        }

        private Matrix4 BuildRescueeMatrix()
        {
            return Matrix4.CreateRotationX(-90f * Mathf.Radian)
                 * Matrix4.CreateTranslation(-_rescuee.Position) 
                 * Matrix4.CreateTranslation( Vector3.UnitZ * 3f + Vector3.UnitY * 6f)
                 * Matrix4.CreateTranslation(_rescuee.Position);
        }

        private void ManageOldMan()
        {
            if (Rescued) return;

            _rescuee.Model.TransformationMatrix = this.BuildRescueeMatrix();
            _rescuee.Physics.TargetPosition = this.Position.Xz.ToVector3() + Vector3.UnitY * Physics.HeightAtPosition(this.Position);

            if (!_rescuee.InUpdateRange)
                _rescuee.Update();

            if (!Cleared || !((GameManager.Player.Position - Position).LengthSquared < 16 * 16))
            {
                _canRescue = false;
                return;
            }

            GameManager.Player.MessageDispatcher.ShowMessage(
                Translations.Get("press_to_untie", Controls.Interact.ToString().ToUpperInvariant()),
                .25f
            );
            _canRescue = true;

            if (!_shouldRescue) return;

            _rescuee.IsTied = false;
            Rescued = true;
            TaskScheduler.DelayFrames(1, delegate
            {
                var talkComponent = new TalkComponent(_rescuee, Translation.Create("saved_old_man"));
                talkComponent.OnTalkingEnded += delegate
                {
                    var settings = new ItemPoolSettings(ItemTier.Rare);
                    _rescuee.Movement.Orientate();
                    TaskScheduler.After(.05f, () =>
                        World.DropItem(ItemPool.Grab(settings), _rescuee.Position + Vector3.UnitX * 5f)
                    );
                    _rescuee.RemoveComponent(talkComponent);
                };
                _rescuee.AddComponent(talkComponent);
            });
            _rescuee.Model.TransformationMatrix = Matrix4.Identity;
            _rescuee.Physics.UsePhysics = true;
            _rescuee.Physics.CollidesWithEntities = true;
            _rescuee.Physics.CollidesWithStructures = true;
            _rescuee.BlockPosition = new Vector3(this.Position.X + 8f, Physics.HeightAtPosition(this.Position) / Chunk.BlockSize, this.Position.Z);
            _rescuee.Rotation = Vector3.Zero;
        }

        public override void Dispose()
        {
            if (Enemies != null)
            {
                for (var i = 0; i < Enemies.Length; i++)
                {
                    Enemies[i].Dispose();
                }
            }
            EventDispatcher.UnregisterKeyDown(this);
            _rescuee?.Dispose();
            this._campfire.Dispose();
            UpdateManager.Remove(this);
        }
    }
}
