using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Rendering.Animation;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class AnimatorTest
    {
        private Animation _animation;
        private Animator _animator;
        private Joint _rootJoint;
        
        [SetUp]
        public void Setup()
        {
            Time.RegisterThread();
            _rootJoint = new Joint(0, "Root", Matrix4.Identity);
            _rootJoint.InverseBindTransform = Matrix4.Identity;
            _rootJoint.AddChild(new Joint(1, "Chest", Matrix4.CreateTranslation(Vector3.UnitX)));
            _rootJoint.GetChild("Chest").InverseBindTransform = Matrix4.Identity;
            var frames = new KeyFrame[4];
            for (var i = 0; i < frames.Length; i++)
            {
                var dict = new Dictionary<string, JointTransform>
                {
                    {"Root", new JointTransform(Vector3.UnitY * i, Quaternion.Identity)},
                    {"Chest", new JointTransform(Vector3.UnitX * i, Quaternion.Identity)}
                };
                frames[i] = new KeyFrame(i * (1f/frames.Length), dict);
            }
            _animation = new Animation(1, frames);
            _animator = new Animator(_rootJoint);
        }
        
        [Test]
        public void TestAnimationEventsAreDispatched()
        {
            var start = false;
            var mid = false;
            var end = false;
            _animation.Loop = false;
            _animation.OnAnimationStart += Sender => start = true;
            _animation.OnAnimationMid += Sender => mid = true;
            _animation.OnAnimationEnd += Sender => end = true;
            _animator.PlayAnimation(_animation);
            Assert.NotNull(_animator.AnimationPlaying);
            for (var i = 0; i < 10; i ++)
            {
                Time.Set(.1f);
                _animator.Update();
            }
            Assert.Null(_animator.AnimationPlaying);
            Assert.True(start);
            Assert.True(mid);
            Assert.True(end);
        }
        
        [Test]
        public void TestAnimationLoops()
        {
            _animator.PlayAnimation(_animation);
            Assert.AreEqual(0, _animator.AnimationTime);
            
            Time.Set(.25f);
            _animator.Update();
            Assert.AreEqual(.25f, _animator.AnimationTime);
            
            Time.Set(.5f);
            _animator.Update();
            Assert.AreEqual(.75f, _animator.AnimationTime);
            
            Time.Set(.25f);
            _animator.Update();
            Assert.AreEqual(1.0f, _animator.AnimationTime);
            
            Time.Set(.5f);
            _animator.Update();
            Assert.AreEqual(.5f, _animator.AnimationTime);
        }
        
        [Test]
        public void TestAnimationIsResettedWhenPlaying()
        {
            var timesDispatched = 0;
            _animation.Loop = false;
            _animation.OnAnimationEnd += Sender => timesDispatched++; 
            _animation.DispatchEvents(1);
            
            Time.Set(1);
            _animator.PlayAnimation(_animation);
            _animator.Update();
            _animator.Update();
            Assert.AreEqual(2, timesDispatched);
        }
        
        [Test]
        public void TestAnimationTransformationsAreDoneCorrectly()
        {
            _animator.PlayAnimation(_animation);
            Time.Set(.5f);
            _animator.Update();
            _animator.Update();
            Assert.AreEqual(Matrix4.CreateTranslation(Vector3.UnitY * 16),
                _rootJoint.AnimatedTransform);
            Assert.AreEqual(Matrix4.CreateTranslation(Vector3.UnitY * 16 + Vector3.UnitX * 16),
                _rootJoint.GetChild("Chest").AnimatedTransform);
        }
    }
}