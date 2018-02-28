/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:15 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using System.Reflection;
using System.Linq;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of SkillsBar.
	/// </summary>
	public class SkillsBar : EventListener
	{
		public Skill[] Skills;
		public Vector2[] SkillPositions = new Vector2[4];
		public Skill W1, W2;
		public LocalPlayer Player;
		public Vector2 SkillScale;
		private static Vector2 _targetResolution = new Vector2(1024, 576);
		private EntityMesh _berryMesh;
		private Texture _berryIcon;
		private GUIText _berryCount;
		private Vector2 _berryIconPosition = Mathf.ScaleGUI(_targetResolution, new Vector2(.357f,-.9f));
		private Vector2 _berryIconScale = Mathf.ScaleGUI(new Vector2(1000,1000), new Vector2(.055f,.05f));
	    private Skill _followingSkill;
	    private Vector2 _originalSkillPosition;

        public SkillsBar(LocalPlayer Player){
			this._berryMesh = EntityMesh.FromVertexData(AssetManager.PlyLoader("Assets/Items/Berry.ply", Vector3.One * 4f, Vector3.UnitY*2, Vector3.Zero));
			this.Player = Player;
			this.SkillScale = Mathf.ScaleGUI(new Vector2(1000,1000), new Vector2(.055f,.0495f));
			
			this.SkillPositions[0] = Mathf.ScaleGUI(_targetResolution, new Vector2(-.3575f,-.9f));
			this.SkillPositions[1] = Mathf.ScaleGUI(_targetResolution, new Vector2(-.236f,-.9f));
			this.SkillPositions[2] = Mathf.ScaleGUI(_targetResolution, new Vector2(-.1195f,-.9f));
			this.SkillPositions[3] = Mathf.ScaleGUI(_targetResolution, new Vector2(-.0025f,-.9f));
			
			for(int i = 0; i < SkillPositions.Length; i++){
				SkillPositions[i] = new Vector2(SkillPositions[i].X, -.9f);
			}
			
			Type[] SkillsTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetLoadableTypes()).Where(type => type.IsSubclassOf(typeof(Skill))).Where( Type => Type != typeof(WeaponAttack)).ToArray();
			this.Skills = new Skill[SkillsTypes.Length];
			for(int i = 0; i < Skills.Length; i++){
				this.Skills[i] = (Skill) Activator.CreateInstance(SkillsTypes[i], Vector2.Zero, SkillScale, Player.UI.GamePanel, Player);
				this.Skills[i].Active = false;
			}
			
			W1 = new WeaponAttack(WeaponAttack.AttackType.SLASH, Mathf.ScaleGUI(_targetResolution, new Vector2(.118f,-.9f)), Mathf.ScaleGUI(new Vector2(1000,1000), new Vector2(.055f,-.05f)), Player.UI.GamePanel, Player);
			W2 = new WeaponAttack(WeaponAttack.AttackType.LUNGE, Mathf.ScaleGUI(_targetResolution, new Vector2(.2366f,-.9f)), Mathf.ScaleGUI(new Vector2(1000,1000), new Vector2(.055f,-.05f)), Player.UI.GamePanel, Player);
			//.3575f
			Vector2 BarScale = Mathf.ScaleGUI(new Vector2(800,600), new Vector2(0.45f, 0.0875f));
			Texture SBar = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Skillsbar.png"), new Vector2(0f,-0.9125f), BarScale);
			SBar.Scale = new Vector2(BarScale.X / 6f * 7f, BarScale.Y);
			Player.UI.GamePanel.AddElement(SBar);
			
			W1.Position = new Vector2(W1.Position.X, -.9f);
			W2.Position = new Vector2(W2.Position.X, -.9f);
			
			this._berryIcon = new Texture(0, _berryIconPosition, _berryIconScale);
			this._berryIcon.TextureElement.IdPointer = () => DrawBerry();
			this._berryIcon.Position = new Vector2(_berryIconPosition.X, -.9f);
            this._berryIcon.TextureElement.Fxaa = true;

            this._berryCount = new GUIText("1", Vector2.Zero, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 8));
			this._berryCount.Position = new Vector2(_berryIconPosition.X + this._berryIcon.Scale.X * .5f, -.925f);
			
			this.Player.UI.GamePanel.AddElement(this._berryIcon);
			this.Player.UI.GamePanel.AddElement(this._berryCount);
			CoroutineManager.StartCoroutine(Update);
		}
		
		private IEnumerator Update(){
			while(Program.GameWindow.Exists){
				if(Player.Inventory.Items[Inventory.FoodHolder] != null && Player.Inventory.Items[Inventory.FoodHolder].Info.Damage > 0 && Player.UI.GamePanel.Enabled){
					this._berryIcon.Enable();
					this._berryCount.Text = Player.Inventory.Items[Inventory.FoodHolder].Info.Damage.ToString();
					this._berryCount.Position = new Vector2(_berryIconPosition.X + 0.0375f, -.95f);
				}else{
					this._berryIcon.Disable();
					this._berryCount.Disable();
				}
				yield return null;
			}
		}
		
		private uint DrawBerry(){
			Player.UI.DrawPreview(_berryMesh, UserInterface.InventoryFbo);
			return UserInterface.InventoryFbo.TextureID[0];
		}
		
		public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(Player.SkillSystem.Show)
			{
				Vector2 Coords = Mathf.ToNormalizedDeviceCoordinates(e.Mouse.X, e.Mouse.Y);
				
				for(int i = 0; i < Skills.Length; i++){
					if(!(Skills[i] is WeaponAttack) && Skills[i].Active){
						if(Skills[i].Position.Y + Skills[i].Scale.Y > -Coords.Y && Skills[i].Position.Y - Skills[i].Scale.Y < -Coords.Y 
				   			&& Skills[i].Position.X + Skills[i].Scale.X > Coords.X && Skills[i].Position.X - Skills[i].Scale.X < Coords.X ){
							
							#region RIGHT-CLICK
							if(e.Button == MouseButton.Right){
								bool IsInSkillBar = false;
								for(int j = 0; j < SkillPositions.Length; j++){
									if(SkillPositions[j] == Skills[i].Position){
										IsInSkillBar = true;
										break;
									}
								}
								if(!IsInSkillBar){
									bool[] SpacesUsed = new bool[4];
									for(int j = 0; j < SkillPositions.Length; j++){
										for(int k = 0; k < Skills.Length; k++){
											if(Skills[k].Position == SkillPositions[j]){
												SpacesUsed[j] = true;
												break;
											}
										}
									}
									for(int j = 0; j < SpacesUsed.Length; j++){
										if(!SpacesUsed[j]){
											Skills[i].Position = SkillPositions[j];
											Skills[i].UseMask = true;
											break;
										}
									}
								}else{
									Skills[i].Position = Vector2.Zero;
									Skills[i].UseMask = false;
									Player.SkillSystem.UpdateBag();
								}
								return;
							}
							#endregion
							if( this._followingSkill == null || (this._followingSkill != null && this._followingSkill != Skills[i])){
								
								if(this._followingSkill != null)
								{
									bool WasSetted = false;
									for(int j = 0; j < SkillPositions.Length; j++){
										if(Skills[i].Position == SkillPositions[j]){
											this._followingSkill.UseMask = true;
											for(int k = 0; k < SkillPositions.Length; k++){
												if(SkillPositions[k] == _originalSkillPosition){
													this.Skills[i].UseMask = true;
													break;
												}else{
													this.Skills[i].UseMask = false;
												}
											}
											WasSetted = true;
											break;
										}else{
											this._followingSkill.UseMask = false;
										}
									}
									
									this._followingSkill.Position = Skills[i].Position;
									this._followingSkill = Skills[i];
									this._followingSkill.Position = this._originalSkillPosition;
									if(WasSetted)
										this._followingSkill = null;
									return;
								}else{
									this._followingSkill = Skills[i];
									this._originalSkillPosition = Skills[i].Position;
									return;
								}
								
							}
						}
					}
				}
				
				for(int i = 0; i < SkillPositions.Length; i++){
					if(SkillPositions[i].Y + Skills[0].Scale.Y > -Coords.Y && SkillPositions[i].Y - Skills[0].Scale.Y < -Coords.Y 
			   			&& SkillPositions[i].X + Skills[0].Scale.X > Coords.X && SkillPositions[i].X - Skills[0].Scale.X < Coords.X ){
						
						if(this._followingSkill != null){
							this._followingSkill.Position = SkillPositions[i];
							this._originalSkillPosition = Vector2.Zero;
							this._followingSkill.UseMask = true;
							this._followingSkill = null;
							this.SettedUp = true;
							return;
						}
					}
				}
				
				if(Player.SkillSystem.BagPosition.Y + Player.SkillSystem.BagScale.Y > -Coords.Y && Player.SkillSystem.BagPosition.Y - Player.SkillSystem.BagScale.Y < -Coords.Y 
				   			&& Player.SkillSystem.BagPosition.X + Player.SkillSystem.BagScale.X > Coords.X && Player.SkillSystem.BagPosition.X - Player.SkillSystem.BagScale.X < Coords.X ){
					
					if(this._followingSkill != null){
						Player.SkillSystem.UpdateBag();
						this._followingSkill = null;
					}
					return;
				}
				
				if(this._followingSkill != null){
					this._followingSkill.Position = this._originalSkillPosition;
					this._followingSkill = null;
					this._originalSkillPosition = Vector2.Zero;
				}
			}
			else
			{
				if(!Player.CanInteract || Player.Knocked || Player.IsDead || Player.IsUnderwater || Player.IsSwimming || Player.IsGliding || Player.Inventory.Show || Player.SkillSystem.Show || GameSettings.Paused) return;
				
				if(e.Button == MouseButton.Left){
					if(W1.MeetsRequirements(this, 0)){
						W1.Cooldown = W1.MaxCooldown;
						W1.KeyDown();
					}
				}else if(e.Button == MouseButton.Right){
					if(W2.MeetsRequirements(this, 0)){
						W2.Cooldown = W2.MaxCooldown;
						W2.KeyDown();
					}
				}
			}
		}
		
		public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(e.Button == MouseButton.Left){
				//if(W1.MeetsRequirements(this, 0)){
				//	W1.Cooldown = W1.MaxCooldown;
					W1.KeyUp();
				//}
			}else if(e.Button == MouseButton.Right){
				//if(W2.MeetsRequirements(this, 0)){
				//	W2.Cooldown = W2.MaxCooldown;
					W2.KeyUp();
				//}
			}
		}
		
		public override void OnMouseMove(object sender, MouseMoveEventArgs e)
		{
			if(_followingSkill != null)
			{
				_followingSkill.Position = Mathf.ToNormalizedDeviceCoordinates(e.X, Constants.HEIGHT-e.Y);
				_followingSkill.UseMask = false;
				if(!Player.SkillSystem.Show)
					_followingSkill.Position = _originalSkillPosition;
			}
		}

		public override void OnKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			if(!Player.CanInteract || Player.Knocked || Player.Movement.IsJumping || Player.IsDead || Player.IsSwimming || Player.IsUnderwater || Player.IsGliding || Player.Inventory.Show || Player.SkillSystem.Show || GameSettings.Paused) return;
			
			string KeyText = e.Key.ToString().ToLowerInvariant();
			
			if(KeyText.Contains("number")){
				int KeyIndex = Int32.Parse(KeyText.Substring(KeyText.Length-1,1)) - 1;
				
				if(KeyIndex >= SkillPositions.Length || KeyIndex < 0)
					return;
				
				int Index = 0;
				for(int i = 0; i < Skills.Length; i++){
					if(Skills[i].Position == SkillPositions[KeyIndex])
						Index = i;
				}
				
				if(Skills[Index] != null){
					Skills[Index].KeyUp();
				}
			}
		}
		
		public override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
		{
			if(!Player.CanInteract || Player.Movement.IsJumping || Player.Knocked || Player.IsDead || Player.IsSwimming || Player.IsUnderwater || Player.IsGliding || Player.Inventory.Show || Player.SkillSystem.Show || GameSettings.Paused) return;
			
			string KeyText = e.Key.ToString().ToLowerInvariant();
			
			if(KeyText.Contains("number")){
				int KeyIndex = Int32.Parse(KeyText.Substring(KeyText.Length-1,1)) - 1;
				
				if(KeyIndex >= SkillPositions.Length || KeyIndex < 0)
					return;
				
				int Index = 0;
				for(int i = 0; i < Skills.Length; i++){
					if(Skills[i].Position == SkillPositions[KeyIndex])
						Index = i;
				}
				
				if(Skills[Index] != null && Skills[Index].MeetsRequirements(this, AbilitiesBeingCasted())){// && Player.IsAttacking == false){
					Sound.SoundManager.PlaySound(Sound.SoundType.ButtonClick, Player.Position, false, 1f, 0.5f);
					Skills[Index].Cooldown = Skills[Index].MaxCooldown;
					Player.Mana -= Skills[Index].ManaCost;
					Skills[Index].KeyDown();
				}else{
					Player.MessageDispatcher.ShowNotification("YOU CANNOT CAST THIS ABILITY",Color.DarkRed, 3f, false);
				}
			}
			
		}
		
		private int AbilitiesBeingCasted(){
			int Count = 0; 
			for(int i = 0; i < Skills.Length; i++){
				if(Skills[i].Casting)
					Count++;
			}
			return Count;
		}
		
		public byte[] Save(){
			string Save0="null",Save1="null",Save2="null",Save3="null";
			for(int i = 0; i < Skills.Length; i++){
				if(Skills[i].Position == SkillPositions[0])
					Save0 = Skills[i].GetType().ToString();
				if(Skills[i].Position == SkillPositions[1])
					Save1 = Skills[i].GetType().ToString();
				if(Skills[i].Position == SkillPositions[2])
					Save2 = Skills[i].GetType().ToString();
				if(Skills[i].Position == SkillPositions[3])
					Save3 = Skills[i].GetType().ToString();
			}
			return System.Text.Encoding.ASCII.GetBytes(Save0+"|"+Save1+"|"+Save2+"|"+Save3);
		}
		
		public bool SettedUp = false;
		public SkillsBar Load(byte[] Data){
			if(Data.Length == 4) return this; // Old format
			string SaveData = System.Text.Encoding.ASCII.GetString(Data);
			string[] Save = SaveData.Split('|');
			SettedUp = false;
			for(int i = 0; i < Skills.Length; i++){
				Skills[i].Position = Vector2.Zero;
				Skills[i].UseMask = false;
				Skills[i].Active = false;
				if(Skills[i].GetType().ToString() == Save[0].Trim('|')){
					Skills[i].Position = SkillPositions[0];
					Skills[i].UseMask = true;
					Skills[i].Active = true;
					SettedUp = true;
				}
				if(Skills[i].GetType().ToString() == Save[1].Trim('|')){
					Skills[i].Position = SkillPositions[1];
					Skills[i].UseMask = true;
					Skills[i].Active = true;
					SettedUp = true;
				}
				if(Skills[i].GetType().ToString() == Save[2].Trim('|')){
					Skills[i].Position = SkillPositions[2];
					Skills[i].UseMask = true;
					Skills[i].Active = true;
					SettedUp = true;
				}
				if(Skills[i].GetType().ToString() == Save[3].Trim('|')){
					Skills[i].Position = SkillPositions[3];
					Skills[i].Active = true;
					Skills[i].UseMask = true;
					SettedUp = true;
				}	
			}
			return this;
		}
	}
}