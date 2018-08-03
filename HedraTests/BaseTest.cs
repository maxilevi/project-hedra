using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using NUnit.Framework;

namespace HedraTests
{
    public class BaseTest
    {
        protected SimpleEventProvider EventProvider { get; private set; }
        
        [SetUp]
        public virtual void Setup()
        {
            EventProvider = new SimpleEventProvider();
            World.Provider = new SimpleWorldProviderMock();
            EventDispatcher.Provider = EventProvider;
        }

        [TearDown]
        public virtual void Teardown()
        {
            
        }
    }
}