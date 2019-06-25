/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/12/2016
 * Time: 04:54 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Components.Effects;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Sound;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    /// <summary>
    /// Description of Campfire.
    /// </summary>
    public class Campfire : CraftingStation
    {
        protected override string CraftingMessage => Translations.Get("use_campfire");
        public IHumanoid Bandit { get; set; }
        private static ParticleSystem _fireParticles;
        private long _passedTime;
        private PointLight _light;
        private SoundItem _sound;
        
        public Campfire(Vector3 Position) : base(Position)
        {
            if(_fireParticles == null)
                _fireParticles = new ParticleSystem(Vector3.Zero);
            _fireParticles.HasMultipleOutputs = true;
        }

        public override CraftingSystem.CraftingStation StationType => CraftingSystem.CraftingStation.Campfire;
        
        protected virtual Vector3 FirePosition => Position;
        
        public override void Update()
        {
            base.Update();
            var distToPlayer = (FirePosition - GameManager.Player.Position).LengthSquared;
            if (distToPlayer < 256 * 256)
            {
                if (this._passedTime++ % 2 == 0)
                {
                    _fireParticles.Color = Particle3D.FireColor;
                    _fireParticles.VariateUniformly = false;
                    _fireParticles.Position = FirePosition + Vector3.UnitY * 1f;
                    _fireParticles.Scale = Vector3.One * .85f;
                    _fireParticles.ScaleErrorMargin = new Vector3(.05f, .05f, .05f);
                    _fireParticles.Direction = Vector3.UnitY * 0f;
                    _fireParticles.ParticleLifetime = 1.65f;
                    _fireParticles.GravityEffect = -0.01f;
                    _fireParticles.PositionErrorMargin = new Vector3(1f, 0f, 1f);

                    _fireParticles.Emit();
                }

                HandleBurning();
            }

            if ( _light == null && distToPlayer < ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){

                this._light = ShaderManager.GetAvailableLight();

                if(this._light != null)
                {
                    _light.Color = new Vector3(.8f, .3f, .2f);
                    _light.Radius = 24;
                    _light.Position = FirePosition;
                    ShaderManager.UpdateLight(this._light);
                }
            }

            if (this._sound == null && distToPlayer < 32f*32f*2f)
            {
                this._sound = SoundPlayer.GetAvailableSource();
            }

            if (this._sound != null)
            {

                if (!this._sound.Source.IsPlaying)
                    this._sound.Source.Play(SoundPlayer.GetBuffer(SoundType.Fireplace), FirePosition, 1f, 1f, true);

                var gain = Math.Max(0, 1 - (FirePosition - SoundPlayer.ListenerPosition).LengthFast / 32f);
                this._sound.Source.Volume = gain;
            }

            if ( this._light != null && (FirePosition - LocalPlayer.Instance.Position).LengthSquared >
                ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){

                this._light.Position = Vector3.Zero;
                this._light.Locked = false;
                ShaderManager.UpdateLight(this._light);
                this._light = null;
                
            }

            if (this._sound != null && distToPlayer > 32f * 32f * 2f)
            {
                this._sound.Source.Stop();
                this._sound.Locked = false;
                this._sound = null;
            }
        }

        private void HandleBurning()
        {
            var entities = World.Entities;
            for (var i = entities.Count - 1; i > -1; i--)
            {
                if (entities[i] == null) continue;
                if ((entities[i].Position - FirePosition).LengthSquared < 4 * 4)
                {
                    if (entities[i].SearchComponent<BurningComponent>() == null)
                    {
                        var isImmune = entities[i].SearchComponent<DamageComponent>().Immune;
                        if (isImmune) continue;
                        entities[i].AddComponent(new BurningComponent(World.Entities[i], 5f, 40f));
                    }
                }
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            if (this._light != null)
            {
                this._light.Color = Vector3.Zero;
                this._light.Position = Vector3.Zero;
                ShaderManager.UpdateLight(this._light);
                this._light.Locked = false;
                this._light = null;
            }
            if (this._sound != null)
            {
                this._sound.Locked = false;
                this._sound = null;
            }
            Bandit?.Dispose();
        }
    }
}
