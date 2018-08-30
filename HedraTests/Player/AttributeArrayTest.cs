using Hedra.Engine.ItemSystem;
using NUnit.Framework;

namespace HedraTests.Player
{
    [TestFixture]
    public class AttributeArrayTest
    {
        private AttributeArray _attributes;
        
        [SetUp]
        public void Setup()
        {
            _attributes = new AttributeArray();    
        }
        
        [Test]
        public void TestHas()
        {
            Assert.False(_attributes.Has("Object0"));
            _attributes.Set("Object0", new object());
            Assert.True(_attributes.Has("Object0"));
        }

        [Test]
        public void TestSetAndGet()
        {
            var obj = new object();
            _attributes.Set("Object0", obj);
            Assert.AreSame(obj, _attributes.Get<object>("Object0"));
        }

        [Test]
        public void TestReplace()
        {
            var obj0 = new object();
            var obj1 = new object();
            _attributes.Set("Object0", obj0);
            Assert.AreNotSame(obj1, _attributes.Get<object>("Object0"));
            _attributes.Set("Object0", obj1);
            Assert.AreSame(obj1, _attributes.Get<object>("Object0"));
        }

        [Test]
        public void TestDelete()
        {
            _attributes.Set("Object0", new object());
            _attributes.Delete("Object0");
            Assert.False(_attributes.Has("Object0"));
        }

        [Test]
        public void TestClear()
        {
            _attributes.Set("Object0", new object());
            _attributes.Clear();
            Assert.False(_attributes.Has("Object0"));
        }
    }
}