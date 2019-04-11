using System;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.Engine.Player
{
    public class HumanoidModelAnimationState
    {
        private readonly IHumanoid _humanoid;
        private readonly HumanoidModel _model;
        public Animation DefaultBlending { get; set; }
        private Animation _walkAnimation;
        private Animation _idleAnimation;
        private Animation _rollAnimation;
        private Animation _eatAnimation;
        private Animation _swimAnimation;
        private Animation _idleSwimAnimation;
        private Animation _rideAnimation;
        private Animation _idleRideAnimation;
        private Animation _climbAnimation;
        private Animation _sitAnimation;
        private Animation _glideAnimation;
        private Animation _knockedAnimation;
        private Animation _tiedAnimation;
        private Animation _sleepAnimation;
        private Animation _jumpAnimation;
        private Animation _sailingAnimation;
        private Animation _helloAnimation;
        private Animation _fishingAnimation;
        
        public HumanoidModelAnimationState(IHumanoid Humanoid, HumanoidModel Model)
        {
            _humanoid = Humanoid;
            _model = Model;
            _walkAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorWalk.dae");
            _idleAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
            _rollAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorRoll.dae");
            _eatAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorEat.dae");
            _swimAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSwim.dae");
            _idleSwimAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSwimIdle.dae");
            _climbAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorClimb.dae");
            _rideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorMount.dae");
            _idleRideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorMountIdle.dae");
            _sitAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSit.dae");
            _glideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorGlide.dae");
            _knockedAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorKnocked.dae");
            _tiedAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorTied.dae");
            _sleepAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSleep.dae");
            _jumpAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorJump.dae");
            _sailingAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSit.dae");
            _fishingAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSit.dae");
            _helloAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorHello.dae");
            _helloAnimation.Loop = false;
            _helloAnimation.OnAnimationEnd += delegate
            {
                
            };
            _rollAnimation.OnAnimationEnd += delegate
            { 
                Humanoid.Physics.ResetFall();
                Humanoid.IsRolling = false;
            };
        }

        public void SelectAnimation(out Animation Current, out Animation Blend)
        {
            Current = null;
            Blend = DefaultBlending;

            if (IsMoving)
            {
                Current = _walkAnimation;
                if (IsRiding)
                {
                    Current = _rideAnimation;
                }
                if (IsUnderwater)
                {
                    Current = _swimAnimation;
                }
            }
            else
            {
                Current = _idleAnimation;
                if (IsRiding)
                {
                    Current = _idleRideAnimation;
                }
                if (IsUnderwater)
                {
                    Current = _idleSwimAnimation;
                }
            }
            if (IsTied)
            {
                Current = _tiedAnimation;
            }
            if (IsSitting)
            {
                Current = _sitAnimation;
            }
            if (IsSleeping)
            {
                Current = _sleepAnimation;
            }
            if (IsRolling)
            {
                Current = _rollAnimation;
            }
            if (IsJumping)
            {
                Current = _jumpAnimation;
            }
            if (IsEating)
            {
                Blend = _eatAnimation;
            }
            if (IsGliding)
            {
                Current = _glideAnimation;
            }
            if (IsSailing)
            {
                Current = _sailingAnimation;
            }
            if (IsClimbing)
            {
                Current = _climbAnimation;
            }
            if (IsKnocked)
            {
                Current = _knockedAnimation;
            }
            if (IsFishing)
            {
                Current = _fishingAnimation;
            }
        }

        public void Greet(Action Callback)
        {
            void OnEndLambda(Animation Sender)
            {
                Callback();
                _helloAnimation.OnAnimationEnd -= OnEndLambda;
            };
            _helloAnimation.OnAnimationEnd += OnEndLambda;
            _model.PlayAnimation(_helloAnimation);
        }

        public void Update()
        {
            _walkAnimation.Speed = _humanoid.Speed;
        }

        public bool IsWalking => _walkAnimation == _model.AnimationPlaying;

        public virtual bool[] ModifiableStates
        {
            get => new []
            {
                IsRiding,
                IsTied,
                IsSitting,
                IsSleeping,
                IsRolling,
                IsJumping,
                IsEating,
                IsGliding,
                IsSailing,
                IsClimbing,
                IsKnocked,
                IsFishing
            };
            set => throw new NotImplementedException();
        }

        private bool IsMoving => _humanoid.IsMoving;
        private bool IsUnderwater => _humanoid.IsUnderwater;
        protected virtual bool IsRiding => _humanoid.IsRiding;
        protected virtual bool IsTied => _humanoid.IsTied;
        protected virtual bool IsSitting => _humanoid.IsSitting;
        protected virtual bool IsSleeping => _humanoid.IsSleeping;
        protected virtual bool IsRolling => _humanoid.IsRolling;
        protected virtual bool IsJumping => _humanoid.IsJumping;
        protected virtual bool IsEating => _humanoid.IsEating;
        protected virtual bool IsGliding => _humanoid.IsGliding;
        protected virtual bool IsSailing => _humanoid.IsSailing;
        protected virtual bool IsClimbing => _humanoid.IsClimbing;
        protected virtual bool IsKnocked => _humanoid.IsKnocked;
        protected virtual bool IsFishing => _humanoid.IsFishing;
    }
}