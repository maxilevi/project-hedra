/*
 * Author: Zaphyk
 * Date: 13/02/2016
 * Time: 07:04 p.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hedra.Engine.Management
{
    /// <summary>
    /// A static class which manages invokes on the main thread
    /// </summary>
    public static class Executer
    {
        private static readonly List<InvokerCall> Functions = new List<InvokerCall>();
        private static readonly List<InvokerCall> StandBy = new List<InvokerCall>();
        private static readonly object Lock = new object();
        
        /// <summary>
        /// Executes the give method on the main thread after a frame has passed.
        /// </summary>
        public static void ExecuteOnMainThread(Action Func)
        {
            lock (Lock)
                StandBy.Add(new InvokerCall(Func));
        }

        public static void Flush()
        {
            lock (Lock)
                StandBy.Clear();
        }
        
        public static void Clear()
        {
            Flush();
            lock (Lock)
                Functions.Clear();
        }

        public static void Update()
        {
            lock (Lock)
            {
                Functions.AddRange(StandBy.ToArray());
                StandBy.Clear();
            }

            for (var i = 0; i < Functions.Count; i++)
            {
                Functions[i].Call();
            }
            Functions.Clear();
        }

        private struct InvokerCall
        {
            /* Used for debugging */
#if DEBUG
            public StackTrace Trace;
#else
                Trace = null;
#endif
            public Action Call;

            public InvokerCall(Action Call)
            {
                this.Call = Call;
#if DEBUG
                Trace = new StackTrace();
#else
                Trace = null;
#endif
            }
        }
    }
}
