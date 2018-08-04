using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using NUnit.Framework;

namespace HedraTests
{
    public class BaseTest
    {
        protected SimpleEventProvider EventProvider { get; private set; }
        protected string SolutionDirectory { get; }

        public BaseTest()
        {
            SolutionDirectory = Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.FullName;
        }
        
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