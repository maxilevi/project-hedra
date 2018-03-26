﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 03:17 p.m.
 * 
 * To change  template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using System.Drawing;
using OpenTK;
using System.Collections;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of Panel.
	/// </summary>
	public class ChrCreatorUI : Panel
	{
		private const int WeaponCount = 3;
		private Class _classType = Class.Warrior;
		private Humanoid _human;
		private static Vector2 _targetResolution = new Vector2(1024, 578);
		private Timer _clickTimer = new Timer(.25f);
		private Button _openFolder;
		
		public ChrCreatorUI(LocalPlayer Player) 
		{
			Font defaultFont = FontCache.Get(UserInterface.Fonts.Families[0], 12);
			Color defaultColor = Color.White;//Color.FromArgb(255,39,39,39);
			
			Vector2 bandPosition = new Vector2(0f, .8f);
			Texture blackBand = new Texture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LEFT_RIGHT);

		    var currentTab = new GUIText("New Character", new Vector2(0f, bandPosition.Y), Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 15, FontStyle.Bold));

            _openFolder = new Button(new Vector2(0.8f,bandPosition.Y), new Vector2(0.15f,0.05f),
			                           "Character Folder", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 13));
			_openFolder.Click += delegate { System.Diagnostics.Process.Start(AssetManager.AppData + "/Characters/"); };
			
			_human = new Humanoid();
			_human.ClassType = Class.Warrior;
			_human.Model = new HumanModel(_human);
			_human.Model.Rotation = Vector3.UnitY * -90;
			_human.Model.TargetRotation = Vector3.UnitY * -90;
			_human.Physics.UseTimescale = false;
			_human.Removable = false;
			_human.BlockPosition = Scenes.MenuBackground.PlatformPosition + new Vector3(+2,0,0);
			_human.Model.Fog = true;
			_human.Model.Enabled = true;
			
			CoroutineManager.StartCoroutine(Update);
			
			string[] classes = new string[]{"Warrior","Archer","Rogue"};//,"Mage","Necromancer"};
			OptionChooser classChooser = new OptionChooser(new Vector2(0,.5f), Vector2.Zero, "Class", defaultColor,
			                                              defaultFont, classes, true);
						
			_human.Model.SetWeapon(ItemPool.Grab(CommonItems.CommonBronzeSword).Weapon);
			
			OnButtonClickEventHandler setWeapon = delegate {
				_classType = (Class) Enum.Parse(typeof(Class), classes[classChooser.Index]);
				if(_classType == Class.Rogue){
					_human.Model.Model.Dispose();
					_human.Model.Model = AnimationModelLoader.LoadEntity("Assets/Chr/RogueIdle.dae");
					_human.Model.SetWeapon(ItemPool.Grab(CommonItems.CommonBronzeDoubleBlades).Weapon);
					_human.Model.LeftWeapon.Mesh.Scale = Vector3.One * .75f;
					
				}else if(_classType == Class.Archer){
					_human.Model.Model.Dispose();
					_human.Model.Model = AnimationModelLoader.LoadEntity("Assets/Chr/ArcherIdle.dae");
					_human.Model.SetWeapon(ItemPool.Grab(CommonItems.CommonWoodenBow).Weapon);
					_human.Model.LeftWeapon.Mesh.Scale = Vector3.One * .75f;
					
				}else if(_classType == Class.Warrior){
					_human.Model.Model.Dispose();
					_human.Model.Model = AnimationModelLoader.LoadEntity("Assets/Chr/WarriorIdle.dae");
					_human.Model.SetWeapon(ItemPool.Grab(CommonItems.CommonBronzeSword).Weapon);
					_human.Model.LeftWeapon.Mesh.Scale = Vector3.One * .75f;
				}
			};
			
			classChooser.RightArrow.Click += setWeapon;
			classChooser.LeftArrow.Click += setWeapon;
			classChooser.CurrentValue.TextColor = defaultColor;
			classChooser.CurrentValue.Update(); 
			
			#region UI
			TextField nameField = new TextField(new Vector2(0,-.7f), new Vector2(.15f,.03f), this);
			Button createChr = new Button(new Vector2(0f,-.8f), new Vector2(.15f,.05f), "Create", 0, defaultColor, FontCache.Get(AssetManager.Fonts.Families[0], 11, FontStyle.Bold));
			createChr.Click += delegate {
				for(int i = 0; i < DataManager.PlayerFiles.Length; i++){
					if(nameField.Text == DataManager.PlayerFiles[i].Name){
						Player.MessageDispatcher.ShowNotification("NAME ALREADY EXISTS", Color.DarkRed, 3f, true);
						return;
					}
						
				}
				if(!LocalPlayer.CreatePlayer(nameField.Text, _human.Model, _classType)) return;		    
			    base.Disable();
			    Scenes.SceneManager.Game.LPlayer.UI.ChrChooser.Enable();
			    nameField.Text = "";    
			};
			
			
			base.AddElement(classChooser);
			base.AddElement(nameField);
			base.AddElement(createChr);
			base.AddElement(blackBand);
			base.AddElement(_openFolder);
            this.AddElement(currentTab);
			base.Disable();
			#endregion
			
			OnPanelStateChange += delegate(object Sender, PanelState E) {
				if(E == PanelState.Disabled){
					Scenes.MenuBackground.Creator = false;
				}
				if(E == PanelState.Enabled){
					Scenes.MenuBackground.Creator = true;
					_openFolder.Clickable = false;
					_clickTimer.Reset();
				}
			};
			
			OnEscapePressed += delegate {	
				base.Disable(); Scenes.SceneManager.Game.LPlayer.UI.ChrChooser.Enable();
			};
		}
		
		private float _newRot;
		private IEnumerator Update(){
			while(Program.GameWindow.Exists){
				
				_human.Model.Enabled = this.Enabled;
				if(this.Enabled){
					if(_clickTimer.Tick())
						_openFolder.Clickable = true;
					_human.Update();
					_newRot += Time.unScaledDeltaTime * 30f;
					_human.Model.Rotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;
					_human.Model.TargetRotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;
					_human.Model.Idle();
					_human.BlockPosition = Scenes.MenuBackground.PlatformPosition;
					_human.BlockPosition = new Vector3(_human.BlockPosition.X, _human.BlockPosition.Y / Engine.Generation.Chunk.BlockSize, _human.BlockPosition.Z);
				}
				yield return null;
			}
		}
	}
}
