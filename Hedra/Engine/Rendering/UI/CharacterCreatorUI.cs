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
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Windowing;
using Hedra.EntitySystem;
using Hedra.Framework;
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
        private CustomizationData _customization;
        private ColorPicker _secondHairColorPicker;

        public CharacterCreatorUI(IPlayer Player) 
        {
            _customization = new CustomizationData();
            _clickTimer = new Timer(.25f)
            {
                UseTimeScale = false
            };
            var defaultFont = FontCache.GetNormal(12);
            var defaultColor = Color.White;
            
            var bandPosition = new Vector2(0f, .8f);
            var blackBand = new BackgroundTexture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, UserInterface.BlackBandSize, GradientType.LeftRight);

            var currentTab = new GUIText(Translation.Create("new_character"), new Vector2(0f, bandPosition.Y), Color.White, FontCache.GetBold(15));

            _openFolder = new Button(new Vector2(0.8f,bandPosition.Y), new Vector2(0.15f,0.05f),
                Translation.Create("character_folder"), Color.White, FontCache.GetNormal(13));
            _openFolder.Click += delegate
            {
                System.Diagnostics.Process.Start(DataManager.CharactersFolder);
            };

            _human = new Humanoid
            {
                Name = "CreatorHumanoid",
            };
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
            _human.Physics.UsePhysics = false;
            World.RemoveEntity(_human);

            var rng = new Random();
            var classChooser = CreateClassChooser(defaultFont, defaultColor, rng);
            var genderChooser = CreateGenderChooser(defaultFont, defaultColor, rng);
            CreateColorPickers(this);
            
            UpdateModel(null, default);
            
            var nameField = new TextField(new Vector2(0,-.7f), new Vector2(.15f,.03f).As1920x1080());
            var createChr = new Button(new Vector2(0f,-.8f), new Vector2(.15f,.05f), Translation.Create("create"), defaultColor, FontCache.GetBold(11));
            createChr.Click += delegate
            {
                for(var i = 0; i < DataManager.PlayerFiles.Length; i++)
                {
                    if (nameField.Text != DataManager.PlayerFiles[i].Name) continue;
                    Player.MessageDispatcher.ShowNotification(Translations.Get("name_exists"), Color.Red, 3f, true);
                    return;
                }

                if(!LocalPlayer.CreatePlayer(nameField.Text, _classType, _customization)) return;          
                base.Disable();
                GameManager.Player.UI.CharacterSelector.Enable();
                nameField.Text = string.Empty;    
            };
            
            AddElement(genderChooser);
            AddElement(classChooser);
            AddElement(nameField);
            AddElement(createChr);
            AddElement(blackBand);
            AddElement(_openFolder);
            AddElement(currentTab);
            base.Disable();

            OnPanelStateChange += PanelStateChange;
            OnEscapePressed += EscapePressed;
            UpdateManager.Add(this);
        }

        private void CreateColorPickers(Panel InPanel)
        {
            const int columnCount = 8;
            const int rowCount = 8;
            var allColors = new Vector4[columnCount * rowCount];
            const float step = 360f / columnCount;
            for (var j = 0; j < rowCount; ++j)
            {
                for (var i = 0; i < columnCount; ++i)
                {
                    allColors[j * columnCount + i] = Colors.HsLtoRgba(i * step, 1.0f, 1.0f - (j+1) / (float) (rowCount+2), 1f);
                }
            }
            
            var picker1 = new ColorPicker(allColors, Translation.Create("first_hair_picker"), new Vector2(.3f, .4f), Vector2.One * 0.25f, InPanel, columnCount, 13);
            picker1.ColorPickedEvent += V =>
            {
                _customization.FirstHairColor = V;
                UpdateModel(null, default);
            };
            InPanel.AddElement(picker1);

            _secondHairColorPicker = new ColorPicker(allColors, Translation.Create("second_hair_picker"), new Vector2(.3f, -.2f), Vector2.One * 0.25f, InPanel, columnCount, 13);
            _secondHairColorPicker.ColorPickedEvent += V =>
            {
                _customization.SecondHairColor = V;
                UpdateModel(null, default);
            };
            InPanel.AddElement(_secondHairColorPicker);

            var skinPicker = new ColorPicker(_skinColors, Translation.Create("skin_picker"), new Vector2(-.75f, .35f), Vector2.One * 0.4f, InPanel, 4);
            skinPicker.ColorPickedEvent += V =>
            {
                _customization.SkinColor = V;
                UpdateModel(null, default);
            };
            InPanel.AddElement(skinPicker);
        }


        private OptionChooser CreateClassChooser(Font DefaultFont, Color DefaultColor, Random Rng)
        {
            var classes =
                ClassDesign.AvailableClassNames.Select(S =>
                    new Pair<string, ClassDesign>(S, ClassDesign.FromString(S))).ToArray();
            classes.Shuffle(Rng);
            var classesNames = classes.Select(S => Translation.Create(S.One.ToLowerInvariant())).ToArray();
            var classChooser = new OptionChooser(new Vector2(0,.5f), Vector2.Zero, Translation.Create("class"), DefaultColor,
                DefaultFont, classesNames, true);

            void UpdateClass(object Sender, MouseButtonEventArgs _)
            {
                _classType = classes[classChooser.Index].Two;
                _customization = CustomizationData.FromClass(_classType, _customization.Gender);
                UpdateModel(null, default);
            }

            UpdateClass(null, default);
            
            classChooser.RightArrow.Click += UpdateClass;
            classChooser.LeftArrow.Click += UpdateClass;
            classChooser.CurrentValue.TextColor = DefaultColor;
            classChooser.CurrentValue.UpdateText();
            return classChooser;
        }
        private OptionChooser CreateGenderChooser(Font DefaultFont, Color DefaultColor, Random Rng)
        {
            var genders = new[]
            {
                new Pair<Translation, HumanGender>(Translation.Create("male"), HumanGender.Male),
                new Pair<Translation, HumanGender>(Translation.Create("female"), HumanGender.Female)
            };
            var genderChooser = new OptionChooser(new Vector2(0,.3f), Vector2.Zero, Translation.Create("gender"), DefaultColor, DefaultFont, genders.Select(P => P.One).ToArray(), true);
            genderChooser.Index = Rng.Next(0, 2);
            
            void UpdateGender(object Sender, MouseButtonEventArgs _)
            {
                _customization = CustomizationData.FromClass(_classType, genders[genderChooser.Index].Two);
                UpdateModel(null, default);
            }

            genderChooser.RightArrow.Click += UpdateGender;
            genderChooser.LeftArrow.Click += UpdateGender;
            genderChooser.CurrentValue.TextColor = DefaultColor;
            genderChooser.CurrentValue.UpdateText();
            return genderChooser;
        }
        
        private void UpdateModel(object Sender, MouseButtonEventArgs E)
        {
            var previousModel = _human.Model;
            _human.Class = _classType;
            _human.Customization = _customization;
            _human.Model = new HumanoidModel(_human, _classType.ModelTemplate);
            _human.Model.SetValues(previousModel);
            _human.SetHelmet(_classType.StartingItems.FirstOrDefault(P => P.Value.IsHelmet).Value);
            _human.SetWeapon(_classType.StartingItems.First(P => P.Value.IsWeapon).Value.Weapon);
            /* One time for updating the default body parts */
            _human.UpdateEquipment();
            /* The second time is for setting them */
            _human.UpdateEquipment();
            previousModel.Dispose();
            if(_customization.Gender == HumanGender.Female && _classType.HasSecondFemaleHairColor || _customization.Gender == HumanGender.Male && _classType.HasSecondHairColor)
                _secondHairColorPicker?.Enable();
            else
                _secondHairColorPicker?.Disable();
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
                _human.UpdateCriticalComponents();
                _newRot += Time.IndependentDeltaTime * 30f;
                _human.Model.LocalRotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;
                _human.Model.TargetRotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;
                _human.Position = new Vector3(Scenes.MenuBackground.PlatformPosition.X, _human.Position.Y, Scenes.MenuBackground.PlatformPosition.Z);
                if(_human.Position.Y <= Physics.HeightAtPosition(_human.Position))
                {
                    _human.Position = new Vector3(_human.Position.X, Physics.HeightAtPosition(_human.Position), _human.Position.Z);
                }
                _human.IsKnocked = false;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            UpdateManager.Remove(this);
        }

        private static Vector4[] _skinColors = new[]
        {
            Colors.FromHtml("#FFBFA1"),
            Colors.FromHtml("#743D2B"),
            Colors.FromHtml("#EDD8C7"),
            Colors.FromHtml("#4A332D"),
            Colors.FromHtml("#D19477"),
            Colors.FromHtml("#DFAA8B"),
            Colors.FromHtml("#E5B5A1"),
            Colors.FromHtml("#FEE3D4"),
        };

        private static Vector4[] _hairColors = new[]
        {
            Colors.FromHtml("#4E3616"),
            Colors.FromHtml("#8E7B6A"),
            Colors.FromHtml("#B2ABA7"),
            Colors.FromHtml("#59442F"),
            Colors.FromHtml("#FFFFFF"),
            //Colors.FromHtml("#FFBFA1"),
            //Colors.FromHtml("#FFBFA1"),
            //Colors.FromHtml("#FFBFA1"),
            //Colors.FromHtml("#FFBFA1"),
        };
    }
}
