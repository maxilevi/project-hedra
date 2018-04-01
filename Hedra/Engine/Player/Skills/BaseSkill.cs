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
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player.Skills
{
	/// <summary>
	/// Description of Skill.
	/// </summary>
	public abstract class BaseSkill : UIElement, IRenderable, IDisposable, IUpdatable
	{
		public static SkillsShader Shader { get; }
		public static Vector3 GrayTint { get; }
		public static Vector3 NormalTint { get; }
        public Vector3 Tint { get; set; }
        public float ManaCost { get; set; }
		public float Cooldown { get; set; }
		public float MaxCooldown { get; set; }
        public int Level { get; set; }
	    public bool Active { get; set; } = true;
        public bool Passive { get; set; }
        public abstract string Description { get; }
	    public bool Casting { get; set; }
	    public uint TexId { get; protected set; }
	    public uint MaskId { get; set; }
        protected bool UseMask => MaskId != 0;
	    protected bool Enabled { get; set; } = true;
		protected RenderableText CooldownSecondsText;
	    protected readonly LocalPlayer Player;
	    private readonly Panel _panel;

	    static BaseSkill()
	    {
	        Shader = new SkillsShader("Shaders/Skills.vert", "Shaders/Skills.frag");
            GrayTint = new Vector3(0.299f, 0.587f, 0.114f);
            NormalTint = Vector3.One;
        }


        protected BaseSkill(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player)
		{
			this.Player = Player;
		    this._panel = InPanel;
            this._position = Position;
			this.Scale = Scale;
			this.Tint = NormalTint;
		    _panel.AddElement(this);
			
			DrawManager.UIRenderer.Add(this, DrawOrder.After);			
			UpdateManager.Add(this);
		}
		
		public virtual bool MeetsRequirements(AbilityBarSystem.Toolbar Bar, int CastingAbilityCount)
		{
		    if (!(Cooldown < 0) || !((LocalPlayer.Instance.Mana - ManaCost) > 0) || CastingAbilityCount != 0 ||
		        this.Level <= 0 || !Active || Player.IsEating) return false;

		    return Player.IsRiding || !Player.IsRiding;
		}
		
		public virtual void Draw(){
			if(!Enabled || !Active)
				return;
			
			Cooldown -= Time.FrameTimeSeconds;
		    if (CooldownSecondsText == null)
		    {
		        CooldownSecondsText = new RenderableText(string.Empty, _position, Color.White,
		            FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));
		        DrawManager.UIRenderer.Add(CooldownSecondsText, DrawOrder.After);
                if(_panel.Enabled) CooldownSecondsText.Enable();
		        _panel.AddElement(CooldownSecondsText);
            }
		    if (CooldownSecondsText.Position != _position) CooldownSecondsText.Position = _position;
		    CooldownSecondsText.Text = Cooldown > 0 ? ((int)Cooldown + 1).ToString() : string.Empty;
			Tint = Player.Mana - this.ManaCost < 0 ? new Vector3(.9f,.6f,.6f) : new Vector3(1,1,1);
			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
			
			Shader.Bind();
			GL.Uniform3(Shader.TintUniform, Tint);
			GL.Uniform2(Shader.ScaleUniform, Scale * new Vector2(1,-1));
			GL.Uniform2(Shader.PositionUniform, Position);
			GL.Uniform2(Shader.BoolsUniform, new Vector2(Level == 0 ? 1 : 0, UseMask ? 1 : 0) );
			GL.Uniform1(Shader.CooldownUniform, this.Cooldown / this.MaxCooldown);
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexId);
			
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, MaskId);
			GL.Uniform1(Shader.MaskUniform, 1);
			
			DrawManager.UIRenderer.SetupQuad();
			DrawManager.UIRenderer.DrawQuad();
			
			Shader.UnBind();
			
			GL.Disable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);	
		}
		
		public abstract void KeyDown();
		public virtual void KeyUp(){}
		public virtual void UnloadBuffs(){}
		public virtual void LoadBuffs(){}
		public virtual void Update(){}

	    public Vector2 Scale { get; set; }

	    private Vector2 _position;
		public Vector2 Position{
			get{ return _position; }
			set{
			    _position = value;
            }
		}
		
		public void Enable(){
			this.Enabled = true;
		}
		
		public void Disable(){
		    this.Enabled = false;
		}
		
		public void Dispose(){
			DrawManager.Remove(this);
			CooldownSecondsText.Dispose();
		}	
	}
}