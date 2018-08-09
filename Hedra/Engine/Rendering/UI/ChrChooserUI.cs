/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 06:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of ChrChooserUI.
	/// </summary>
	public class ChrChooserUI : Panel
	{
		private PlayerInformation[] _information;
		private readonly List<Humanoid> _humans;
		private Humanoid _selectedHuman, _previousHuman;
	    private readonly GUIText _level;
	    private readonly GUIText _name;
		private readonly List<UIElement> _dataElements = new List<UIElement>();
		
		public ChrChooserUI(IPlayer Player)
		{
		    _humans = new List<Humanoid>();
            var bandPosition = new Vector2(0f, .8f);
			var blackBand = new Texture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);
			var blackBand2 = new Texture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), -bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);	
			
            var currentTab = new GUIText("Choose a character", new Vector2(0f, bandPosition.Y), Color.White, FontCache.Get(AssetManager.BoldFamily, 15, FontStyle.Bold));

			var newChr = new Button(new Vector2(0.8f,bandPosition.Y), new Vector2(0.15f,0.05f),
			                           "New Character", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 13));
			newChr.Click += delegate { this.Disable(); GameManager.Player.UI.ChrCreator.Enable(); };	
			
			var playBtn = new Button(new Vector2(-.1f, -.8f), Vector2.One, "Load", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 14));
			
			#region PlayBtn
			playBtn.Click += delegate {
				int index = 0;
				for(int i = 0; i < _humans.Count; i++){
					if(_humans[i] == _selectedHuman){
						index = i;
						break;
					}
				}
				
				var information = DataManager.PlayerFiles[index];
				GameManager.MakeCurrent(information);
			};
			#endregion
			
			var deleteButton = new Button(new Vector2(.1f, -.8f), Vector2.One, "Delete", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 14));
			
			#region DeleteButton
			deleteButton.Click += delegate {
				int index = 0;
				for(int i = 0; i < _humans.Count; i++){
					if(_humans[i] == _selectedHuman){
						index = i;
						break;
					}
				}
				System.IO.File.Delete(AssetManager.AppData+"Characters/"+_information[index].Name+".db");
				System.IO.File.Delete(AssetManager.AppData+"Characters/"+_information[index].Name+".db.bak");
				_selectedHuman.Model.Enabled = false;
				_selectedHuman.Position = Vector3.Zero;
			    _selectedHuman.Dispose();
                _humans.Remove(_selectedHuman);
			    _name.Text = String.Empty;
			    _level.Text = String.Empty;
			    _selectedHuman = null;

			};
			#endregion
			
			_name = new GUIText(string.Empty, new Vector2(0, .55f), Color.White, FontCache.Get(AssetManager.BoldFamily, 24, FontStyle.Bold));
			_level = new GUIText(string.Empty, new Vector2(0, .425f), Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 16));
			
			this._dataElements.Add(_name);
			this._dataElements.Add(_level);
			this._dataElements.Add(playBtn);
			this._dataElements.Add(deleteButton);
			for(var i = 0; i < _dataElements.Count; i++){
				this.AddElement(_dataElements[i]);
			}
			this.AddElement(playBtn);
			this.AddElement(newChr);
			this.AddElement(blackBand);
			this.AddElement(blackBand2);
            this.AddElement(currentTab);
			this.Disable();
			
			CoroutineManager.StartCoroutine(UpdateWrapper);
			
			OnPanelStateChange += delegate(object Sender, PanelState E) {
				if(E == PanelState.Disabled)
					Scenes.MenuBackground.Campfire = false;
                if (E == PanelState.Enabled)
                {            
                    ReloadFiles();
                }
			};
			
			OnEscapePressed += delegate { 
				this.Disable(); GameManager.Player.UI.Menu.Enable();
			};
		}
		
		public void ReloadFiles()
        {
		    PlayerInformation[] newInformation = DataManager.PlayerFiles;

		    bool same = true;
		    if (_information != null && _information.Length == newInformation.Length)
		    {

		        for (int k = 0; k < _information.Length; k++)
		        {
		            if (_information[k].Name + _information[k].Class + _information[k].BlockPosition + _information[k].Health + _information[k].Level
		                != newInformation[k].Name + newInformation[k].Class + newInformation[k].BlockPosition + newInformation[k].Health + newInformation[k].Level)
		            {
		                same = false;
		            }
		        }
		        if (same) return;
		    }
		    _information = newInformation;

		    if (_humans != null)
		    {
		        for (int i = 0; i < _humans.Count; i++)
		        {
                    _humans[i].Dispose();
		        }
		    }
            _humans.Clear();

		    for (int i = 0; i < _information.Length; i++)
		    {
		        Humanoid human = new Humanoid();
		        human.Model = new HumanoidModel(human);
		        human.Model.Resize(1.25f * Vector3.One);
		        human.Physics.UseTimescale = false;
		        human.Removable = false;
		        human.Model.Enabled = false;
		        human.Physics.CanCollide = false;
		        _humans.Add(human);
		    }
		    
		    _selectedHuman = null;
		    _previousHuman = null;
		    _level.Text = string.Empty;
		    _name.Text = string.Empty;
				
			for(int i = 0; i < _information.Length; i++){
			    Vector3 offset = this.FireDirection(i, 8f);


                if (_humans[i].Model != null)
					_humans[i].Model.Dispose();
				
				_humans[i].Class = _information[i].Class;
				_humans[i].Model = new HumanoidModel(_humans[i]);
			    _humans[i].Model.Resize(1.25f * Vector3.One);
                _humans[i].BlockPosition = Scenes.MenuBackground.FirePosition + offset;
				_humans[i].Model.Rotation = Physics.DirectionToEuler(-offset.Normalized().Xz.ToVector3());
				_humans[i].Model.TargetRotation = _humans[i].Model.Rotation;
				_humans[i].Model.Enabled = true;
				_humans[i].Name = _information[i].Name;
				_humans[i].Level = _information[i].Level;
			    _humans[i].PlaySpawningAnimation = false;
			    _humans[i].SearchComponent<DamageComponent>().Immune = true;

                foreach (var pair in _information[i].Items)
				{
				    var item = pair.Value;
				    if (pair.Key != PlayerInventory.WeaponHolder || item == null) continue;
				    _humans[i].MainWeapon = item;
				    _humans[i].Model.SetWeapon(_humans[i].MainWeapon.Weapon);
				}
			    _humans[i].Model.LeftWeapon.Scale = (1f / 1.25f) * Vector3.One;
			}
		}
		
				
		public override void OnMouseButtonDown(object Sender, OpenTK.Input.MouseButtonEventArgs E)
		{
		    if (!this.Enabled) return;
		    for(var i = 0; i <_humans.Count; i++){
		        if (_humans[i].Model.Tint != new Vector4(2, 2, 2, 1) || _humans[i] == _selectedHuman ||
		            _humans[i].Model == null) continue;
		        if(_previousHuman != null){
		            for(var k = 0; k <_humans.Count; k++){
		                if (_previousHuman != _humans[k]) continue;
		                Vector3 fPos = Scenes.MenuBackground.FirePosition + this.FireDirection(k, 10);
		                _previousHuman.BlockPosition = new Vector3(fPos.X, _previousHuman.BlockPosition.Y, fPos.Z);
		                _previousHuman.Model.Rotation = new Vector3(0, Physics.DirectionToEuler(this.FireDirection(k, 10).NormalizedFast().Xz.ToVector3()).Y+180, 0);
		                _previousHuman.Model.TargetRotation = new Vector3(0, Physics.DirectionToEuler(this.FireDirection(k, 10).NormalizedFast().Xz.ToVector3()).Y+180, 0);
		            }
		        }
		        _previousHuman = _selectedHuman;
		        _selectedHuman = _humans[i];
		        _name.Text = _selectedHuman.Name;
		        _level.Text = $"{Utils.FirstCharToUpper(_selectedHuman.Class.ToString().ToLowerInvariant())} Level {_selectedHuman.Level}";
		        break;
		    }
		}
		
		private Vector3 FireDirection(int I, float Mult)
		{
		    if (_humans.Count == 1) I = 1;
			return Vector3.TransformPosition(Vector3.UnitX * Mult, Matrix4.CreateRotationY( ( I * 180 / _information.Length - 180 / _information.Length) * Mathf.Radian ));
		}
		
		private IEnumerator UpdateWrapper(){
			while (Program.GameWindow.Exists){
				Update();
				yield return null;
			}
		}

	    public void StopModels()
	    {
	        for (int i = 0; i < _humans.Count; i++)
                _humans[i]?.Model?.StopSound();
        }

		private void Update()
        {		
			//Force update!
			for(int i = 0; i < _humans.Count; i++){
				if(World.Seed != World.MenuSeed){
			        _humans[i].Model.Enabled = false;

				}else{

					int k = i;
					if(_humans[k].MainWeapon != null && _humans[k].MainWeapon.Weapon.InAttackStance)
						_humans[k].Model.Model.BlendAnimation(_humans[k].MainWeapon.Weapon.AttackStanceAnimation);                             
						
					_humans[i].Model.Enabled = true;
                    _humans[i].Update();					
				}
			}

		    for (int j = 0; j < _humans.Count; j++)
		    {
		        Vector3 target = FireDirection(j, 6);
		        _humans[j].Model.Rotation = Physics.DirectionToEuler(target.NormalizedFast().Xz.ToVector3()) + Vector3.UnitY * 180f;
		        _humans[j].Model.TargetRotation = _humans[j].Model.Rotation;
		    }

            if (this.Enabled){
				if(_selectedHuman != null){
					for(int i = 0; i < _dataElements.Count; i++){
						_dataElements[i].Enable();
					}
				}else{
					for(int i = 0; i < _dataElements.Count; i++){
						_dataElements[i].Disable();
					}
				}
				
				Scenes.MenuBackground.Campfire = true;
				GameManager.Player.View.CameraHeight = Vector3.UnitY * 4;
				#region Hover
				Vector2 coords = Mathf.ToNormalizedDeviceCoordinates(Events.EventDispatcher.Mouse.X, Events.EventDispatcher.Mouse.Y);
				coords += new Vector2(1,1);
				coords /= 2;
				for(int i = 0; i <_humans.Count; i++){

                    Vector4 space = Vector4.Transform(new Vector4(_humans[i].Position+Vector3.UnitY,1), DrawManager.FrustumObject.ModelViewMatrix);
					space = Vector4.Transform(space, DrawManager.FrustumObject.ProjectionMatrix);
					Vector2 ndc = (space.Xyz / space.W).Xy + new Vector2(1,1);
					ndc /= 2;
					if( _humans[i].Model.Enabled && Math.Abs(ndc.X - coords.X) < 0.05f && Math.Abs(1-ndc.Y - coords.Y) < 0.125f)
						_humans[i].Model.Tint = new Vector4(2,2,2,1);
					else
						_humans[i].Model.Tint = new Vector4(1,1,1,1);
                }
                #endregion

                #region Move selected human

                if (_selectedHuman != null){
					int i = 0;
					for(int k = 0; k < _humans.Count; k++){
                        if (_selectedHuman == _humans[k]){
							i = k;
							break;
						}
					}
                    Vector3 target = FireDirection(i, 6);
                    if(_selectedHuman.MainWeapon != null)
                        _selectedHuman.MainWeapon.Weapon.InAttackStance = true;
					if( (_selectedHuman.BlockPosition.Xz - Scenes.MenuBackground.FirePosition.Xz).LengthSquared > 4*4)
					{
						_selectedHuman.Physics.Translate(-target.NormalizedFast() * 6f * Time.IndependantDeltaTime);
					}
					
				}
				#endregion
				
				#region Move the previous human
				if(_previousHuman != null){
					int i = 0;
					for(int k = 0; k < _humans.Count; k++){
						if(_previousHuman == _humans[k]){
							i = k;
							break;
						}
					}
					Vector3 backTarget = FireDirection(i, 10);
				    if (_previousHuman.MainWeapon != null)
                        _previousHuman.MainWeapon.Weapon.InAttackStance = false;
                    if ( (_previousHuman.BlockPosition.Xz - Scenes.MenuBackground.FirePosition.Xz - backTarget.Xz).LengthSquared > 1*1){
						_previousHuman.Physics.Translate(backTarget.NormalizedFast() * 6f * Time.IndependantDeltaTime);
                        _previousHuman.Model.Rotation = Physics.DirectionToEuler(backTarget.NormalizedFast().Xz.ToVector3());
                        _previousHuman.Model.TargetRotation = _previousHuman.Model.Rotation;
                    }
					
				}
				#endregion
			}else{
				Scenes.MenuBackground.Campfire = false;
				//Game.LPlayer.View.CameraHeight = OldHeight;
			}
			
		}
	}
}
