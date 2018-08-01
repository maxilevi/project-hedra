using Hedra.Engine.Generation;
using NUnit.Framework;

namespace HedraTests
{
    public class BaseTest
    {
        [SetUp]
        public virtual void Setup()
        {
            World.Provider = new SimpleWorldProviderMock();
            //EventDispatcher.Provider = new DummyProvider();
        }

        [TearDown]
        public virtual void Teardown()
        {
            
        }
    }
}