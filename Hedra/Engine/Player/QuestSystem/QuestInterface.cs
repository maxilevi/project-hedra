using System;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestInterface : PlayerInterface
    {
        public override Key OpeningKey => Controls.QuestLog;
        private readonly IPlayer _player;
        
        public QuestInterface(IPlayer Player)
        {
            _player = Player;
        }

        public void Update()
        {
            
        }
        
        public override bool Show { get; set; }
    }
}