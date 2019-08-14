using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Sound;
using Hedra.Game;
using ThreadState = System.Threading.ThreadState;

namespace Hedra.Engine.Player
{
    public class BackgroundUpdater
    {
        private static SpinWait _spinner;
        private static Thread _updateThread;
        private static bool _isWaiting;
        private static Stopwatch _watch;
        private static object _updateLock;
        private static List<IUpdatable> _updateList;
        
        public static void Load()
        {
            _updateThread = new Thread(Update);
            _spinner = new SpinWait();
            _watch = new Stopwatch();
            _updateList = new List<IUpdatable>();
            _updateLock = new object();
        }
        
        public static void Dispatch()
        {
            if(_updateThread.ThreadState == ThreadState.Unstarted)
                _updateThread.Start();
            _isWaiting = false;
        }

        private static void Update()
        {
            Time.RegisterThread();
            _watch.Start();
            var lastTick = _watch.Elapsed.TotalSeconds;
            var elapsed = .0;
            var frames = 0;
            while (Program.GameWindow.Exists)
            {
                if(_isWaiting) _spinner.SpinOnce();
                var totalSeconds = _watch.Elapsed.TotalSeconds;
                var Delta = Math.Min(1.0, totalSeconds - lastTick);
                lastTick = totalSeconds;
                var frameTime = Delta;
                while (frameTime > 0f)
                {
                    var delta = Math.Min(frameTime, Physics.Timestep);
                    Time.Set(delta, false);
                    UpdateEntities();
                    UpdateCommands();
                    frameTime -= delta;
                }
                Time.Set(Delta);
                Time.IncrementFrame(Delta);
                _isWaiting = true;
            }
        }

        private static void UpdateEntities()
        {
            var entities = World.Entities.ToArray();
            for (var i = entities.Length - 1; i > -1; i--)
            {
                if (entities[i] != GameManager.Player && 
                    (
                        entities[i].InUpdateRange &&
                        !GameSettings.Paused &&
                        !GameManager.IsLoading ||
                        entities[i].IsBoss)
                    )
                {
                    if(GameManager.Player.Companion.Entity == entities[i]) continue;
                    
                    entities[i].Update();
                }
                else if (entities[i] != GameManager.Player && entities[i].InUpdateRange && GameSettings.Paused)
                {
                    (entities[i].Model as IAudible)?.StopSound();
                }
            }
        }

        private static void UpdateCommands()
        {
            lock (_updateLock)
            {
                for (var i = 0; i < _updateList.Count; ++i)
                {
                    _updateList[i].Update();
                }
            }
        }

        public static void Add(IUpdatable Update)
        {
            lock (_updateLock)
            {
                _updateList.Add(Update);
            }
        }

        public static void Remove(IUpdatable Update)
        {
            lock (_updateLock)
            {
                _updateList.Remove(Update);
            }
        }
    }
}