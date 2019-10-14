/*
 * Author: Zaphyk
 * Date: 17/03/2016
 * Time: 01:54 p.m.
 *
 */
using System;
using System.Drawing;
using System.Linq;
using Hedra.Engine.Localization;
using Hedra.Engine.Windowing;
using Hedra.Rendering.UI;
using System.Numerics;


namespace Hedra.Engine.Rendering.UI
{
    public class OptionChooser : UIElement
    {
        // \u25C0 Left
        // \u25B6 Right        
        
        public GUIText Text { get; private set; }
        public Button LeftArrow{ get; private set; }
        public Button RightArrow { get; private set; }
        public GUIText CurrentValue { get; private set; }
        public int Index { get; set; }
        public Font Font { get; private set; }
        public Color Color { get; private set; }
        public Translation[] Options { get; private set; }
        
        private void Initialize(Vector2 Position, Vector2 Scale, Translation Translation, Color C, Font F, Translation[] Options, bool Centered)
        {
            this.Font = F;
            this.Color = C;
            this.Options = Options;
            var max = Options.Max(T => T.Get().Length);
            var longestValue = Options.FirstOrDefault(T => T.Get().Length == max);


            var prevCurrentValue = new GUIText(longestValue, Position, Color.Transparent, F);
            var prevRightArrow = new Button(Position, Scale, "\u25B6", Color.Transparent, F);
            var prevLeftArrow = new Button(Position, Scale, "\u25C0", Color.Transparent, F);
            
            if(!Centered)
            {
                this.Text = new GUIText(Translation, new Vector2(Position.X - prevCurrentValue.Scale.X - prevRightArrow.Scale.X, Position.Y), C, F);
                Vector2 Place() => this.Text.Position + new Vector2(prevLeftArrow.Scale.X + this.Text.Scale.X, 0);
                Translation.LanguageChanged += delegate
                {
                    this.LeftArrow.Position = Place();
                };
                this.LeftArrow = new Button(Place(), Scale, "\u25C0", C, F);
                this.CurrentValue = new GUIText(longestValue, LeftArrow.Position + new Vector2(LeftArrow.Scale.X + prevCurrentValue.Scale.X, 0), C, F);
                this.RightArrow = new Button(CurrentValue.Position + new Vector2(CurrentValue.Scale.X + prevRightArrow.Scale.X, 0), Scale, "\u25B6", C, F);
            }
            else
            {
                this.Text = new GUIText(Translation, Position + new Vector2(0,prevCurrentValue.Scale.Y * 1.5f), C, F);
            
                this.CurrentValue = new GUIText(longestValue, Position, C, F);
                this.LeftArrow = new Button(Position - new Vector2(prevLeftArrow.Scale.X + CurrentValue.Scale.X,0), Scale, "\u25C0", C, F);
                this.RightArrow = new Button(CurrentValue.Position + new Vector2(CurrentValue.Scale.X + prevRightArrow.Scale.X, 0), Scale, "\u25B6", C, F);
            }
            LeftArrow.Click += this.OnArrowClick;
            RightArrow.Click +=  this.OnRightArrowClick;
            
            prevLeftArrow.Dispose();
            prevRightArrow.Dispose();
            prevCurrentValue.Dispose();
        }
        
        public OptionChooser(Vector2 Position, Vector2 Scale, Translation Text, Color C, Font F, Translation[] Options, bool Centered = false) : base()
        {
            Initialize(Position, Scale, Text, C, F, Options, Centered);
        }
        
        public OptionChooser(Vector2 Position, Vector2 Scale, string Text, Color C, Font F, string[] Options, bool Centered = false) : base()
        {
            Initialize(Position, Scale, Translation.Default(Text), C, F, Options.Select(Translation.Default).ToArray(), Centered);
        }
        
        public void OnArrowClick(object Sender, MouseButtonEventArgs E)
        {
            Index--;
            this.Update();
        }
        
        public void OnRightArrowClick(object Sender, MouseButtonEventArgs E)
        {
            Index++;
            this.Update();
        }
        
        private void Update()
        {
            if(Index == Options.Length)
                Index = 0;
            if(Index == -1)
                Index = Options.Length-1;

            this.CurrentValue.SetTranslation(Options[Index]);
        }
        
        public void Enable(){
            LeftArrow.Enable();
            RightArrow.Enable();
            Text.Enable();
            CurrentValue.Enable();
        }
        
        public void Disable(){
            LeftArrow.Disable();
            RightArrow.Disable();
            Text.Disable();
            CurrentValue.Disable();
        }
        
        private bool _mClickable = true;
        public bool Clickable{
            get => _mClickable;
            set{
                this.LeftArrow.CanClick = value;
                this.RightArrow.CanClick = value;
                this._mClickable = value;
            }
        }
        
        private Vector2 _mScale;
        public Vector2 Scale{
            get => _mScale;
            set{
                _mScale = value;
                Text.Scale = Scale;
                LeftArrow.Scale = Scale;
                RightArrow.Scale = Scale;
                Text.Scale = Scale;
                CurrentValue.Scale = Scale;
            }
        }
        private Vector2 _mPosition;
        public Vector2 Position{
            get => _mPosition;
            set{
                Text.Position = Text.Position - _mPosition + value;
                LeftArrow.Position = LeftArrow.Position - _mPosition + value;
                RightArrow.Position = RightArrow.Position - _mPosition + value;
                CurrentValue.Position = CurrentValue.Position - _mPosition + value;
                _mPosition = value;
            }
        }

        public void Dispose()
        {
            this.Text?.Dispose();
            this.Font?.Dispose();
            this.LeftArrow?.Dispose();
            this.RightArrow?.Dispose();
            this.CurrentValue?.Dispose();
        }
    }
}
