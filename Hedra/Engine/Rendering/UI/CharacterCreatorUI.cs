/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 03:17 p.m.
 * 
 * To change  template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.Windowing;
using Hedra.Engine.WorldBuilding;
using Hedra.Framework;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     Description of Panel.
    /// </summary>
    public class CharacterCreatorUI : Panel, IUpdatable
    {
        private static readonly Vector4[] _skinColors =
        {
            Colors.FromHtml("#FFBFA1"),
            Colors.FromHtml("#743D2B"),
            Colors.FromHtml("#EDD8C7"),
            Colors.FromHtml("#4A332D"),
            Colors.FromHtml("#D19477"),
            Colors.FromHtml("#DFAA8B"),
            Colors.FromHtml("#E5B5A1"),
            Colors.FromHtml("#FEE3D4")
        };

        private readonly Timer _clickTimer;
        private Humanoid _human;
        private readonly Button _openFolder;
        private ClassDesign _classType;
        private CustomizationData _customization;
        private float _newRot;
        private ColorPicker _secondHairColorPicker;
        private Dictionary<string, Humanoid> _humans;

        public CharacterCreatorUI(IPlayer Player)
        {
            var classes =
                ClassDesign.AvailableClassNames.Select(S =>
                    new Pair<string, ClassDesign>(S, ClassDesign.FromString(S))).ToArray();
            _customization = new CustomizationData();
            _clickTimer = new Timer(.25f)
            {
                UseTimeScale = false
            };
            var defaultFont = FontCache.GetNormal(12);
            var defaultColor = Color.White;

            var bandPosition = new Vector2(0f, .8f);
            var blackBand = new BackgroundTexture(Color.FromRgb(69, 69, 69), Color.FromRgb(19, 19, 19),
                bandPosition, UserInterface.BlackBandSize, GradientType.LeftRight);

            var currentTab = new GUIText(Translation.Create("new_character"), new Vector2(0f, bandPosition.Y),
                Color.White, FontCache.GetBold(15));

            _openFolder = new Button(new Vector2(0.8f, bandPosition.Y), new Vector2(0.15f, 0.05f),
                Translation.Create("character_folder"), Color.White, FontCache.GetNormal(13));
            _openFolder.Disable();
            _openFolder.Click += (_, __) => Process.Start(DataManager.CharactersFolder);
            
            CreateModels(classes);

            var rng = new Random();
            var classChooser = CreateClassChooser(classes, defaultFont, defaultColor, rng);
            var genderChooser = CreateGenderChooser(defaultFont, defaultColor, rng);
            CreateColorPickers(this);

            UpdateModel();

            var nameField = new TextField(new Vector2(0, -.7f), new Vector2(.15f, .03f).As1920x1080());
            var createChr = new Button(new Vector2(0f, -.8f), new Vector2(.15f, .05f), Translation.Create("create"),
                defaultColor, FontCache.GetBold(11));
            createChr.Click += delegate
            {
                for (var i = 0; i < DataManager.PlayerFiles.Length; i++)
                {
                    if (nameField.Text != DataManager.PlayerFiles[i].Name) continue;
                    Player.MessageDispatcher.ShowNotification(Translations.Get("name_exists"), Color.Red, 3f, true);
                    return;
                }

                if (!LocalPlayer.CreatePlayer(nameField.Text, _classType, _customization)) return;
                Disable();
                DataManager.ReloadPlayerFiles();
                GameManager.Player.UI.CharacterSelector.Enable();
                nameField.Text = string.Empty;
            };

            AddElement(genderChooser);
            AddElement(classChooser);
            AddElement(nameField);
            AddElement(createChr);
            AddElement(blackBand);
            //AddElement(_openFolder);
            AddElement(currentTab);
            Disable();

            OnPanelStateChange += PanelStateChange;
            OnEscapePressed += EscapePressed;
            UpdateManager.Add(this);
        }

        public void Update()
        {
            _human.Model.Enabled = Enabled;
            if (Enabled)
            {
                _newRot += Time.IndependentDeltaTime * 30f;
                foreach (var pair in _humans)
                {
                    var human = pair.Value;
                    human.Physics.ResetVelocity();
                    human.Physics.ResetFall();
                    human.Physics.ResetSpeed();
                    var prev = human.Model.Enabled;
                    human.Model.Enabled = true;
                    human.Update();
                    human.UpdateCriticalComponents();
                    human.Model.Enabled = prev;
                    
                    human.Model.LocalRotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;
                    human.Model.TargetRotation = Vector3.UnitY * -90 + Vector3.UnitY * _newRot;

                    human.Position = new Vector3(MenuBackground.PlatformPosition.X, human.Position.Y, MenuBackground.PlatformPosition.Z);
                    human.Position = new Vector3(human.Position.X,  Physics.HeightAtPosition(human.Position) + 1f, human.Position.Z);

                    human.IsKnocked = false;
                    human.Model.StopMoving();
                }

                if (_clickTimer.Tick())
                    _openFolder.CanClick = true;

                if (_customization.Gender == HumanGender.Female && _classType.HasSecondFemaleHairColor ||
                    _customization.Gender == HumanGender.Male && _classType.HasSecondHairColor)
                    _secondHairColorPicker?.Enable();
                else
                    _secondHairColorPicker?.Disable();
            }
        }

        private void CreateColorPickers(Panel InPanel)
        {
            var columnCount = 8;
            var allColors = NPCCreator.HairColors;
            var picker1 = new ColorPicker(allColors, Translation.Create("first_hair_picker"), new Vector2(.3f, .4f),
                Vector2.One * 0.25f, InPanel, columnCount, 13);
            picker1.ColorPickedEvent += V =>
            {
                _customization.FirstHairColor = V;
                UpdateModel();
            };
            InPanel.AddElement(picker1);

            _secondHairColorPicker = new ColorPicker(allColors, Translation.Create("second_hair_picker"),
                new Vector2(.3f, -.2f), Vector2.One * 0.25f, InPanel, columnCount, 13);
            _secondHairColorPicker.ColorPickedEvent += V =>
            {
                _customization.SecondHairColor = V;
                UpdateModel();
            };
            InPanel.AddElement(_secondHairColorPicker);

            var skinPicker = new ColorPicker(_skinColors, Translation.Create("skin_picker"), new Vector2(-.75f, .35f),
                Vector2.One * 0.4f, InPanel, 4);
            skinPicker.ColorPickedEvent += V =>
            {
                _customization.SkinColor = V;
                UpdateModel();
            };
            InPanel.AddElement(skinPicker);
        }


        private OptionChooser CreateClassChooser(Pair<string, ClassDesign>[] classes, Font DefaultFont, Color DefaultColor, Random Rng)
        {
            classes.Shuffle(Rng);
            var classesNames = classes.Select(S => Translation.Create(S.One.ToLowerInvariant())).ToArray();
            var classChooser = new OptionChooser(new Vector2(0, .5f), Vector2.Zero, Translation.Create("class"),
                DefaultColor,
                DefaultFont, classesNames, true);

            void UpdateClass(object Sender, MouseButtonEventArgs _)
            {
                _classType = classes[classChooser.Index].Two;
                _customization = CustomizationData.FromClass(_classType, _customization.Gender);
                UpdateModel();
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
            var genderChooser = new OptionChooser(new Vector2(0, .3f), Vector2.Zero, Translation.Create("gender"),
                DefaultColor, DefaultFont, genders.Select(P => P.One).ToArray(), true);
            genderChooser.Index = Rng.Next(0, 2);

            void UpdateGender(object Sender, MouseButtonEventArgs _)
            {
                _customization = CustomizationData.FromClass(_classType, genders[genderChooser.Index].Two);
                UpdateModel();
            }

            genderChooser.RightArrow.Click += UpdateGender;
            genderChooser.LeftArrow.Click += UpdateGender;
            genderChooser.CurrentValue.TextColor = DefaultColor;
            genderChooser.CurrentValue.UpdateText();
            return genderChooser;
        }

        private void UpdateModel()
        {
            var previousHuman = _human;
            var human = _humans[_classType.Name + "-" + (int) _customization.Gender];
            _human = human;
            if (previousHuman != null)
                _human.Model.SetValues(previousHuman.Model);
            _human.Model.Enabled = true;
            human.Customization = _customization;
            if (previousHuman != null && previousHuman != human)
                previousHuman.Model.Enabled = false;
        }

        private void CreateModels(Pair<string, ClassDesign>[] Classes)
        {
            _humans = new Dictionary<string, Humanoid>();
            foreach (var pair in Classes)
            {
                for (var gender = 0; gender < (int)HumanGender.All; ++gender)
                {
                    var identifier = pair.One + "-" + gender;
                    var human = new Humanoid
                    {
                        Name = $"CreatorHumanoid_{identifier}",
                    };
                    var model = new HumanoidModel(human, pair.Two.ModelTemplate)
                    {
                        LocalRotation = Vector3.UnitY * -90,
                        TargetRotation = Vector3.UnitY * -90,
                        Position = MenuBackground.PlatformPosition,
                        ApplyFog = true,
                        Enabled = false,
                    };
                    var classType = pair.Two;
                    human.Model = model;
                    human.Class = classType;
                    human.Customization = CustomizationData.FromClass(classType, (HumanGender)gender);
                    human.SetHelmet(classType.StartingItems.FirstOrDefault(P => P.Value.IsHelmet).Value);
                    human.SetWeapon(classType.StartingItems.First(P => P.Value.IsWeapon).Value.Weapon);
                    /* One time for updating the default body parts */
                    human.UpdateEquipment();
                    /* The second time is for setting them */
                    human.UpdateEquipment();
                    human.SearchComponent<DamageComponent>().Immune = true;
                    human.Physics.UseTimescale = false;
                    human.Physics.UsePhysics = false;
                    human.Physics.CollidesWithEntities = false;
                    human.Model.Enabled = false;
                    human.Model.UpdateWhenOutOfView = true;
                    World.RemoveEntity(_human);
                    _humans[identifier] = human;
                }
            }

            _human = _humans["Warrior-0"];
        }

        private void PanelStateChange(object Sender, PanelState State)
        {
            switch (State)
            {
                case PanelState.Disabled:
                    MenuBackground.Creator = false;
                    break;
                case PanelState.Enabled:
                    MenuBackground.Creator = true;
                    _openFolder.CanClick = false;
                    _openFolder.Disable();
                    _clickTimer.Reset();
                    break;
            }
        }

        private void EscapePressed(object Sender, EventArgs Args)
        {
            Disable();
            GameManager.Player.UI.CharacterSelector.Enable();
        }

        public override void Dispose()
        {
            base.Dispose();
            UpdateManager.Remove(this);
        }
    }
}