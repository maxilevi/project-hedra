/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.Rendering.UI;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.SkillSystem
{
    public delegate void OnStateUpdated();

    public abstract class AbstractBaseSkill : DrawableTexture, UIElement
    {
        public event OnStateUpdated StateUpdated;
        public int Level { get; set; }
        public abstract uint IconId { get; }
        public bool IsAffecting => IsAffectingModifier > 0;
        public virtual bool Passive { get; set; }
        public abstract float ManaCost { get; }
        public bool Active { get; set; } = true;
        public bool Casting { get; set; }
        public abstract float MaxCooldown { get; }
        public virtual float IsAffectingModifier => Passive ? 1 : 0;
        public abstract Vector2 Scale { get; set; }
        public abstract Vector2 Position { get; set; }
        public abstract void InitializeUI(Vector2 Position, Vector2 Scale, Panel InPanel);
        public abstract void Initialize(ISkillUser User);
        public abstract void Enable();
        public abstract void Disable();
        public abstract void Dispose();
        public abstract bool MeetsRequirements();
        public abstract bool CanBeCastedWhileAttacking { get; }
        public abstract void Use();
        public abstract void Reset();
        protected abstract void DoUse();
        public abstract void ResetCooldown();
        public abstract void KeyUp();
        public abstract void Unload();
        public abstract void Load();
        public abstract string[] Attributes { get; }
        public abstract string Description { get; }
        public abstract string DisplayName { get; }
        public abstract bool PlaySound { get; }
        
        protected void InvokeStateUpdated()
        {
            StateUpdated?.Invoke();
        }
    }
    public abstract class BaseSkill<T> : AbstractBaseSkill, IRenderable, IUpdatable, ISimpleTexture, IAdjustable where T : ISkillUser
    {
        private static readonly Shader Shader = Shader.Build("Shaders/Skills.vert", "Shaders/Skills.frag");
        private static readonly Vector3 NormalTint = Vector3.One;
        protected float Cooldown { get; set; }
        protected Vector3 Tint { get; set; }
        protected T User { get; set; }
        protected virtual bool HasCooldown => true;
        protected virtual bool ShouldDisable { get; set; }
        private bool _initializedUI;
        private bool Enabled { get; set; } = true;
        private Vector2 _adjustedPosition;
        private RenderableText _cooldownSecondsText;
        private Panel _panel;
        private Vector2 _position;

        protected BaseSkill()
        {
            /* Invoked via reflection */
        }

        public override void Initialize(ISkillUser User)
        {
            if(!(User is T))
                throw new ArgumentException($"Provided user must be of type '{typeof(T)}' but is of type '{User.GetType()}'");
            this.User = (T)User;
            UpdateManager.Add(this);
        }
        
        public override void InitializeUI(Vector2 Position, Vector2 Scale, Panel InPanel)
        {
            this.Position = Position;
            this.Scale = Scale;
            TextureId = IconId;
            Tint = NormalTint;
            _panel = InPanel;
            _panel.AddElement(this);
            
            _cooldownSecondsText = new RenderableText(string.Empty, Position, Color.White, FontCache.GetBold(12));
            _panel.AddElement(_cooldownSecondsText);
            if(_panel.Enabled) _cooldownSecondsText.Enable();
            
            DrawManager.UIRenderer.Add(this, DrawOrder.After);
            _initializedUI = true;
        }
        
        public override bool MeetsRequirements()
        {
            if (Cooldown > 0 || User.Mana - ManaCost <= 0 || Level <= 0 || !Active) return false;

            return !ShouldDisable && User.CanCastSkill;
        }
        
        public virtual void Draw()
        {
            if(!Enabled || !Active)
                return;
            
            if(!_initializedUI) throw new ArgumentException("This skill hasn't been initialized yet.");
            
            Cooldown -= Time.DeltaTime;
            if (_cooldownSecondsText.Position != Position) _cooldownSecondsText.Position = Position;
            _cooldownSecondsText.Text = Cooldown > 0 && HasCooldown ? ((int)Cooldown + 1).ToString() : string.Empty;
            Renderer.Enable(EnableCap.Blend);
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.CullFace);
            
            Shader.Bind();
            Shader["Tint"] = User.Mana - this.ManaCost < 0 && Tint == NormalTint ? new Vector3(.9f,.6f,.6f) : Tint;
            Shader["Scale"] = Scale * new Vector2(1,-1);
            Shader["Position"] = _adjustedPosition;
            Shader["Bools"] = new Vector2(Level == 0 || ShouldDisable ? 1 : 0, 1);
            Shader["Cooldown"] = OverlayBlending;
            
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, TextureId = IconId);
            
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, InventoryArrayInterface.DefaultId);
            Shader["Mask"] = 1;
            
            DrawManager.UIRenderer.DrawQuad();
            
            Shader.Unbind();
            
            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
            
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            
            _cooldownSecondsText.Draw();
        }

        public void Adjust()
        {
            _adjustedPosition = GUITexture.Adjust(Position);
        }

        public override void ResetCooldown()
        {
            Cooldown = 0;
        }
        
        protected void SetOnCooldown()
        {
            Cooldown = MaxCooldown / User.Attributes.CooldownReductionModifier;
        }

        public override bool CanBeCastedWhileAttacking => false;
        protected virtual float OverlayBlending => Cooldown / MaxCooldown;
        public override bool PlaySound => false;

        public override void Use()
        {
            SetOnCooldown();
            DoUse();
        }

        public override void Reset()
        {
            Level = 0;
            Update();
        }

        public override void KeyUp(){}
        public override void Unload(){}
        public override void Load(){}
        public abstract void Update();

        public override Vector2 Scale { get; set; }
        public override string[] Attributes => new string[0];

        public override Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                this.Adjust();
            }
        }

        public override void Enable()
        {
            this.Enabled = true;
        }
        
        public override void Disable()
        {
            this.Enabled = false;
        }
        
        public override void Dispose()
        {
            UpdateManager.Remove(this);
            if (_initializedUI)
            {
                DrawManager.Remove(this);
                TextureRegistry.Remove(IconId);
                _cooldownSecondsText.Dispose();
            }
        }    
    }
}
