using Hedra.Engine.Generation;
using NUnit.Framework;

namespace HedraTests
{
    [TestFixture]
    public class BaseTest
    {
        [SetUp]
        public virtual void Setup()
        {
            World.Provider = new DummyProvider();
            //EventDispatcher.Provider = new DummyProvider();
        }

        [TearDown]
        public virtual void Teardown()
        {
            
        }
    }
}