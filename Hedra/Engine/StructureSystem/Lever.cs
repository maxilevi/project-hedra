using System.Numerics;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem
{
    public delegate void OnLeverActivateEvent(IHumanoid Humanoid);

    public class Lever : AnimableInteractableStructure
    {
        public Lever(Vector3 Position, Vector3 Scale) : base(Position, Scale)
        {
        }

        public override string Message => Translations.Get("interact_lever");

        public override int InteractDistance => 16;

        protected override string ModelPath => "Assets/Env/Objects/LeverIdle.dae";
        protected override string IdleAnimationPath => "Assets/Env/Objects/LeverIdle.dae";
        protected override string UseAnimationPath => "Assets/Env/Objects/LeverActivate.dae";
        protected override string ColliderPath => "Assets/Env/Objects/Lever.ply";
        protected override float AnimationSpeed => 1.25f;
        protected override Vector3 ModelScale => Vector3.One * 3f;
        public event OnLeverActivateEvent OnActivate;

        protected override void OnUse(IHumanoid Humanoid)
        {
            OnActivate?.Invoke(Humanoid);
            SoundPlayer.PlaySound(SoundType.Door, Position);
        }
    }
}