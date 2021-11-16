/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 27/01/2017
 * Time: 04:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;

namespace Hedra.Engine.Player
{
    public delegate void OnCompanionChanged(Item NewItem, IEntity NewPet);

    public class CompanionHandler
    {
        private readonly IPlayer _player;
        private readonly Script _script;
        private readonly Dictionary<string, object> _state;
        private IEntity _lastPet;

        public CompanionHandler(IPlayer Player)
        {
            _player = Player;
            _state = new Dictionary<string, object>();
            _script = Interpreter.GetScript("Companion.py");
            _script.Get("init").Invoke(Player, _state);
        }

        public IEntity Entity => (IEntity)_state["pet"];
        public bool IsActive => Entity != null;

        public Item Item => _player.Inventory.Pet;
        public event OnCompanionChanged CompanionChanged;

        public void Update()
        {
            _script.Get("update").Invoke(_state);
            if (_lastPet != Entity)
            {
                CompanionChanged.Invoke(Item, Entity);
                _lastPet = Entity;
            }
        }
    }
}