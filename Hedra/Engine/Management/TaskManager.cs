/*
 * Author: Maxi Levi
 * Date: 08/02/2016
 * Time: 03:04 a.m.
 *
 */
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Helper class to simplify running delayed, concurrent, parallel and asynchronous actions.
	/// </summary>
	public static class TaskManager{

        /// <summary>
        /// Executes the provided action parallel to the main thread. 
        /// Uses the .NET ThreadPool class.
        /// </summary>
        /// <param name="Action">Action to execute</param>
        public static void Parallel(Action Action)
		{
		    ThreadPool.QueueUserWorkItem(delegate { Action(); });
		}

        /// <summary>
        /// Executes the provided action asynchronously.
        /// </summary>
        /// <param name="Action">Action to execute.</param>
        public static void Asynchronous(Action Action){
			Task.Factory.StartNew(Action);
		}
		
        /// <summary>
        /// Executes a provided action after a specified delay.
        /// </summary>
        /// <param name="Time">Time to wait. In milliseconds.</param>
        /// <param name="Action">Action to execute.</param>
		public static void After(int Time, Action Action){
			CoroutineManager.StartCoroutine(AfterSeconds, Time, Action);
		}

        /// <summary>
        /// Executes a provided action after a condition is met.
        /// </summary>
        /// <param name="Condition">Condition to be met.</param>
        /// <param name="Action">Action to execute.</param>
	    public static void When(Func<bool> Condition, Action Action)
	    {
	        CoroutineManager.StartCoroutine(AfterAction, Condition, Action);
	    }

	    /// <summary>
	    /// Executes a provided action concurrently.
	    /// </summary>
	    /// <param name="Action">Action to execute.</param>
	    public static void While(Func<bool> Condition, Action Do)
	    {
	        CoroutineManager.StartCoroutine(WhileCondition, Condition, Do);
	    }

        /// <summary>
        /// Executes a provided action concurrently.
        /// </summary>
        /// <param name="Action">Action to execute.</param>
        public static void Concurrent(Func<IEnumerator> Action)
	    {
	        CoroutineManager.StartCoroutine(Action);
	    }

        /// <summary>
        /// Executes a provided action after a specific amount of frames have passed.
        /// </summary>
        /// <param name="Frames">Frames to wait for.</param>
        /// <param name="Do">Action to execute</param>
	    public static void Delay(int Frames, Action Do)
	    {
	        CoroutineManager.StartCoroutine(AfterFrames, Frames, Do);
	    }

        #region HelperCoroutines
	    private static IEnumerator AfterFrames(params object[] Args)
	    {
	        var framesToPass = (int) Args[0];
	        var action = (Action) Args[1];
	        var passedFrames = 0;
	        while (passedFrames < framesToPass)
	        {
	            passedFrames++;
	            yield return null;
	        }
	        action();
	    }

        private static IEnumerator AfterSeconds(params object[] Args){
			float passedTime = 0;
			float milliseconds = Convert.ToSingle(Args[0]);
            var action = (Action)Args[1];
            while (passedTime*1000 < milliseconds){
				passedTime += Time.ScaledFrameTimeSeconds;
				yield return null;
			}
			action();
		}

	    private static IEnumerator AfterAction(params object[] Args)
	    {
	        var pointerToGate = (Func<bool>) Args[0];
	        var action = (Action) Args[1];
            while (!pointerToGate())
	        {
	            yield return null;
	        }
	        action();
	    }

	    private static IEnumerator WhileCondition(params object[] Args)
	    {
	        var condition = (Func<bool>)Args[0];
	        var action = (Action)Args[1];
	        while (condition())
	        {
	            action();
	            yield return null;
	        }
	    }
        #endregion
    }
}
