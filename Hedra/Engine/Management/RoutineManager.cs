using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Hedra.Core;

namespace Hedra.Engine.Management
{
    public static class RoutineManager
    {
        private static readonly List<IEnumerator> Routines;
        private static readonly List<IEnumerator> RoutinesToAdd;
        private static readonly object Lock;
        private static readonly object ToAddLock;

        static RoutineManager()
        {
            Routines = new List<IEnumerator>();
            RoutinesToAdd = new List<IEnumerator>();
            ToAddLock = new object();
            Lock = new object();
        }

        public static void StartRoutine(Func<IEnumerator> Func)
        {
            lock(ToAddLock)
                RoutinesToAdd.Add(Func());
        }

        public static void StartRoutine(Func<object[], IEnumerator> Func, params object[] Param)
        {
            lock(ToAddLock)
                RoutinesToAdd.Add(Func(Param));
        }
         
        public static void Update()
        {
            lock(Lock)
            {
                lock (ToAddLock)
                {
                    for (var i = 0; i < RoutinesToAdd.Count; ++i)
                    {
                        Routines.Add(RoutinesToAdd[i]);
                    }
                    RoutinesToAdd.Clear();
                }
                for(var i = Routines.Count-1; i > -1; i--)
                {
                    var passed = Routines[i].MoveNext();
                    if(!passed)
                        Routines.RemoveAt(i);
                }
            }
        }

        public static IEnumerator WaitForSeconds(float Seconds)
        {
            var passedTime = 0f; 
            while (passedTime < Seconds)
            {
                passedTime += Time.DeltaTime;
                yield return null;
            }
        }

        public static void Clear()
        {
            lock(Lock)
                Routines.Clear();
            lock(ToAddLock)
                RoutinesToAdd.Clear();
        }

        public static int Count
        {
            get
            {
                lock (Lock)
                    return Routines.Count;
            }
        }
    }
}
