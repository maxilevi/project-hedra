/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/02/2017
 * Time: 11:56 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Management;
using Hedra.Engine.Sound;
using Hedra.Engine.Events;
using Hedra.Engine.Game;

namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of Chat.
    /// </summary>
    public class Chat
    {
        public bool Focused { get; set ;}
        private readonly IPlayer _player;
        private readonly TextField _commandLine;
        private readonly GUIText _textBox;
        private readonly Vector2 _textBoxPosition = new Vector2(-0.95f, -.65f);
        private readonly Panel _inPanel = new Panel();
        private string _lastInput;
        private bool _show;

        public Chat(LocalPlayer Player){
            this._player = Player;
            var barPosition = new Vector2(-0.95f, -0.75f);
            this._commandLine = new TextField(barPosition + Vector2.UnitX * .225f, new Vector2(.225f,.02f), _inPanel, false);
            this._textBox = new GUIText(string.Empty, _textBoxPosition, Color.White, FontCache.Get(AssetManager.NormalFamily, 10));
            _inPanel.AddElement(this._textBox);
            _inPanel.AddElement(this._commandLine);
            _inPanel.Disable();
            EventDispatcher.RegisterKeyDown(this, this.OnKeyDown);
        }
        
        public void Update(){
            if(!_commandLine.InFocus && Focused && Show)
                _commandLine.InFocus = true;
        }
        
        public void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
            if(Focused && EventArgs.Key == Key.Up && _lastInput != null){
                this._commandLine.Text = _lastInput;
            }
        }
        
        public void PushText()
        {

            if(_commandLine.Text.Length >= 1 && _commandLine.Text[0] == '/')
            {
                if (CommandManager.ProcessCommand(_commandLine.Text, _player, out string response))
                    SoundManager.PlaySound(SoundType.NotificationSound, _player.Position);
                this.AddLine(response);
                _lastInput = _commandLine.Text;
            }
            else
            {
                if(_commandLine.Text != string.Empty)
                {
                    _lastInput = _commandLine.Text;
                    var outText = _player.Name+": "+WordFilter.Filter(_commandLine.Text);
                    this.AddLine(outText);
                    Networking.NetworkManager.SendChatMessage(outText);
                }
            }
            _commandLine.Text = string.Empty;
            this.LoseFocus();
            
        }
        
        public void AddLine(string NewLine)
        {
            var lines = _textBox.Text.Split( Environment.NewLine.ToCharArray() );
            var lineCount = 0;
            foreach (var line in lines)
            {
                if(line != string.Empty && line != Environment.NewLine)
                    lineCount++;
            }
            if(lineCount == 7)
            {
                var newText = new StringBuilder();
                var k = 0;
                foreach (var line in lines)
                {
                    if (line == string.Empty || line == Environment.NewLine) continue;
                    
                    if(k != 0) newText.AppendLine( line.Replace(Environment.NewLine, string.Empty) );
                    k++;
                }
                _textBox.Text = newText + NewLine;
            }
            else
            {
                _textBox.Text = _textBox.Text + Environment.NewLine + NewLine; 
            }
            lines = _textBox.Text.Split( Environment.NewLine.ToCharArray() );
            var longestLine = string.Empty;
            foreach (var line in lines)
            {
                if(line.Length > longestLine.Length)
                    longestLine = line;
            }
            _textBox.Position = _textBoxPosition + _textBox.Scale;
        }
        
        public void Clear()
        {
            _textBox.Text = string.Empty;
        }
        
        public void Focus()
        {
            _player.CanInteract = false;
            _player.View.CaptureMovement = false;
            _player.View.LockMouse = false;
            _player.UI.GamePanel.Cross.Disable();
            _commandLine.Enable();
            _commandLine.InFocus = true;
            Focused = true;
            UpdateManager.CursorShown = true;
            _commandLine.Text = string.Empty;
        }
        
        public void LoseFocus()
        {
            if (Focused)
            {
                _player.CanInteract = true;
                _player.View.CaptureMovement = true;
                _player.View.LockMouse = true;
            }
            _player.UI.GamePanel.Cross.Enable();
            _commandLine.Disable();
            _commandLine.InFocus = false;
            Focused = false;
            UpdateManager.CursorShown = false;
            UpdateManager.CenterMouse();
        }
        
        public bool Show
        {
            get => _show;
            set
            {
                _show = value;
                if (_show && GameSettings.ShowChat)
                {
                    _inPanel.Enable();
                }
                else
                {
                    _inPanel.Disable();
                    _textBox.Disable();
                }
            }
        }
    }
}
