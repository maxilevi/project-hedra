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
		public static void Delay(int Time, Action Action){
			CoroutineManager.StartCoroutine(AfterSeconds, Time, Action);
		}

        /// <summary>
        /// Executes a provided action after a condition is met.
        /// </summary>
        /// <param name="Condition">Condition to be met.</param>
        /// <param name="Action">Action to execute.</param>
	    public static void Delay(Func<bool> Condition, Action Action)
	    {
	        CoroutineManager.StartCoroutine(AfterAction, Condition, Action);
	    }

        /// <summary>
        /// Executes a provided action concurrently.
        /// </summary>
        /// <param name="Action">Action to execute.</param>
        public static void Concurrent(Func<IEnumerator> Action)
	    {
	        CoroutineManager.StartCoroutine(Action);
	    }

        #region HelperCoroutines
        private static IEnumerator AfterSeconds(params object[] Args){
			float passedTime = 0;
			float milliseconds = Convert.ToSingle(Args[0]);
            var action = (Action)Args[1];
            while (passedTime*1000 < milliseconds){
				passedTime += Time.FrameTimeSeconds;
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
        #endregion
    }
}
