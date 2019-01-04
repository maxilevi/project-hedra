using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Hedra.Engine.Management
{
    public static class RoutineManager
    {
        private static readonly List<IEnumerator> Routines;
        private static readonly object Lock;

        static RoutineManager()
        {
            Routines = new List<IEnumerator>();
            Lock = new object();
        }

        public static void StartRoutine(Func<IEnumerator> Func)
        {
            lock(Lock)
                Routines.Add(Func());
        }

        public static void StartRoutine(Func<object[], IEnumerator> Func, params object[] Param)
        {
            lock(Lock)
                Routines.Add(Func(Param));
        }
         
        public static void Update()
        {
             lock(Lock)
             {
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
