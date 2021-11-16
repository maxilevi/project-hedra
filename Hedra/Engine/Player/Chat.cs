/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/02/2017
 * Time: 11:56 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.Events;
using Hedra.Engine.Scripting;
using Hedra.Game;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Description of Chat.
    /// </summary>
    public class Chat : IDisposable
    {
        private readonly Script _script;
        private readonly Dictionary<string, object> _state;

        public Chat(IPlayer User)
        {
            _state = new Dictionary<string, object>();
            _script = Interpreter.GetScript("Chat.py");
            _script.Get("init").Invoke(User, _state);
            EventDispatcher.RegisterKeyDown(this, (S, A) => _script.Get("on_key_down").Invoke(A, _state));
        }

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
        }

        public void Update()
        {
            _script.Get("update").Invoke(_state);
        }

        public void Clear()
        {
            _script.Get("clear").Invoke(_state);
        }

        public void Write(string Message)
        {
            _script.Get("add_line").Invoke(_state, Message);
        }

        public static void Log(string Message)
        {
            GameManager.Player.Chat.Write(Message);
        }
    }
}