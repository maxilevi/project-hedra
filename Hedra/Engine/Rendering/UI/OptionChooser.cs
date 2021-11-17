/*
 * Author: Zaphyk
 * Date: 17/03/2016
 * Time: 01:54 p.m.
 *
 */

using System.Linq;
using System.Numerics;
using Hedra.Engine.Localization;
using Hedra.Engine.Windowing;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public class OptionChooser : UIElement
    {
        private const string RightArrowString = ">";
        private const string LeftArrowString = "<";

        private bool _mClickable = true;

        private Vector2 _mPosition;

        private Vector2 _mScale;

        public OptionChooser(Vector2 Position, Vector2 Scale, Translation Text, Color C, Font F, Translation[] Options,
            bool Centered = false)
        {
            Initialize(Position, Scale, Text, C, F, Options, Centered);
        }

        public OptionChooser(Vector2 Position, Vector2 Scale, string Text, Color C, Font F, string[] Options,
            bool Centered = false)
        {
            Initialize(Position, Scale, Translation.Default(Text), C, F, Options.Select(Translation.Default).ToArray(),
                Centered);
        }
        // \u25C0 Left
        // \u25B6 Right        

        public GUIText Text { get; private set; }
        public Button LeftArrow { get; private set; }
        public Button RightArrow { get; private set; }
        public GUIText CurrentValue { get; private set; }
        public int Index { get; set; }
        public Font Font { get; private set; }
        public Color Color { get; private set; }
        public Translation[] Options { get; private set; }

        public bool Clickable
        {
            get => _mClickable;
            set
            {
                LeftArrow.CanClick = value;
                RightArrow.CanClick = value;
                _mClickable = value;
            }
        }

        public void Enable()
        {
            LeftArrow.Enable();
            RightArrow.Enable();
            Text.Enable();
            CurrentValue.Enable();
        }

        public void Disable()
        {
            LeftArrow.Disable();
            RightArrow.Disable();
            Text.Disable();
            CurrentValue.Disable();
        }

        public Vector2 Scale
        {
            get => _mScale;
            set
            {
                _mScale = value;
                Text.Scale = Scale;
                LeftArrow.Scale = Scale;
                RightArrow.Scale = Scale;
                Text.Scale = Scale;
                CurrentValue.Scale = Scale;
            }
        }

        public Vector2 Position
        {
            get => _mPosition;
            set
            {
                Text.Position = Text.Position - _mPosition + value;
                LeftArrow.Position = LeftArrow.Position - _mPosition + value;
                RightArrow.Position = RightArrow.Position - _mPosition + value;
                CurrentValue.Position = CurrentValue.Position - _mPosition + value;
                _mPosition = value;
            }
        }

        public void Dispose()
        {
            Text?.Dispose();
            LeftArrow?.Dispose();
            RightArrow?.Dispose();
            CurrentValue?.Dispose();
        }

        private void Initialize(Vector2 Position, Vector2 Scale, Translation Translation, Color C, Font F,
            Translation[] Options, bool Centered)
        {
            Font = F;
            Color = C;
            this.Options = Options;
            var max = Options.Max(T => T.Get().Length);
            var longestValue = Options.FirstOrDefault(T => T.Get().Length == max);


            var prevCurrentValue = new GUIText(longestValue, Position, Color.Transparent, F);
            var prevRightArrow = new Button(Position, Scale, LeftArrowString, Color.Transparent, F);
            var prevLeftArrow = new Button(Position, Scale, RightArrowString, Color.Transparent, F);

            if (!Centered)
            {
                Text = new GUIText(Translation,
                    new Vector2(Position.X - prevCurrentValue.Scale.X - prevRightArrow.Scale.X, Position.Y), C, F);

                Vector2 Place()
                {
                    return Text.Position + new Vector2(prevLeftArrow.Scale.X + Text.Scale.X, 0);
                }

                Translation.LanguageChanged += delegate { LeftArrow.Position = Place(); };
                LeftArrow = new Button(Place(), Scale, LeftArrowString, C, F);
                CurrentValue = new GUIText(longestValue,
                    LeftArrow.Position + new Vector2(LeftArrow.Scale.X + prevCurrentValue.Scale.X, 0), C, F);
                RightArrow =
                    new Button(CurrentValue.Position + new Vector2(CurrentValue.Scale.X + prevRightArrow.Scale.X, 0),
                        Scale, RightArrowString, C, F);
            }
            else
            {
                Text = new GUIText(Translation, Position + new Vector2(0, prevCurrentValue.Scale.Y * 1.5f), C, F);

                CurrentValue = new GUIText(longestValue, Position, C, F);
                LeftArrow = new Button(Position - new Vector2(prevLeftArrow.Scale.X + CurrentValue.Scale.X, 0), Scale,
                    LeftArrowString, C, F);
                RightArrow =
                    new Button(CurrentValue.Position + new Vector2(CurrentValue.Scale.X + prevRightArrow.Scale.X, 0),
                        Scale, RightArrowString, C, F);
            }

            LeftArrow.Click += OnArrowClick;
            RightArrow.Click += OnRightArrowClick;

            prevLeftArrow.Dispose();
            prevRightArrow.Dispose();
            prevCurrentValue.Dispose();
            Update();
        }

        public void OnArrowClick(object Sender, MouseButtonEventArgs E)
        {
            Index--;
            Update();
        }

        public void OnRightArrowClick(object Sender, MouseButtonEventArgs E)
        {
            Index++;
            Update();
        }

        private void Update()
        {
            if (Index == Options.Length)
                Index = 0;
            if (Index == -1)
                Index = Options.Length - 1;

            CurrentValue.SetTranslation(Options[Index]);
        }
    }
}