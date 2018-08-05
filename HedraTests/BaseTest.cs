using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Moq;
using NUnit.Framework;

namespace HedraTests
{
    public abstract class BaseTest
    {
        protected SimpleEventProvider EventProvider { get; private set; }
        
        [SetUp]
        public virtual void Setup()
        {
            EventProvider = new SimpleEventProvider();
            World.Provider = new SimpleWorldProviderMock();
            EventDispatcher.Provider = EventProvider;
            Graphics2D.Provider = new SimpleTexture2DProviderMock();
        }

        [TearDown]
        public virtual void Teardown()
        {
            
        }
    }
}