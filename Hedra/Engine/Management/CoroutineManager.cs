using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Hedra.Engine.Management
{
	public static class CoroutineManager
	{
		public static List<IEnumerator> Coroutines = new List<IEnumerator>();

        public static void StartCoroutine(Func<IEnumerator> func)
	     {
	     	lock(Coroutines){
	     		Coroutines.Add(func());
            }
	     }
	     
	     public static void StartCoroutine(Func<object, IEnumerator> func, object param)
	     {
	     	lock(Coroutines){
	     		Coroutines.Add(func(param));
	     	}
	     }
	     
	     public static void StartCoroutine(Func<object[], IEnumerator> func, params object[] param)
	     {
	     	lock(Coroutines){
	     		Coroutines.Add(func(param));
	     	}
	     }
	     
	     public static void Update()
	     {
	     	lock(Coroutines){
		         for(int i = Coroutines.Count-1; i > -1; i--)
		         {
                    
                    bool Passed = Coroutines[i].MoveNext();
		         	if(!Passed)
		         		Coroutines.RemoveAt(i);
		         }
	     	}
	     }
	}
}
