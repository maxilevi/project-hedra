using System.Media;
using Hedra.AISystem;
using Hedra.AISystem.Humanoid;
using Hedra.Components.Effects;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Sound;
using OpenTK;
using SoundPlayer = Hedra.Sound.SoundPlayer;

namespace Hedra.Mission.Blocks
{
    public class CatchAnimalMission : MissionBlock
    {
        public IEntity Animal { get; set; }

        public override bool IsCompleted =>
            (Animal.Position - Owner.Position).LengthSquared < Chunk.BlockSize * Chunk.BlockSize;
        public override void Setup()
        {
            if(Animal.SearchComponent<BasicAIComponent>() != null)
                Animal.RemoveComponent(Animal.SearchComponent<BasicAIComponent>());
            Animal.ShowIcon(CacheItem.AttentionIcon);
            Animal.AddComponent(new SpeedBonusComponent(Animal, -Animal.Speed + Owner.Speed - 0.15f));
            Animal.AddComponent(new EscapeAIComponent(Animal, Owner));
            Animal.AddComponent(new CatchComponent(Animal, Owner));
        }

        public override QuestView BuildView()
        {
            return new EntityView((QuadrupedModel)Animal.Model);
        }

        public override bool HasLocation => true;
        public override Vector3 Location => Animal.Position;
        public override string ShortDescription => Translations.Get("quest_catch_animal_short", Animal.Name.ToUpperInvariant());
        public override string Description => Translations.Get("quest_catch_animal_description", Giver.Name, Animal.Name.ToUpperInvariant(), Animal.Name.ToUpperInvariant());
        public override DialogObject DefaultOpeningDialog => new DialogObject
        {
            Keyword = "quest_catch_animal_dialog",
            Arguments = new object[]
            {
                Animal.Type.ToUpperInvariant()
            }
        };
        
        private class CatchComponent : EntityComponent
        {
            private readonly IPlayer _owner;
            private bool _disposed;
            public CatchComponent(IEntity Entity, IPlayer Owner) : base(Entity)
            {
                _owner = Owner;
            }

            public override void Update()
            {
                if (_disposed || !((Parent.Position - _owner.Position).LengthSquared < Chunk.BlockSize * Chunk.BlockSize)) return;
                _owner.Questing.Trigger();
                SoundPlayer.PlaySound(SoundType.NotificationSound, Parent.Position);
                Parent.AddComponent(new DisposeComponent(Parent, 0));
                _disposed = true;
            }
        }
    }
}