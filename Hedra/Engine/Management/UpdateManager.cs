/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 05:35 p.m.
 *
 */
using System.Collections.Generic;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using OpenTK.Input;

namespace Hedra.Engine.Management
{
    /// <summary>
    /// A static class which gathers all update functions
    /// </summary>
    public static class UpdateManager
    {
        private static readonly HashSet<IUpdatable> UpdateFunctions;
        private static readonly List<IUpdatable> UpdateFunctionsList;
        private static readonly TickSystem Ticker;
        private static readonly object Lock = new object();
        private static readonly List<IUpdatable> ToRemove;

        static UpdateManager()
        {
            UpdateFunctions = new HashSet<IUpdatable>();
            UpdateFunctionsList = new List<IUpdatable>();
            ToRemove = new List<IUpdatable>();
            Ticker = new TickSystem();
        }

        public static void Add(IUpdatable Updatable)
        {
            lock (Lock)
            {
                if (Updatable is ITickable tickable)
                {
                    Ticker.Add(tickable);
                    return;
                }
                UpdateFunctions.Add(Updatable);
                UpdateFunctionsList.Add(Updatable);
            }
        }

        public static void Remove(IUpdatable Updatable)
        {
            lock (Lock)
            {
                ToRemove.Add(Updatable);
            }
        }

        private static void DoRemove(IUpdatable Updatable)
        {
            lock (Lock)
            {
                if (Updatable is ITickable tickable)
                {
                    Ticker.Remove(tickable);
                    return;
                }
                UpdateFunctions.Remove(Updatable);
                UpdateFunctionsList.Remove(Updatable);
            }
        }
         
        public static void Update()
        {
            lock (Lock)
            {
                RemovePending();
                for (var i = 0; i < UpdateFunctionsList.Count; ++i)
                {
                    if (UpdateFunctionsList[i] == null)
                    {
                        UpdateFunctions.Remove(UpdateFunctionsList[i]);
                        UpdateFunctionsList.RemoveAt(i);
                        continue;
                    }

                    UpdateFunctionsList[i].Update();
                }

                Ticker.Tick();
                SkyManager.Update();
            }
        }

        private static void RemovePending()
        {
            for(var i = 0; i < ToRemove.Count; i++)
            {
                DoRemove(ToRemove[i]);
            }
            ToRemove.Clear();
        }

        public static int UpdateCount
        {
            get
            {
                lock (Lock)
                {
                    return UpdateFunctionsList.Count;
                }
            }
        }
    }
}