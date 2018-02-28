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

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Chat.
	/// </summary>
	public class Chat : EventListener
	{
		public bool Focused {get; set;}
		private LocalPlayer Player;
		private TextField CommandLine;
		private GUIText TextBox;
		private Vector2 TargetResolution = new Vector2(1024, 578);
		private Vector2 TextBoxPosition = new Vector2(-0.95f, -.65f);
		private Panel InPanel = new Panel();
		private string LastInput;
		
		public Chat(LocalPlayer Player){
			this.Player = Player;
			Vector2 BarPosition = new Vector2(-0.95f, -0.75f);
			this.CommandLine = new TextField(BarPosition + Vector2.UnitX * .225f, new Vector2(.225f,.02f), InPanel, false);
			this.TextBox = new GUIText("", TextBoxPosition, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 10));
			InPanel.AddElement(this.TextBox);
			InPanel.AddElement(this.CommandLine);
			InPanel.Disable();
		}
		
		public void Update(){
			if(!CommandLine.InFocus && Focused && Show)
				CommandLine.InFocus = true;
		}
		
		public override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
		{
			if(Focused && e.Key == Key.Up && LastInput != null){
				this.CommandLine.Text = LastInput;
			}
		}
		
		public void PushText(){

			if(CommandLine.Text.Length >= 1 && CommandLine.Text[0] == '/'){
				if(!CommandManager.ProcessCommand(CommandLine.Text, Player))
					this.AddLine("Unkown command.");
				else
					SoundManager.PlaySound(SoundType.NotificationSound, Player.Position, false, 1f, 1f);
				LastInput = CommandLine.Text;
			}else{
				if(CommandLine.Text != ""){
					//It's normal text
					LastInput = CommandLine.Text;
					string OutText = Player.Name+": "+WordFilter.Filter(CommandLine.Text);
					this.AddLine(OutText);
					Networking.NetworkManager.SendChatMessage(OutText);
				}
			}
			CommandLine.Text = "";
			this.LoseFocus();
			
		}
		
		public void AddLine(string NewLine){
			string[] Lines = TextBox.Text.Split( Environment.NewLine.ToCharArray() );
			int LineCount = 0;
			for(int i = 0; i < Lines.Length; i++){
				if(Lines[i] != "" && Lines[i] != Environment.NewLine)
					LineCount++;
			}
			if(LineCount == 7){
				StringBuilder NewText = new StringBuilder();
				int k = 0;
				for(int i = 0; i < Lines.Length; i++){
					if(Lines[i] != "" && Lines[i] != Environment.NewLine){
						if(k != 0)
							NewText.AppendLine( Lines[i].Replace(Environment.NewLine, "") );
						k++;
					}
				}
				TextBox.Text = NewText.ToString() + NewLine;
			}else{
				TextBox.Text = TextBox.Text + Environment.NewLine + NewLine; 
			}
			Lines = TextBox.Text.Split( Environment.NewLine.ToCharArray() );
			string LongestLine = "";
			for(int i = 0; i < Lines.Length; i++){
				if(Lines[i].Length > LongestLine.Length)
					LongestLine = Lines[i];
			}
			TextBox.Position = TextBoxPosition + TextBox.Scale;
		}
		
		public void Clear(){
			TextBox.Text = "";
		}
		
		public void Focus(){
			if(Player.SkillSystem.Show || Player.Inventory.Show || !Player.UI.GamePanel.Enabled) return;
			Player.CanInteract = false;
			Player.View.Check = false;
			Player.View.LockMouse = false;
			Player.UI.GamePanel.Cross.Disable();
			CommandLine.Enable();
			CommandLine.InFocus = true;
			Focused = true;
			UpdateManager.CursorShown = true;
			CommandLine.Text = "";
		}
		
		public void LoseFocus(){
		    if (Focused)
		    {
		        Player.CanInteract = true;
		        Player.View.Check = true;
		        Player.View.LockMouse = true;
		    }
		    Player.UI.GamePanel.Cross.Enable();
			CommandLine.Disable();
			CommandLine.InFocus = false;
			Focused = false;
			UpdateManager.CursorShown = false;
			UpdateManager.CenterMouse();
		}
		
		private bool m_Show;
		public bool Show{
			get{ return m_Show; }
			set{
				#if !DEBUG
				return;
				#endif
				m_Show = value;
				if(m_Show && GameSettings.ShowChat){
					InPanel.Enable();
				}else{
					InPanel.Disable();
					TextBox.Disable();
				}
			}
		}
	}
}
