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
using System.Diagnostics;
using Hedra.Engine.IO;

namespace Hedra.Engine.Management
{
    /// <summary>
    /// Helper class to simplify running delayed, concurrent, parallel and asynchronous actions.
    /// </summary>
    public static class TaskScheduler
    {

        /// <summary>
        /// Executes the provided action parallel to the main thread. 
        /// Uses the .NET ThreadPool class.
        /// </summary>
        /// <param name="Action">Action to execute</param>
        public static void Parallel(Action Action)
        {
            var trace = new StackTrace();
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    Action();
                }
                catch (Exception e)
                {
                    Log.WriteLine($"Failed to do job:{Environment.NewLine}{e}." +
                                  $"{Environment.NewLine}Trace:{Environment.NewLine}{trace}");
                }
            });
        }

        /// <summary>
        /// Executes the provided action asynchronously.
        /// </summary>
        /// <param name="Action">Action to execute.</param>
        public static void Asynchronous(Action Action, Action Callback = null)
        {
            var trace = new StackTrace();
            Task.Factory.StartNew(delegate
            {
                try
                {
                    Action();
                }
                catch (Exception e)
                {
                    Log.WriteLine($"Failed to do job:{Environment.NewLine}{e}." +
                                  $"{Environment.NewLine}Trace: {trace}");
                }
                finally
                {
                    Callback?.Invoke();
                }
            });
        }
        
        /// <summary>
        /// Executes a provided action after a specified delay.
        /// </summary>
        /// <param name="Time">Time to wait. In seconds.</param>
        /// <param name="Action">Action to execute.</param>
        public static void After(float Time, Action Action)
        {
            RoutineManager.StartRoutine(AfterSeconds, Time, Action);
        }

        /// <summary>
        /// Executes a provided action after a condition is met.
        /// </summary>
        /// <param name="Condition">Condition to be met.</param>
        /// <param name="Action">Action to execute.</param>
        public static void When(Func<bool> Condition, Action Action)
        {
            RoutineManager.StartRoutine(AfterAction, Condition, Action);
        }

        /// <summary>
        /// Executes a provided action concurrently.
        /// </summary>
        /// <param name="Action">Action to execute.</param>
        public static void While(Func<bool> Condition, Action Do)
        {
            RoutineManager.StartRoutine(WhileCondition, Condition, Do);
        }

        /// <summary>
        /// Executes a provided action concurrently.
        /// </summary>
        /// <param name="Action">Action to execute.</param>
        public static void Concurrent(Func<IEnumerator> Action)
        {
            RoutineManager.StartRoutine(Action);
        }

        /// <summary>
        /// Executes a provided action after a specific amount of frames have passed.
        /// </summary>
        /// <param name="Frames">Frames to wait for.</param>
        /// <param name="Do">Action to execute</param>
        public static void DelayFrames(int Frames, Action Do)
        {
            RoutineManager.StartRoutine(AfterFrames, Frames, Do);
        }

        /// <summary>
        /// Executes a provided action for a specific amount of seconds..
        /// </summary>
        /// <param name="Duration">Duration in seconds.</param>
        /// <param name="Do">Action to execute</param>
        public static void For(float Duration, Action Do)
        {
            var time = 0f;
            While(() => time < Duration,
            delegate
            {
                Do();
                time += Time.DeltaTime;
            });
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

        private static IEnumerator AfterSeconds(params object[] Args)
        {
            float passedTime = 0;
            float targetTime = Convert.ToSingle(Args[0]);
            var action = (Action)Args[1];
            while (passedTime < targetTime)
            {
                passedTime += Time.IndependantDeltaTime;
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
