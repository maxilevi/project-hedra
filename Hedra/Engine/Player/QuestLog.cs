/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/01/2017
 * Time: 03:00 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using System.Drawing;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.QuestSystem.Objectives;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Description of QuestLog.
    /// </summary>
    public class QuestLog
    {
        private readonly Texture _chainIcon;
        private Objective _currentQuest;
        private readonly Panel _inPanel;
        private readonly LocalPlayer _player;
        private readonly GUIText _questDescription;
        private bool _show;
        private readonly Vector2 _targetResolution = new Vector2(1024, 578);
        private readonly bool _enabled = true;

        public bool Show
        {
            get { return _show; }
            set
            {
                return;
                if (GameManager.IsLoading || !_enabled || _show == value)
                    return;

                SoundManager.PlayUISound(SoundType.OnOff, 1.0f, 0.6f);
                _show = value;

                if (value)
                {
                    if (_player.UI.GamePanel.Enabled) _player.UI.GamePanel.QuestLogMsg.Disable();
                    _inPanel.Enable();
                }
                else
                {
                    if (_player.UI.GamePanel.Enabled) _player.UI.GamePanel.QuestLogMsg.Enable();
                    _inPanel.Disable();
                }
            }
        }

        public QuestLog(LocalPlayer Player)
        {
            _player = Player;
            _inPanel = new Panel();

            var background = new Texture(Graphics2D.LoadFromAssets("Assets/UI/QuestLog.png"), Vector2.Zero,
                Mathf.ScaleGUI(_targetResolution, Vector2.One));

            _chainIcon = new Texture(0, Mathf.ScaleGUI(_targetResolution, new Vector2(0.7f, .325f)),
                Mathf.ScaleGUI(_targetResolution, new Vector2(0.07f, 0.13f) * 2.9f))
            {
                TextureElement = {Flipped = true}
            };

            _questDescription = new GUIText("QUEST DESCRIPTION",
                Mathf.ScaleGUI(_targetResolution, new Vector2(0.7f, -0.3f)),
                Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));

            _inPanel.AddElement(background);
            _inPanel.AddElement(_questDescription);
            _inPanel.AddElement(_chainIcon);
            _inPanel.OnPanelStateChange += this.OnPanelStateChange;
            _inPanel.Disable();
        }

        public void Update()
        {
            if (Show && World.QuestManager.Quest != null)
                _chainIcon.TextureElement.TextureId = World.QuestManager.Quest.QuestLogIcon;

            if (_currentQuest != World.QuestManager.Quest && Show)
            {
                _currentQuest = World.QuestManager.Quest;
                this.UpdateText();
            }
        }

        public void OnPanelStateChange(object Sender, PanelState State)
        {
            if (State == PanelState.Enabled) this.UpdateText();
        }

        public void UpdateText()
        {
            const int characterLimit = 32;

            string desc = Utils.FitString(World.QuestManager.Quest.Description, characterLimit);
            _questDescription.Text = desc.Length == 0 ? World.QuestManager.Quest.Description : desc;
        }
    }
}