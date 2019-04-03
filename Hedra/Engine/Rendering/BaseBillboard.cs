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
using Hedra.Core;
using Hedra.Engine.Game;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Rendering
{
    public abstract class BaseBillboard : DrawableTexture, IRenderable, IDisposable, IUpdatable
    {
        private static readonly Shader Shader;
        protected abstract Vector2 Measurements { get; }
        public bool Disposed { get; private set; }
        public float VanishSpeed { get; set; } = 2;
        public bool Vanish { get; set; }
        public float Scalar { get; set; } = 1;
        private Func<Vector3> _follow;
        
        private Vector3 _position;
        private Vector3 _addedPosition;
        private readonly float _maxLifetime;
        private float _life;
        private float _opacity = 1;

        static BaseBillboard()
        {
            Shader = Shader.Build("Shaders/Billboard.vert", "Shaders/Billboard.frag");
        }

        protected BaseBillboard(float Lifetime, Func<Vector3> Follow)
        {
            this._maxLifetime = Lifetime;
            this._follow = Follow;
            this._position = _follow();
            DrawManager.UIRenderer.Add(this);
            UpdateManager.Add(this);
        }

        public void Update()
        {
            HandleVanish();
            _position = Mathf.Lerp(_position, _follow(), Time.DeltaTime * 8f);
            if(_life >= _maxLifetime) this.Dispose();
            _life += Time.DeltaTime;
        }

        public void Draw()
        {
            var viewMat = GameManager.Player.View.ModelViewMatrix;
            var cameraRight = new Vector3(viewMat.M11, viewMat.M21, viewMat.M31).NormalizedFast();
            var cameraUp = new Vector3(viewMat.M12, viewMat.M22, viewMat.M32).NormalizedFast();

            GUIRenderer.SetDraw(Shader);
            DrawManager.UIRenderer.BindQuadVAO();
            GUIRenderer.SetTexture(0, TextureId);
            Shader["texture_sampler"] = 0;       
            Shader["scale"] = Measurements * Scalar;
            Shader["position"] = _position + _addedPosition;
            //Shader["camera_right"] = cameraRight;
            //Shader["camera_up"] = cameraUp;
            Shader["opacity"] = _opacity;
            DrawManager.UIRenderer.DrawQuad();
            GUIRenderer.UnsetDrawing(Shader);
            
        }

        private void HandleVanish()
        {
            if (!Vanish) return;
            _addedPosition += Vector3.UnitY * Time.DeltaTime * VanishSpeed;
            _opacity = 1f - _life / _maxLifetime;
        }
        
        public virtual void Dispose()
        {
            DrawManager.UIRenderer.Remove(this);
            UpdateManager.Remove(this);
            TextureRegistry.Remove(TextureId);
            Disposed = true;
        }
    }
}
