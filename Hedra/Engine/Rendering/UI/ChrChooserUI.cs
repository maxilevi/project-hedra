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
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Networking;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Frustum;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.Sound;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of ChrChooserUI.
    /// </summary>
    public class ChrChooserUI : Panel
    {
        public bool ShouldHost { get; set; }
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
            var blackBand = new BackgroundTexture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);
            var blackBand2 = new BackgroundTexture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), -bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);    
            
            var currentTab = new GUIText(Translation.Create("choose_character"), new Vector2(0f, bandPosition.Y), Color.White, FontCache.GetBold(15));

            var newChr = new Button(new Vector2(0.8f,bandPosition.Y), new Vector2(0.15f,0.05f),
                Translation.Create("new_character"), Color.White, FontCache.GetNormal(13));
            newChr.Click += delegate { this.Disable(); GameManager.Player.UI.ChrCreator.Enable(); };    
            
            var playBtn = new Button(new Vector2(-.1f, -.8f), Vector2.One, Translation.Create("load"), Color.White, FontCache.GetNormal(14));
            
            playBtn.Click += delegate 
            {
                GameManager.LoadCharacter(DataManager.PlayerFiles[_humans.IndexOf(_selectedHuman)]);
                if(ShouldHost)
                    Network.Instance.Host();
            };
            
            var deleteButton = new Button(new Vector2(.1f, -.8f), Vector2.One, Translation.Create("delete"), Color.White, FontCache.GetNormal(14));
            
            deleteButton.Click += (O, S) => DeleteSelected();
            
            _name = new GUIText(string.Empty, new Vector2(0, .55f), Color.White, FontCache.GetBold(24));
            _level = new GUIText(string.Empty, new Vector2(0, .425f), Color.White, FontCache.GetNormal(16));
            
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
            
            RoutineManager.StartRoutine(UpdateWrapper);
            
            OnPanelStateChange += delegate(object Sender, PanelState E)
            {
                switch (E)
                {
                    case PanelState.Disabled:
                        Scenes.MenuBackground.Campfire = false;
                        break;
                    case PanelState.Enabled:
                        ReloadFiles();
                        break;
                }
            };
            
            OnEscapePressed += delegate { 
                this.Disable(); GameManager.Player.UI.Menu.Enable();
            };
        }

        private void DeleteSelected()
        {
            var index = _humans.IndexOf(_selectedHuman);
            DataManager.DeleteCharacter(_information[index]);
            this.ReloadFiles();
        }
        
        public void ReloadFiles()
        {
            var newInformation = DataManager.PlayerFiles.Take(GameSettings.MaxCharacters).ToArray();

            var same = true;
            if (_information != null && _information.Length == newInformation.Length)
            {

                for (var k = 0; k < _information.Length; k++)
                {
                    if (_information[k].Name + _information[k].Class + _information[k].Health + _information[k].Level
                        != newInformation[k].Name + newInformation[k].Class + newInformation[k].Health + newInformation[k].Level)
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

            for (var i = 0; i < _information.Length; i++)
            {
                var human = new Humanoid();
                human.Model = new HumanoidModel(human);
                human.Physics.UseTimescale = false;
                human.Removable = false;
                human.Model.Enabled = false;
                human.Physics.CollidesWithStructures = false;
                _humans.Add(human);
            }
            
            _selectedHuman = null;
            _previousHuman = null;
            _level.Text = string.Empty;
            _name.Text = string.Empty;
                
            for(int i = 0; i < _information.Length; i++){
                Vector3 offset = this.FireDirection(i, 6.4f);


                if (_humans[i].Model != null)
                    _humans[i].Model.Dispose();
                
                _humans[i].Class = _information[i].Class;
                _humans[i].Model = new HumanoidModel(_humans[i]);
                _humans[i].BlockPosition = Scenes.MenuBackground.FirePosition + offset;
                _humans[i].Model.LocalRotation = Physics.DirectionToEuler(-offset.Normalized().Xz.ToVector3());
                _humans[i].Model.TargetRotation = _humans[i].Model.LocalRotation;
                _humans[i].Model.Enabled = true;
                _humans[i].Name = _information[i].Name;
                _humans[i].Level = _information[i].Level;
                _humans[i].PlaySpawningAnimation = false;
                _humans[i].SearchComponent<DamageComponent>().Immune = true;
                _humans[i].Physics.TargetPosition = new Vector3(
                    _humans[i].BlockPosition.X,
                    Physics.HeightAtPosition(_humans[i].BlockPosition),
                    _humans[i].BlockPosition.Z
                );
                _humans[i].Model.Position = _humans[i].Physics.TargetPosition;
                foreach (var pair in _information[i].Items)
                {
                    var item = pair.Value;
                    if (item == null) continue;
                    AddItemToHuman(_humans[i], pair.Key, item);
                }
            }
        }

        private static void AddItemToHuman(Humanoid Human, int Index, Item Object)
        {
            switch (Index)
            {
                case PlayerInventory.WeaponHolder:
                    Human.MainWeapon = Object;
                    Human.SetWeapon(Human.MainWeapon.Weapon);
                    break;
                case PlayerInventory.HelmetHolder:
                    Human.SetHelmet(Object.Helmet);
                    break;
                case PlayerInventory.ChestplateHolder:
                    Human.SetChestplate(Object.Chestplate);
                    break;
                case PlayerInventory.PantsHolder:
                    Human.SetPants(Object.Pants);
                    break;
                case PlayerInventory.BootsHolder:
                    Human.SetBoots(Object.Boots);
                    break;
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
                        Vector3 fPos = Scenes.MenuBackground.FirePosition + this.FireDirection(k, 8);
                        _previousHuman.BlockPosition = new Vector3(fPos.X, _previousHuman.BlockPosition.Y, fPos.Z);
                        _previousHuman.Model.LocalRotation = new Vector3(0, Physics.DirectionToEuler(this.FireDirection(k, 8).NormalizedFast().Xz.ToVector3()).Y+180, 0);
                        _previousHuman.Model.TargetRotation = new Vector3(0, Physics.DirectionToEuler(this.FireDirection(k, 8).NormalizedFast().Xz.ToVector3()).Y+180, 0);
                    }
                }
                _previousHuman = _selectedHuman;
                _selectedHuman = _humans[i];
                _name.Text = _selectedHuman.Name;
                _level.Text = $"{Translations.Get(_selectedHuman.Class.ToString().ToLowerInvariant())} {Translations.Get("level")} {_selectedHuman.Level}";
                break;
            }
        }
        
        private Vector3 FireDirection(int I, float Mult)
        {
            var angle = 0f;
            switch (_information.Length)
            {
                case 1:
                    angle = 0;
                    break;
                case 2:
                    angle = (I * 90 - 45) * Mathf.Radian;
                    break;
                case 3:
                    angle = (I * 60 - 60) * Mathf.Radian;
                    break;
                case 4:
                    angle = (I * 45 - 70) * Mathf.Radian;
                    break;
            }
            return Vector3.TransformPosition(Vector3.UnitX * Mult, Matrix4.CreateRotationY((float)angle));
        }
        
        private IEnumerator UpdateWrapper()
        {
            while (Program.GameWindow.Exists)
            {
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
                        _humans[k].Model.BlendAnimation(_humans[k].MainWeapon.Weapon.AttackStanceAnimation);                             
                        
                    _humans[i].Model.Enabled = (_humans[i].Model.ModelPosition.Xz - _humans[i].BlockPosition.Xz).LengthFast < 8;
                    _humans[i].Update();                    
                }
            }

            for (int j = 0; j < _humans.Count; j++)
            {
                Vector3 target = FireDirection(j, 4.8f);
                _humans[j].Model.LocalRotation = Physics.DirectionToEuler(target.NormalizedFast().Xz.ToVector3()) + Vector3.UnitY * 180f;
                _humans[j].Model.TargetRotation = _humans[j].Model.LocalRotation;
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
                var size = GUITexture.Adjust(new Vector2(0.05f, 0.25f));
                for(int i = 0; i <_humans.Count; i++){

                    Vector4 space = Vector4.Transform(new Vector4(_humans[i].Position + Vector3.UnitY * 6f,1), Culling.ModelViewMatrix);
                    space = Vector4.Transform(space, Culling.ProjectionMatrix);
                    Vector2 ndc = (space.Xyz / space.W).Xy + new Vector2(1,1);
                    ndc /= 2;
                    if (_humans[i].Model.Enabled && Math.Abs(ndc.X - coords.X) < size.X && Math.Abs(1 - ndc.Y - coords.Y) < size.Y)
                    {
                        if( (_humans[i].Model.Tint.Xyz - new Vector3(2, 2, 2)).LengthFast > 0.05f)
                            SoundPlayer.PlayUISound(SoundType.ButtonClick);
                        _humans[i].Model.Tint = new Vector4(2, 2, 2, 1);
                    }
                    else
                    {
                        _humans[i].Model.Tint = new Vector4(1, 1, 1, 1);
                    }
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
                    Vector3 target = FireDirection(i, 4.8f);
                    if(_selectedHuman.MainWeapon != null)
                        _selectedHuman.MainWeapon.Weapon.InAttackStance = true;
                    if( (_selectedHuman.BlockPosition.Xz - Scenes.MenuBackground.FirePosition.Xz).LengthSquared > 4*4)
                    {
                        _selectedHuman.Physics.Translate(-target.NormalizedFast() * 6f * Time.IndependentDeltaTime);
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
                        _previousHuman.Physics.Translate(backTarget.NormalizedFast() * 6f * Time.IndependentDeltaTime);
                        _previousHuman.Model.LocalRotation = Physics.DirectionToEuler(backTarget.NormalizedFast().Xz.ToVector3());
                        _previousHuman.Model.TargetRotation = _previousHuman.Model.LocalRotation;
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
