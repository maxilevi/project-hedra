using System;
using System.Linq;
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
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class BanditCamp : BaseStructure, IUpdatable, ICompletableStructure
    {
        private readonly int _passedTime = 0;
        private bool _restoreSoundPlayed;
        public Humanoid Rescuee { get; private set; }
        public Entity[] Enemies { get; set; }
        public bool Cleared { get; private set; }
        public bool Completed => _shouldUntie;
        public float Radius { get; set; }
        public int EnemiesLeft => Enemies.Count(E => !E.IsDead);
        private readonly Campfire _campfire;
        private readonly Item _item;
        private bool _shouldUntie;
        private bool _canRescue;

        public BanditCamp(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
            this.BuildRescuee(new Random((int)(Position.X / 11 * (Position.Z / 13))));
            this._campfire = new Campfire(Position);
            _item = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare));
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs EventArgs)
            {
                _shouldUntie = EventArgs.Key == Controls.Interact && _canRescue;
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
            Rescuee = World.WorldBuilding.SpawnHumanoid(randomTypes[Rng.Next(0, randomTypes.Length)],
                this.Position + Vector3.UnitY * 7f + Vector3.UnitZ * 3.0f);
            Rescuee.ResetEquipment();
            Rescuee.Physics.UsePhysics = false;
            Rescuee.Physics.CollidesWithEntities = false;
            Rescuee.Physics.CollidesWithStructures = false;
            Rescuee.MainWeapon = null;

            Rescuee.Name = Rng.Next(0, 10) == 1 ? "Deckard Cain" : NameGenerator.PickMaleName(Rng);
            var trade = Rescuee.SearchComponent<TradeComponent>();
            if(trade != null) Rescuee.RemoveComponent(trade);
            Rescuee.RemoveComponent(Rescuee.SearchComponent<HealthBarComponent>());
            Rescuee.AddComponent(new HealthBarComponent(Rescuee, Rescuee.Name, HealthBarType.Friendly));
            Rescuee.SearchComponent<DamageComponent>().Immune = true;
            Rescuee.IsTied = true;
        }

        private Matrix4 BuildRescueeMatrix()
        {
            return Matrix4.CreateRotationX(-90f * Mathf.Radian)
                 * Matrix4.CreateTranslation(-Rescuee.Position) 
                 * Matrix4.CreateTranslation( Vector3.UnitZ * 3f + Vector3.UnitY * 7f)
                 * Matrix4.CreateTranslation(Rescuee.Position);
        }

        private void ManageOldMan()
        {
            if (Completed) return;

            Rescuee.Model.TransformationMatrix = this.BuildRescueeMatrix();
            Rescuee.Physics.TargetPosition = this.Position.Xz.ToVector3() + Vector3.UnitY * Physics.HeightAtPosition(this.Position);

            if (!Rescuee.InUpdateRange)
                Rescuee.Update();

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

            if (!_shouldUntie) return;

            Rescuee.IsTied = false;
            TaskScheduler.DelayFrames(1, delegate
            {
                var talkComponent = new TalkComponent(Rescuee, Translation.Create("saved_old_man"));
                talkComponent.OnTalkingEnded += delegate
                {
                    Rescuee.Movement.Orientate();
                    TaskScheduler.After(.05f, () =>
                        World.DropItem(_item, Rescuee.Position + Vector3.UnitX * 5f)
                    );
                    Rescuee.RemoveComponent(talkComponent);
                };
                Rescuee.AddComponent(talkComponent);
            });
            Rescuee.Model.TransformationMatrix = Matrix4.Identity;
            Rescuee.Physics.UsePhysics = true;
            Rescuee.Physics.CollidesWithEntities = true;
            Rescuee.Physics.CollidesWithStructures = true;
            Rescuee.BlockPosition = new Vector3(this.Position.X + 8f, Physics.HeightAtPosition(this.Position) / Chunk.BlockSize, this.Position.Z);
            Rescuee.Rotation = Vector3.Zero;
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
            Rescuee?.Dispose();
            this._campfire.Dispose();
            UpdateManager.Remove(this);
        }

        public ItemDescription DeliveryItem => ItemDescription.FromItem(_item, Translations.Get("quest_pickup_get_item_description", _item.DisplayName, Rescuee.Name));
        public QuestReward Reward => null;
    }
}
