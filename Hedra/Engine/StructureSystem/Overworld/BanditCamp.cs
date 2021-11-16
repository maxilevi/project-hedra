using System;
using System.Linq;
using System.Numerics;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class BanditCamp : BaseStructure, IUpdatable, ICompletableStructure
    {
        private readonly Campfire _campfire;
        private readonly Item _item;
        private readonly int _passedTime = 0;
        private bool _canRescue;
        private bool _restoreSoundPlayed;
        private bool _shouldUntie;

        public BanditCamp(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
            BuildRescuee(new Random((int)(Position.X / 11 * (Position.Z / 13))));
            _campfire = new Campfire(Position);
            _item = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare));
            EventDispatcher.RegisterKeyDown(this,
                delegate(object Sender, KeyEventArgs EventArgs)
                {
                    _shouldUntie = EventArgs.Key == Controls.Interact && _canRescue;
                });
            AddChildren(_campfire);
            UpdateManager.Add(this);
        }

        public Humanoid Rescuee { get; private set; }
        public Entity[] Enemies { get; set; }
        public bool Cleared { get; private set; }
        public float Radius { get; set; }
        public int EnemiesLeft => Enemies.Count(E => !E.IsDead);
        public bool Completed => !Rescuee.IsTied;

        public override void Dispose()
        {
            if (Enemies != null)
                for (var i = 0; i < Enemies.Length; i++)
                    Enemies[i].Dispose();
            EventDispatcher.UnregisterKeyDown(this);
            Rescuee?.Dispose();
            _campfire.Dispose();
            UpdateManager.Remove(this);
        }

        public void Update()
        {
            if (Enemies != null)
            {
                var allDead = true;
                for (var i = 0; i < Enemies.Length; i++)
                    if (Enemies[i] != null && !Enemies[i].IsDead &&
                        (Enemies[i].Position - Position).Xz().LengthSquared()
                        < Radius * Radius * .75f * .75f)
                        allDead = false;

                Cleared = allDead;
            }

            _campfire.Position = Position.Xz().ToVector3() + Vector3.UnitY * Physics.HeightAtPosition(Position);
            _campfire.SetCanCraft(false);
            ManageOldMan();
        }

        private void BuildRescuee(Random Rng)
        {
            var randomTypes = new[]
            {
                HumanType.Warrior, HumanType.Archer, HumanType.Blacksmith, HumanType.Mage, HumanType.TravellingMerchant,
                HumanType.Bard, HumanType.Farmer, HumanType.Scholar, HumanType.GreenVillager, HumanType.Clothier,
                HumanType.Mason
            };
            Rescuee = NPCCreator.SpawnHumanoid(randomTypes[Rng.Next(0, randomTypes.Length)],
                Position + Vector3.UnitZ * 3.0f);
            Rescuee.ResetEquipment();
            Rescuee.Physics.UsePhysics = false;
            Rescuee.Physics.CollidesWithEntities = false;
            Rescuee.Physics.CollidesWithStructures = false;
            Rescuee.MainWeapon = null;

            Rescuee.Name = Rng.Next(0, 10) == 1 ? "Deckard Cain" : NameGenerator.PickMaleName(Rng);
            var trade = Rescuee.SearchComponent<TradeComponent>();
            if (trade != null) Rescuee.RemoveComponent(trade);
            Rescuee.RemoveComponent(Rescuee.SearchComponent<HealthBarComponent>());
            Rescuee.AddComponent(new HealthBarComponent(Rescuee, Rescuee.Name, HealthBarType.Friendly));
            Rescuee.SearchComponent<DamageComponent>().Immune = true;
            Rescuee.IsTied = true;
        }

        private Matrix4x4 BuildRescueeMatrix()
        {
            return Matrix4x4.CreateRotationX(-90f * Mathf.Radian)
                   * Matrix4x4.CreateTranslation(-Rescuee.Position)
                   * Matrix4x4.CreateTranslation(Vector3.UnitZ * 3f + Vector3.UnitY * 7f)
                   * Matrix4x4.CreateTranslation(Rescuee.Position);
        }

        private void ManageOldMan()
        {
            if (Completed) return;

            Rescuee.Model.TransformationMatrix = BuildRescueeMatrix();
            Rescuee.Position = Position.Xz().ToVector3() + Vector3.UnitY * (Physics.HeightAtPosition(Position) - 1);

            if (!Rescuee.InUpdateRange)
                Rescuee.Update();

            if (!Cleared || !((GameManager.Player.Position - Position).LengthSquared() < 16 * 16))
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
            Rescuee.Model.TransformationMatrix = Matrix4x4.Identity;
            Rescuee.Physics.UsePhysics = true;
            Rescuee.Physics.CollidesWithEntities = true;
            Rescuee.Physics.CollidesWithStructures = true;
            Rescuee.Position = new Vector3(Position.X + 8f, Physics.HeightAtPosition(Position) / Chunk.BlockSize - 2,
                Position.Z);
            Rescuee.Rotation = Vector3.Zero;
        }
    }
}