/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/12/2016
 * Time: 04:54 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Components;
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
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    /// <summary>
    /// Description of Campfire.
    /// </summary>
    public class Campfire : CraftingStation
    {
        private bool _canCraft;
        public override bool CanCraft => _canCraft;
        protected override string CraftingMessage => Translations.Get("use_campfire");
        public IHumanoid Bandit { get; set; }
        private static ParticleSystem _fireParticles;
        private long _passedTime;
        private PointLight _light;
        private AreaSound _fireSound;
        
        public Campfire(Vector3 Position) : base(Position)
        {
            if(_fireParticles == null)
                _fireParticles = new ParticleSystem(Vector3.Zero);
            _fireParticles.HasMultipleOutputs = true;
            _canCraft = true;
            _fireSound = new AreaSound(SoundType.Fireplace, FirePosition, 32);
        }

        public override Crafting.CraftingStation StationType => Crafting.CraftingStation.Campfire;
        
        protected virtual Vector3 FirePosition => Position;
        
        public override void Update()
        {
            base.Update();
            var distToPlayer = (FirePosition - GameManager.Player.Position).LengthSquared();
            if (distToPlayer < 256 * 256 && HasFire)
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

            if ( _light == null && distToPlayer < ShaderManager.LightDistance * ShaderManager.LightDistance * 2f && HasFire){

                this._light = ShaderManager.GetAvailableLight();

                if(this._light != null)
                {
                    _light.Color = LightColor;
                    _light.Radius = LightRadius;
                    _light.Position = FirePosition;
                    ShaderManager.UpdateLight(this._light);
                }
            }

            _fireSound.Position = FirePosition;
            _fireSound.Update(HasFire);

            if ( _light != null && (FirePosition - LocalPlayer.Instance.Position).LengthSquared() > ShaderManager.LightDistance * ShaderManager.LightDistance * 2f)
            {
                _light.Position = Vector3.Zero;
                _light.Locked = false;
                ShaderManager.UpdateLight(this._light);
                _light = null;
            }
        }

        public void SetCanCraft(bool CanCraft)
        {
            _canCraft = CanCraft;
        }

        private void HandleBurning()
        {
            var entities = World.Entities;
            for (var i = entities.Count - 1; i > -1; i--)
            {
                if (entities[i] == null) continue;
                if ((entities[i].Position - FirePosition).LengthSquared() < 4 * 4)
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

        protected virtual Vector3 LightColor { get; } = new Vector3(1.25f, .2f, .2f);
        protected virtual float LightRadius { get; } = 24;
        public bool HasFire { get; set; } = true;
        
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
            _fireSound.Dispose();
            Bandit?.Dispose();
        }
    }
}
