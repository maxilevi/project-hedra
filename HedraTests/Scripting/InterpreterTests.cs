using Hedra.Engine.Scripting;
using NUnit.Framework;

namespace HedraTests.Scripting
{
    [TestFixture]
    public class InterpreterTests
    {
        [Test]
        public void CompileScripts()
        {
            Assert.DoesNotThrow( Interpreter.Load, "Failed to load scripts");   
        }
    }
}