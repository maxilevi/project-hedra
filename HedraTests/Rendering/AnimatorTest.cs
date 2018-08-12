using System.Collections.Generic;
using Hedra.Engine;
using Hedra.Engine.Rendering.Animation;
using NUnit.Framework;
using OpenTK;

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
        public void TestPlayAnimationFromStartToEnd()
        {
            /*_animator.PlayAnimation(_animation);
            TestContext.WriteLine(_animation.Speed);
            TestContext.WriteLine(_rootJoint.AnimatedTransform);
            TestContext.WriteLine(_rootJoint.GetChild("Chest").AnimatedTransform);
            Time.Set(.25f);
            _animator.Update();
            Time.Set(.5f);
            TestContext.WriteLine(_animation.Speed);
            TestContext.WriteLine(_rootJoint.AnimatedTransform);
            TestContext.WriteLine(_rootJoint.GetChild("Chest").AnimatedTransform);
            _animator.Update();
            Time.Set(.25f);
            TestContext.WriteLine(_animation.Speed);
            TestContext.WriteLine(_rootJoint.AnimatedTransform);
            TestContext.WriteLine(_rootJoint.GetChild("Chest").AnimatedTransform);*/
        }
    }
}