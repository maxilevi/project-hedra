﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player.Skills
{
	/// <summary>
	/// Description of Skill.
	/// </summary>
	public abstract class BaseSkill : UIElement, IRenderable, IUpdatable, ISimpleTexture
	{
		public static Shader Shader { get; }
		public static Vector3 GrayTint { get; }
		public static Vector3 NormalTint { get; }
	    public virtual float MaxCooldown { get; protected set; }
        protected virtual bool HasCooldown => true;
        public Vector3 Tint { get; set; }
        public virtual float ManaCost { get; protected set; }
		public float Cooldown { get; set; }
        public int Level { get; set; }
	    public bool Active { get; set; } = true;
        public virtual bool Passive { get; set; }
        public abstract string Description { get; }
	    public bool Casting { get; set; }
	    public virtual uint TextureId { get; protected set; }
	    public uint MaskId { get; set; }
		protected virtual bool Grayscale { get; set; }
		public bool Initialized { get; private set;}
        protected bool UseMask => MaskId != 0;
	    protected bool Enabled { get; set; } = true;
		protected virtual bool UseTextureIdCache { get; } = true;
		protected RenderableText CooldownSecondsText;
	    protected IPlayer Player => GameManager.Player;
	    private Panel _panel;
		private uint _textureId;

	    static BaseSkill()
	    {
	        Shader = Shader.Build("Shaders/Skills.vert", "Shaders/Skills.frag");
            GrayTint = new Vector3(0.299f, 0.587f, 0.114f);
            NormalTint = Vector3.One;
        }

		protected BaseSkill()
		{
			
		}

		public void Initialize(Vector2 Position, Vector2 Scale, Panel InPanel, IPlayer Player)
		{
			this._panel = InPanel;
			this.Position = Position;
			this.Scale = Scale;
			this.Tint = NormalTint;
			this._textureId = TextureId;
		    _panel.AddElement(this);
			
			DrawManager.UIRenderer.Add(this, DrawOrder.After);			
			UpdateManager.Add(this);
			this.Initialized = true;
		}
		
		public virtual bool MeetsRequirements(Toolbar Bar, int CastingAbilityCount)
		{
		    if (!(Cooldown < 0) || !((LocalPlayer.Instance.Mana - ManaCost) > 0) || CastingAbilityCount != 0 ||
		        this.Level <= 0 || !Active || Player.IsEating) return false;

		    return Player.IsRiding || !Player.IsRiding;
		}
		
		public virtual void Draw()
        {
			if(!Enabled || !Active)
				return;
			
			if(!Initialized) throw new ArgumentException("This skill hasn't been initialized yet.");

			Cooldown -= Time.IndependantDeltaTime;
		    if (CooldownSecondsText == null)
		    {
		        CooldownSecondsText = new RenderableText(string.Empty, Position, Color.White,
		            FontCache.Get(AssetManager.BoldFamily, 12, FontStyle.Bold));
		        DrawManager.UIRenderer.Add(CooldownSecondsText, DrawOrder.After);
                if(_panel.Enabled) CooldownSecondsText.Enable();
		        _panel.AddElement(CooldownSecondsText);
            }
		    if (CooldownSecondsText.Position != Position) CooldownSecondsText.Position = Position;
		    CooldownSecondsText.Text = Cooldown > 0 && HasCooldown ? ((int)Cooldown + 1).ToString() : string.Empty;
			Renderer.Enable(EnableCap.Blend);
			Renderer.Disable(EnableCap.DepthTest);
			Renderer.Disable(EnableCap.CullFace);
			
			Shader.Bind();
            Shader["Tint"] = Player.Mana - this.ManaCost < 0 && Tint == NormalTint ? new Vector3(.9f,.6f,.6f) : Tint;
			Shader["Scale"] = Scale * new Vector2(1,-1);
			Shader["Position"] = Position;
			Shader["Bools"] = new Vector2(Level == 0 || Grayscale ? 1 : 0, UseMask ? 1 : 0);
			Shader["Cooldown"] = this.Cooldown / this.MaxCooldown;
			
			Renderer.ActiveTexture(TextureUnit.Texture0);
			Renderer.BindTexture(TextureTarget.Texture2D, UseTextureIdCache ? _textureId : TextureId);
			
			Renderer.ActiveTexture(TextureUnit.Texture1);
			Renderer.BindTexture(TextureTarget.Texture2D, MaskId);
		    Shader["Mask"] = 1;
			
			DrawManager.UIRenderer.SetupQuad();
			DrawManager.UIRenderer.DrawQuad();
			
			Shader.Unbind();
			
			Renderer.Disable(EnableCap.Blend);
			Renderer.Enable(EnableCap.DepthTest);
			Renderer.Enable(EnableCap.CullFace);	
		}
		
		public abstract void Use();
		public virtual void KeyUp(){}
		public virtual void Unload(){}
		public virtual void Load(){}
		public virtual void Update(){}

	    public Vector2 Scale { get; set; }

	    public Vector2 Position { get; set; }

	    public void Enable()
        {
			this.Enabled = true;
		}
		
		public void Disable()
        {
		    this.Enabled = false;
		}
		
		public void Dispose(){
			DrawManager.Remove(this);
			CooldownSecondsText.Dispose();
		}	
	}
}
