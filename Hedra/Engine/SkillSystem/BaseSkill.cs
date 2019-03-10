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
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.SkillSystem
{
    public delegate void OnStateUpdated();
    
    /// <summary>
    /// Description of Skill.
    /// </summary>
    public abstract class BaseSkill : UIElement, IRenderable, IUpdatable, ISimpleTexture, IAdjustable
    {
        private static readonly Shader Shader = Shader.Build("Shaders/Skills.vert", "Shaders/Skills.frag");
        private static readonly Vector3 NormalTint = Vector3.One;
        public event OnStateUpdated StateUpdated;
        public bool IsAffecting => IsAffectingModifier > 0;
        public virtual float MaxCooldown { get; }
        public virtual float IsAffectingModifier => Passive ? 1 : 0;
        public virtual float ManaCost { get; }
        public int Level { get; set; }
        public bool Active { get; set; } = true;
        public virtual bool Passive { get; set; }
        public abstract string Description { get; }
        public abstract string DisplayName { get; }
        public bool Casting { get; set; }
        public abstract uint TextureId { get; }
        protected float Cooldown { get; set; }
        protected Vector3 Tint { get; set; }
        protected IPlayer Player => GameManager.Player;
        protected virtual bool HasCooldown => true;
        protected virtual bool ShouldDisable { get; set; }
        private bool _initialized;
        private bool Enabled { get; set; } = true;
        private Vector2 _adjustedPosition;
        private RenderableText _cooldownSecondsText;
        private Panel _panel;
        private Vector2 _position;

        protected BaseSkill()
        {
            /* Invoked via reflection */
        }

        public virtual void Initialize(Vector2 Position, Vector2 Scale, Panel InPanel, IPlayer Player)
        {
            this._panel = InPanel;
            this.Position = Position;
            this.Scale = Scale;
            this.Tint = NormalTint;
            _panel.AddElement(this);
            
            DrawManager.UIRenderer.Add(this, DrawOrder.After);
            UpdateManager.Add(this);
            this._initialized = true;
        }
        
        public virtual bool MeetsRequirements()
        {
            if (Cooldown > 0 || Player.Mana - ManaCost <= 0 ||
                this.Level <= 0 || !Active || Player.IsEating) return false;

            return Player.IsRiding || !Player.IsRiding && !ShouldDisable;
        }
        
        public virtual void Draw()
        {
            if(!Enabled || !Active)
                return;
            
            if(!_initialized) throw new ArgumentException("This skill hasn't been initialized yet.");

            Cooldown -= Time.DeltaTime;
            if (_cooldownSecondsText == null)
            {
                _cooldownSecondsText = new RenderableText(string.Empty, Position, Color.White,
                    FontCache.Get(AssetManager.BoldFamily, 12, FontStyle.Bold));
                if(_panel.Enabled) _cooldownSecondsText.Enable();
                _panel.AddElement(_cooldownSecondsText);
            }
            if (_cooldownSecondsText.Position != Position) _cooldownSecondsText.Position = Position;
            _cooldownSecondsText.Text = Cooldown > 0 && HasCooldown ? ((int)Cooldown + 1).ToString() : string.Empty;
            Renderer.Enable(EnableCap.Blend);
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.CullFace);
            
            Shader.Bind();
            Shader["Tint"] = Player.Mana - this.ManaCost < 0 && Tint == NormalTint ? new Vector3(.9f,.6f,.6f) : Tint;
            Shader["Scale"] = Scale * new Vector2(1,-1);
            Shader["Position"] = _adjustedPosition;
            Shader["Bools"] = new Vector2(Level == 0 || ShouldDisable ? 1 : 0, 1);
            Shader["Cooldown"] = OverlayBlending;
            
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, TextureId);
            
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, InventoryArrayInterface.DefaultId);
            Shader["Mask"] = 1;
            
            DrawManager.UIRenderer.SetupQuad();
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

        protected void InvokeStateUpdated()
        {
            StateUpdated?.Invoke();
        }
        
        public void ResetCooldown()
        {
            Cooldown = 0;
        }

        protected void SetOnCooldown()
        {
            Cooldown = MaxCooldown / Player.Attributes.CooldownReductionModifier;
        }

        public virtual bool CanBeCastedWhileAttacking => false;
        protected virtual float OverlayBlending => Cooldown / MaxCooldown;
        public virtual bool PlaySound => false;

        public void Use()
        {
            SetOnCooldown();
            DoUse();
        }

        public void Reset()
        {
            Level = 0;
            Update();
        }

        protected abstract void DoUse();
        public virtual void KeyUp(){}
        public virtual void Unload(){}
        public virtual void Load(){}
        public abstract void Update();

        public Vector2 Scale { get; set; }
        public virtual string[] Attributes => new string[0];

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                this.Adjust();
            }
        }

        public void Enable()
        {
            this.Enabled = true;
        }
        
        public void Disable()
        {
            this.Enabled = false;
        }
        
        public void Dispose()
        {
            DrawManager.Remove(this);
            UpdateManager.Remove(this);
            _cooldownSecondsText.Dispose();
        }    
    }
}
