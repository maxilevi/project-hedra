/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 03:17 p.m.
 * 
 * To change  template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using System.Drawing;
using System.Numerics;
using System.Collections;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Windowing;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.UI;



namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of Panel.
    /// </summary>
    public class CharacterCreatorUI : Panel, IUpdatable
    {
        private readonly Humanoid _human;
        private readonly Timer _clickTimer;
        private readonly Button _openFolder;
        private float _newRot;
        private ClassDesign _classType;
        
        public CharacterCreatorUI(IPlayer Player) 
        {
            _clickTimer = new Timer(.25f)
            {
                UseTimeScale = false
            };
            var defaultFont = FontCache.GetNormal(12);
            var defaultColor = Color.White;
            
            var bandPosition = new Vector2(0f, .8f);
            var blackBand = new BackgroundTexture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);

            var currentTab = new GUIText(Translation.Create("new_character"), new Vector2(0f, bandPosition.Y), Color.White, FontCache.GetBold(15));

            _openFolder = new Button(new Vector2(0.8f,bandPosition.Y), new Vector2(0.15f,0.05f),
                Translation.Create("character_folder"), Color.White, FontCache.GetNormal(13));
            _openFolder.Click += delegate
            {
                System.Diagnostics.Process.Start($"{AssetManager.AppData}/Characters/");
            };
            
            _human = new Humanoid();
            _human.Model = new HumanoidModel(_human)
            {
                LocalRotation = Vector3.UnitY * -90,
                TargetRotation = Vector3.UnitY * -90,
                Position = Scenes.MenuBackground.PlatformPosition,
                ApplyFog = true,
                Enabled = true
            };
            _human.SearchComponent<DamageComponent>().Immune = true;
            _human.Physics.UseTimescale = false;
            World.RemoveEntity(_human);


            var classes = ClassDesign.AvailableClassNames.Select(S => Translation.Create(S.ToLowerInvariant())).ToArray();
            var classChooser = new OptionChooser(new Vector2(0,.5f), Vector2.Zero, Translation.Create("class"), defaultColor,
                                                          defaultFont, classes, true);
            classChooser.Index = Array.IndexOf(ClassDesign.AvailableClassNames, _human.Class.Name);

            void SetWeapon(object Sender, MouseButtonEventArgs E)
            {
                _classType = ClassDesign.FromString(ClassDesign.AvailableClassNames[classChooser.Index]);
                var rotation = _human.Model.LocalRotation;
                var position = _human.Position = Scenes.MenuBackground.PlatformPosition;

                _human.Model.Dispose();
                _human.Model = new HumanoidModel(_human, _classType.ModelTemplate)
                {
                    Position = position,
                    LocalRotation = rotation,
                    TargetRotation = rotation
                };
                _human.SetWeapon(_classType.StartingItems.First(P => P.Value.IsWeapon).Value.Weapon);
            }

            SetWeapon(null, default);
            
            classChooser.RightArrow.Click += SetWeapon;
            classChooser.LeftArrow.Click += SetWeapon;
            classChooser.CurrentValue.TextColor = defaultColor;
            classChooser.CurrentValue.UpdateText(); 
            
            var nameField = new TextField(new Vector2(0,-.7f), new Vector2(.15f,.03f));
            var createChr = new Button(new Vector2(0f,-.8f), new Vector2(.15f,.05f), Translation.Create("create"), defaultColor, FontCache.GetBold(11));
            createChr.Click += delegate
            {
                for(var i = 0; i < DataManager.PlayerFiles.Length; i++)
                {
                    if (nameField.Text != DataManager.PlayerFiles[i].Name) continue;
                    Player.MessageDispatcher.ShowNotification(Translations.Get("name_exists"), Color.Red, 3f, true);
                    return;
                }

                if(!LocalPlayer.CreatePlayer(nameField.Text, _classType)) return;          
                base.Disable();
                GameManager.Player.UI.CharacterSelector.Enable();
                nameField.Text = string.Empty;    
            };
            
            
            base.AddElement(classChooser);
            base.AddElement(nameField);
            base.AddElement(createChr);
            base.AddElement(blackBand);
            base.AddElement(_openFolder);
            this.AddElement(currentTab);
            base.Disable();

            OnPanelStateChange += PanelStateChange;
            OnEscapePressed += EscapePressed;
            UpdateManager.Add(this);
        }

        private void PanelStateChange(object Sender, PanelState State)
        {
            switch (State)
            {
                case PanelState.Disabled:
                    Scenes.MenuBackground.Creator = false;
                    break;
                case PanelState.Enabled:
                    Scenes.MenuBackground.Creator = true;
                    _openFolder.CanClick = false;
                    _clickTimer.Reset();
                    break;
            }
        }

        private void EscapePressed(object Sender, EventArgs Args)
        {
            base.Disable();
            GameManager.Player.UI.CharacterSelector.Enable();
        }
        
        public void Update()
        {
            _human.Model.Enabled = this.Enabled;
            if (this.Enabled)
            {
                if(_clickTimer.Tick())
                    _openFolder.CanClick = true;
                _human.Update();
                _newRot += Time.IndependentDeltaTime * 30f;
                _human.Model.LocalRotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;
                _human.Model.TargetRotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;
                _human.Position = new Vector3(Scenes.MenuBackground.PlatformPosition.X, _human.Position.Y, Scenes.MenuBackground.PlatformPosition.Z);
                if(_human.Position.Y <= Physics.HeightAtPosition(_human.Position)){
                    _human.Position = new Vector3(_human.Position.X, Physics.HeightAtPosition(_human.Position) + 4, _human.Position.Z);
                }
                _human.IsKnocked = false;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            UpdateManager.Remove(this);
        }
    }
}
