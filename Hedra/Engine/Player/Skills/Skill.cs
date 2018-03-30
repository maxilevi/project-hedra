/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using System.Collections;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Skill.
	/// </summary>
	public abstract class Skill : UIElement, IRenderable, IDisposable, IUpdatable
	{
		public static uint SkillsBarMask = Graphics2D.LoadFromAssets("Assets/SkillsBarMask.png");
		public static uint ReflectionMask = Graphics2D.LoadFromAssets("Assets/SkillsReflection.png");
		
		public static SkillsShader Shader = new SkillsShader("Shaders/Skills.vert", "Shaders/Skills.frag");
		public static Vector3 GrayTint = new Vector3(0.299f, 0.587f, 0.114f);
		public static Vector3 NormalTint = Vector3.One;
		public uint TexId;
		public Vector3 Tint = NormalTint;
		public float ManaCost = 0;
		public float Cooldown = 0;
		public float MaxCooldown;
		public LocalPlayer Player;
		public int Level;
		public bool Active = true;
		public bool UseMask = true;
		public bool Passive = false;
		public abstract string Description {get;}
		
		protected bool Enabled = true;
		protected RenderableText RText;
		public bool Casting;

	    private float _reflecTime;
	    private float _reflectionPower;
	    private bool _doEffect;

        protected Skill(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player)
		{
			this.Player = Player;
			this._position = Position;
			this.Scale = Scale;
			
			InPanel.AddElement(this);
			
			DrawManager.UIRenderer.Add(this, DrawOrder.After);
			RText = new RenderableText("", Position, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));
			DrawManager.UIRenderer.Add(RText, DrawOrder.Before);
			InPanel.AddElement(RText);
			
			UpdateManager.Add(this);
		}
		
		public virtual bool MeetsRequirements(AbilityBarSystem.AbilityBar Bar, int CastingAbilityCount)
		{
		    if (Cooldown < 0 && (LocalPlayer.Instance.Mana - ManaCost) > 0 && CastingAbilityCount == 0
		        && this.Level > 0 && Active && !Player.IsEating)
		    {
		        if (Player.IsRiding || !Player.IsRiding) return true;
		    }
		    return false;
		}
		
		public virtual void Draw(){
			if(!Enabled || !Active)
				return;
			
			Cooldown -= Time.FrameTimeSeconds;
			if(Cooldown > 0)
				RText.Text = ((int) Cooldown + 1).ToString();
			else
				this.RText.Text = "";
			Tint = Player.Mana - this.ManaCost < 0 ? new Vector3(.9f,.6f,.6f) : new Vector3(1,1,1);
			
			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
			
			Shader.Bind();
			GL.Uniform3(Shader.TintUniform, Tint);
			GL.Uniform2(Shader.ScaleUniform, Scale * new Vector2(1,-1));
			GL.Uniform2(Shader.PositionUniform, Position);
			GL.Uniform2(Shader.BoolsUniform, new Vector2((Level == 0) ? 1 : 0, (UseMask) ? 1 : 0) );
			GL.Uniform1(Shader.CooldownUniform, this.Cooldown / this.MaxCooldown);
			GL.Uniform1(Shader.TimeUniform, _reflecTime);
			GL.Uniform1(Shader.ReflectionUniform, _reflectionPower);
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexId);
			
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, SkillsBarMask);
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
		
		public void PlayReflection(){
			_doEffect = true;
			CoroutineManager.StartCoroutine(ReflectionTask);
		}
		
		private IEnumerator ReflectionTask(){
			this._reflectionPower = 1;
			while(_doEffect){
				yield return null;
				this._reflecTime += Time.FrameTimeSeconds;
			}
			this._reflecTime = 0f;
			this._reflectionPower = 0f;
		}
		
		public void StopReflection(){
			_doEffect = false;
		}

	    public Vector2 Scale { get; set; }

	    private Vector2 _position;
		public Vector2 Position{
			get{ return _position; }
			set{ 
				_position = value;
				RText.Dispose();
				RText = new RenderableText("", Position, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));
				DrawManager.UIRenderer.Add(RText, DrawOrder.After);
			}
		}
		
		public void Enable(){
			this.Enabled = true;
		    RText?.Enable();
		}
		
		public void Disable(){
		    RText?.Disable();
		    this.Enabled = false;
		}
		
		public void Dispose(){
			DrawManager.Remove(this);
			RText.Dispose();
		}
		
	}
}
