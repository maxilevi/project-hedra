using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Hedra.Engine.Management
{
    public static class CoroutineManager
    {
        private static readonly List<IEnumerator> Coroutines;
        private static readonly object Lock;

        static CoroutineManager()
        {
            Coroutines = new List<IEnumerator>();
            Lock = new object();
        }

        public static void StartCoroutine(Func<IEnumerator> Func)
        {
            lock(Coroutines)
                Coroutines.Add(Func());
        }
         
         public static void StartCoroutine(Func<object, IEnumerator> Func, object Param)
         {
            lock(Coroutines)
                Coroutines.Add(Func(Param));
         }
         
         public static void StartCoroutine(Func<object[], IEnumerator> Func, params object[] Param)
         {
            lock(Coroutines)
                Coroutines.Add(Func(Param));
         }
         
        public static void Update()
        {
             lock(Coroutines)
             {
                for(var i = Coroutines.Count-1; i > -1; i--)
                {
                    var passed = Coroutines[i].MoveNext();
                    if(!passed)
                        Coroutines.RemoveAt(i);
                }
            }
        }
    }
}
