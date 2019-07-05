/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 27/01/2017
 * Time: 04:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Collections.Generic;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Description of PetManager.
    /// </summary>
    public class CompanionHandler
    {
        private readonly Script _script;
        private readonly Dictionary<string, object> _state;
        public IEntity Entity => (IEntity) _state["pet"];

        public CompanionHandler(IPlayer Player)
        {
            _state = new Dictionary<string, object>();
            _script = Interpreter.GetScript("Companion.py");
            _script.Get("init").Invoke(Player, _state);
        }

        public void Update()
        {
            _script.Get("update").Invoke(_state);
        }
    }
}