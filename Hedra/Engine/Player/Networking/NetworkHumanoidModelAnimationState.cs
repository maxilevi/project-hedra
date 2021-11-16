using Hedra.Engine.ModuleSystem.Templates;
using Hedra.EntitySystem;

namespace Hedra.Engine.Player.Networking
{
    public class NetworkHumanoidModelAnimationState : HumanoidModelAnimationState
    {
        private bool _isClimbing;
        private bool _isEating;
        private bool _isFishing;
        private bool _isGliding;
        private bool _isJumping;
        private bool _isKnocked;
        private bool _isRiding;
        private bool _isRolling;
        private bool _isSailing;
        private bool _isSitting;
        private bool _isSleeping;
        private bool _isTied;

        public NetworkHumanoidModelAnimationState(IHumanoid Humanoid, HumanoidModel Model,
            HumanoidModelTemplate Template) : base(Humanoid, Model, Template)
        {
        }

        public override bool[] ModifiableStates
        {
            get => base.ModifiableStates;
            set
            {
                _isRiding = value[0];
                _isTied = value[1];
                _isSitting = value[2];
                _isSleeping = value[3];
                _isRolling = value[4];
                _isJumping = value[5];
                _isEating = value[6];
                _isGliding = value[7];
                _isSailing = value[8];
                _isClimbing = value[9];
                _isKnocked = value[10];
                _isFishing = value[11];
            }
        }

        protected override bool IsRiding => _isRiding;
        protected override bool IsTied => _isTied;
        protected override bool IsSitting => _isSitting;
        protected override bool IsSleeping => _isSleeping;
        protected override bool IsRolling => _isRolling;
        protected override bool IsJumping => _isJumping;
        protected override bool IsEating => _isEating;
        protected override bool IsGliding => _isGliding;
        protected override bool IsSailing => _isSailing;
        protected override bool IsClimbing => _isClimbing;
        protected override bool IsKnocked => _isKnocked;
        protected override bool IsFishing => _isFishing;
    }
}