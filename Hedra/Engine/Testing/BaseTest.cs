using System;
using System.Linq;
using System.Reflection;
using Hedra.Engine.Management;

namespace Hedra.Engine.Testing
{
    public class BaseTest
    {
        /// <summary>
        /// Propierty indicating if a test is passed.
        /// </summary>
        public bool Passed { get; private set; }

        /// <summary>
        /// Detailed output of the test result.
        /// </summary>
        public string Log { get; private set; }

        /// <summary>
        /// Count of total errors the test found.
        /// </summary>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// Name of the current test being run.
        /// </summary>
        public string TestName { get; private set; }

        /// <summary>
        /// Sets up the enviroment before tests are executed.
        /// </summary>
        public virtual void Setup() {}

        /// <summary>
        /// Clears the test output.
        /// </summary>
        public void Reset()
        {
            Log = string.Empty;
            Passed = true;
            ErrorCount = 0;
            TestName = string.Empty;
        }

        /// <summary>
        /// Runs all the tests in this class.
        /// </summary>
        public void Run()
        {
            this.Setup();

            var methods = this.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.IsDefined(typeof(TestMethod), true) && false)
                {
                    this.Reset();
                    TestName = this.GetType().Name + "." + method.Name;

                    method.Invoke(this, null);

                    Engine.Log.Write(TestName + " ", ConsoleColor.Magenta);
                    Engine.Log.Write((Passed ? "PASSED" : "FAILED") + Environment.NewLine, Passed ? ConsoleColor.Green : ConsoleColor.Red);
                    Tester.Log( string.IsNullOrEmpty(this.Log) ? this.Log : this.Log + Environment.NewLine);
                }
            }

            TaskManager.Parallel(delegate
            {
                var tests = methods.ToList().Where(M => M.IsDefined(typeof(AutomatedTest), true));
                tests.ToList().ForEach( T => T.Invoke(this, null) );
            });
        }

        /// <summary>
        /// Assert a condition is true, otherwise fail with the provided message.
        /// </summary>
        /// <param name="Condition">The condition to assert.</param>
        /// <param name="Message">Message to display.</param>
        public void AssertTrue(bool Condition, string Message = "Expected TRUE got FALSE")
        {
            if (!Condition)
            {
                this.Passed = false;
                this.Log += TestName+"_ERR"+ ErrorCount + " : "+ Message + Environment.NewLine;
                this.ErrorCount++;
            }
        }

        /// <summary>
        /// Assert a condition is false, otherwise fail with the provided message.
        /// </summary>
        /// <param name="Condition">The condition to assert.</param>
        /// <param name="Message">Message to display.</param>
        public void AssertFalse(bool Condition, string Message = "Expected FALSE got TRUE")
        {
           this.AssertTrue(!Condition, Message);
        }

        /// <summary>
        /// Assert both objects are equal, otherwise fail with the provided message.
        /// </summary>
        /// <param name="Obj1">The first object to compare.</param>
        /// <param name="Obj2">The second object to compare.</param>
        /// <param name="Message">Message to display.</param>
        public void AssertEqual(object Obj1, object Obj2, string Message = null)
        {
            if (Message == null)
            {
                Message = $"EQUAL expectation fails at {Obj1.ToString()} == {Obj2.ToString()}";
            }
            this.AssertTrue(Obj1.Equals(Obj2), Message);
        }
    }
}
