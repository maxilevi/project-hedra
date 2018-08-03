/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/06/2016
 * Time: 12:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Rendering
{
	//Kind of hacky, refactor later
	public class Billboard : IRenderable, IDisposable
	{
		public Vector3 Position { get; set; }
        public float LifeTime { get; set; }
        public bool Disposed { get; set; }
        public bool Vanish { get; set; }
		public float Size { get; set; } = 1;
		public float Speed { get; set; } = 2;
	    public bool DisposeTextureId { get; set; } = true;
		public Func<Vector3> FollowFunc { get; set; }
        public UIElement Texture { get; set; }
        public bool Enabled {get; set;}		
		private readonly bool _textBillboard;
		private readonly Vector2 _originalScale;
	    private float _life;
	    private Vector3 _addedPosition;

        public Billboard(float LifeTime, string Text, Color TextColor, Font TextFont, Vector3 Position)
		{
			_textBillboard = true;
			this.LifeTime = LifeTime;
			this.Position = Position;
			this.Texture = new GUIText(string.Empty, Vector2.Zero, TextColor, TextFont);
			this.Texture.Enable();
			this._originalScale = this.Texture.Scale;
            this.UpdateText(Text);
		    DrawManager.UIRenderer.Add(this);
        }
		
		public Billboard(float LifeTime, uint Icon, Vector3 Position, Vector2 Scale)
		{
			_textBillboard = false;
			this.LifeTime = LifeTime;
			this.Position = Position;
			this.Texture = new Texture(Icon, Vector2.Zero, Scale);
			this.Texture.Enable();
			this._originalScale = Scale;
			DrawManager.UIRenderer.Remove( ((Texture) this.Texture).TextureElement);			
			DrawManager.UIRenderer.Add(this);
		}

	    public void UpdateText(string Text)
	    {
	        var asGuiText = (GUIText) this.Texture;
	        asGuiText.Text = Text;
            DrawManager.UIRenderer.Remove(asGuiText.UIText);
        }

		public void Draw()
        {
		    if (FollowFunc != null)
		    {
		        Position = Mathf.Lerp(Position, FollowFunc(), (float) Time.DeltaTime * 8f);
		    }
			
			if(Vanish)
            {
				_addedPosition += Vector3.UnitY * Time.DeltaTime * Speed;
				((GUIText) this.Texture).UIText.Opacity = 1-(_life / LifeTime);
			}
			_life += Time.DeltaTime;
			
			if(LifeTime != 0 && _life >= LifeTime){
				this.Dispose();
				return;
			}
			
			var player = GameManager.Player;
			float product = Mathf.DotProduct(player.View.LookingDirection, (Position+_addedPosition - player.Position).NormalizedFast());
			if(product <= -0.5f) return;
			
			Vector4 eyeSpace = Vector4.Transform(new Vector4(Position+_addedPosition,1), DrawManager.FrustumObject.ModelViewMatrix);
			Vector4 homogeneusSpace = Vector4.Transform(eyeSpace, DrawManager.FrustumObject.ProjectionMatrix);
			Vector3 ndc = homogeneusSpace.Xyz / homogeneusSpace.W;
			this.Texture.Position = Mathf.Clamp(ndc.Xy, -.98f, .98f);
            if (!_textBillboard)
            {
                this.Texture.Scale = this._originalScale * Size;
            }

            DrawManager.UIRenderer.Draw(_textBillboard
		        ? ((GUIText) this.Texture).UIText
		        : ((Texture) this.Texture).TextureElement);
		}
		
		public void Dispose()
        {
            if (!DisposeTextureId)
            {
                var texAsTexture = Texture as Texture;
                var texAsText = Texture as GUIText;
                if (texAsTexture != null) texAsTexture.TextureElement.TextureId = 0;
                if(texAsText != null) texAsText.UIText.TextureId = 0;
            }
            this.Texture?.Dispose();
			DrawManager.Remove(this);
			Disposed = true;
		}
	}
}
