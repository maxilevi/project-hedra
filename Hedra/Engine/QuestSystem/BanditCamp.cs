using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.QuestSystem
{
    internal class BanditCamp : BaseStructure, IUpdatable
    {
        private readonly int _passedTime = 0;
        private bool _restoreSoundPlayed;
        private Humanoid _rescuee;
        public Entity[] Enemies;
        public bool Cleared { get; private set; }
        public bool Rescued { get; private set; }
        public float Radius { get; set; }
        private readonly Campfire _campfire;
        private bool _shouldRescue;
        private bool _canRescue;

        public BanditCamp(Vector3 Position, float Radius)
        {
            this.Position = Position;
            this.Radius = Radius;
            this.BuildRescuee(new Random((int)(Position.X / 11 * (Position.Z / 13))));
            this._campfire = new Campfire(Position);
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs EventArgs)
            {
                _shouldRescue = EventArgs.Key == Key.E && _canRescue;
            });
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

            this.ManageOldMan();
        }

        private void BuildRescuee(Random Rng)
        {
            var randomTypes = new[]
            {
                HumanType.Warrior, HumanType.Archer, HumanType.Blacksmith, HumanType.Mage, HumanType.TravellingMerchant
            };
            _rescuee = World.QuestManager.SpawnHumanoid(randomTypes[Rng.Next(0, randomTypes.Length)],
                this.Position + Vector3.UnitY * 7f + Vector3.UnitZ * 3.0f);
            _rescuee.Physics.UsePhysics = false;
            _rescuee.Physics.HasCollision = false;
            _rescuee.Physics.CanCollide = false;
            _rescuee.MainWeapon = null;

            _rescuee.Name = Rng.Next(0, 10) == 1 ? "Deckard Cain" : NameGenerator.PickMaleName(Rng);
            _rescuee.RemoveComponent(_rescuee.SearchComponent<HealthBarComponent>());
            _rescuee.AddComponent(new HealthBarComponent(_rescuee, _rescuee.Name));
            _rescuee.SearchComponent<DamageComponent>().Immune = true;
            World.AddEntity(_rescuee);
        }

        private Matrix4 BuildRescueeMatrix()
        {
            return Matrix4.CreateRotationX(-90f * Mathf.Radian)
                 * Matrix4.CreateTranslation(-_rescuee.Position) 
                 * Matrix4.CreateTranslation( Vector3.UnitZ * 3f + Vector3.UnitY * 7f)
                 * Matrix4.CreateTranslation(_rescuee.Position);
        }

        private void ManageOldMan()
        {
            if (Rescued) return;

            _rescuee.Model.Model.TransformationMatrix = this.BuildRescueeMatrix();
            _rescuee.BlockPosition = this.Position.Xz.ToVector3() + Vector3.UnitY * Physics.HeightAtPosition(this.Position);
            _rescuee.Model.Position = _rescuee.BlockPosition;
            _rescuee.Model.Tied();

            if (!_rescuee.InUpdateRange)
                _rescuee.Update();

            if (!Cleared || !((GameManager.Player.Position - Position).LengthSquared < 16 * 16))
            {
                _canRescue = false;
                return;
            }

            GameManager.Player.MessageDispatcher.ShowMessage("PRESS [E] TO UNTIE", .25f);
            _canRescue = true;

            if (!_shouldRescue) return;

            Rescued = true;
            TaskManager.Delay(1, delegate
            {
                var talkComponent = new TalkComponent(_rescuee,
                    "I am grateful to you for saving me. Take this item as a show of gratitude");
                talkComponent.OnTalk += delegate(Entity Talkee)
                {
                    var settings = new ItemPoolSettings(ItemTier.Rare, EquipmentType.Axe);
                    _rescuee.Movement.Orientate();
                    TaskManager.After(1000, () =>
                        World.DropItem(ItemPool.Grab(settings), _rescuee.Position + Vector3.UnitX * 5f)
                    );
                    //TaskManager.After( (int) ((talkComponent.Duration+1) * 1000), () =>
                    //    Talkee.AddComponent(new EscapeAIComponent(_rescuee, GameManager.Player))
                    //);
                };
                _rescuee.AddComponent(talkComponent);
            });
            _rescuee.Model.Model.TransformationMatrix = Matrix4.Identity;
            _rescuee.Physics.UsePhysics = true;
            _rescuee.Physics.HasCollision = true;
            _rescuee.Physics.CanCollide = true;
            _rescuee.BlockPosition = new Vector3(this.Position.X + 8f, Physics.HeightAtPosition(this.Position) / Chunk.BlockSize, this.Position.Z);
            _rescuee.Rotation = Vector3.Zero;
            _rescuee.Model.Idle();
        }

        public override void Dispose()
        {
            this._campfire.Dispose();
            UpdateManager.Remove(this);
        }
    }
}
