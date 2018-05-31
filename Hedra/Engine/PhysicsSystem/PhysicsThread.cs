using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.PhysicsSystem
{
    public class PhysicsThread
    {
        public OnBatchProcessedEventHandler OnBatchProcessedEvent;
        public OnCommandProcessedEventHandler OnCommandProcessedEvent;
        public PhysicsThreadType Type { get; }
        private readonly Thread _thread;
        private readonly ConcurrentBag<Entity> _toUpdate;
        private readonly ConcurrentBag<MoveCommand> _toMove;
        private bool _sleep;

        public PhysicsThread(PhysicsThreadType Type)
        {
            this.Type = Type;
            _toUpdate = new ConcurrentBag<Entity>();
            _toMove = new ConcurrentBag<MoveCommand>();
            _thread = new Thread(this.Process);
            _thread.Start();
        }

        public void Add(Entity Item)
        {
            _toUpdate.Add(Item);
        }

        public void Add(MoveCommand Item)
        {
            try
            {
                _toMove.Add(Item);
            }
            catch (IndexOutOfRangeException e)
            {
                Log.WriteLine("Detected a sync error.");
            }
        }

        public void Wakeup()
        {
            _sleep = false;
        }

        public int Count => _toMove.Count;

        public void ProcessCommands()
        {
            while (!_toMove.IsEmpty)
            {
                try
                {
                    var result = _toMove.TryTake(out MoveCommand command);
                    if (result)
                    {
                        command.Parent?.Physics.ProccessCommand(command);
                        OnCommandProcessedEvent?.Invoke(command);
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLine(e.ToString());
                    break;
                }
            }
        }

        public void ProcessUpdates()
        {
            while (!_toUpdate.IsEmpty)
            {
                try
                {
                    var result = _toUpdate.TryTake(out Entity entity);
                    if (result)
                    {
                        entity?.Physics.Update();
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLine(e.ToString());
                    break;
                }
            }
        }

        public void Process()
        {
            while (Program.GameWindow.Exists)
            {
                if (_sleep)
                {
                    Thread.Sleep(1);
                    continue;
                }

                switch (Type)
                {
                    case PhysicsThreadType.ProcessUpdate:
                        this.ProcessUpdates();
                        break;
                    case PhysicsThreadType.ProcessCommand:
                        this.ProcessCommands();
                        break;
                }

                OnBatchProcessedEvent?.Invoke();
                _sleep = true;
            }
        }
    }

    public enum PhysicsThreadType
    {
        ProcessUpdate,
        ProcessCommand
    }
}
