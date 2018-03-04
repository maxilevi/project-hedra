using System;
using System.Collections.Generic;
using System.Threading;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.QuestSystem;

namespace Hedra.Engine.PhysicsSystem
{
    public class PhysicsThread
    {
        private readonly Thread _thread;
        private readonly List<Entity> _toUpdate;
        private readonly List<MoveCommand> _toMove;
        private bool _sleep;

        public PhysicsThread()
        {
            _toUpdate = new List<Entity>();
            _toMove = new List<MoveCommand>();
            _thread = new Thread(this.Process);
            _thread.Start();
        }

        public void Add(Entity Item)
        {
            _toUpdate.Add(Item);
        }

        public void Add(MoveCommand Item)
        {
            _toMove.Add(Item);
        }

        public void Wakeup()
        {
            _sleep = false;
        }

        public int Count => _toMove.Count;

        public void Process()
        {
            while (Program.GameWindow.Exists)
            {
                if (_sleep)
                {
                    Thread.Sleep(1);
                    continue;
                }

                for (int i = _toUpdate.Count - 1; i > -1; i--)
                {
                    try
                    {
                        if (_toUpdate[i] != null)
                            _toUpdate[i].Physics.Update();

                        _toUpdate.RemoveAt(i);
                    }
                    catch (Exception e)
                    {
                        Log.WriteLine(e.ToString());
                    }
                }



                for (int i = _toMove.Count - 1; i > -1; i--)
                {
                    try
                    {
                        if (_toMove[i].Parent != null)
                            _toMove[i].Parent.Physics.ProccessCommand(_toMove[i]);

                        _toMove.RemoveAt(i);
                    }
                    catch (Exception e)
                    {
                        Log.WriteLine(e.ToString());
                    }
                }

                _sleep = true;
            }

        }
    }
}
