using System;
using Hedra.Core;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.SkillSystem
{
    public class SkillUtils
    {
        public static void DamageNearby(IEntity Caster, float Damage, float Radius, float Angle = 0, bool PlaySound = true)
        {
            DoNearby(Caster, Radius, Angle, (E, D) =>
            {
                E.Damage(Damage * D, Caster, out var exp, PlaySound);
                if(Caster is IHumanoid human)
                    human.XP += exp;
            });
        }
        
        public static void DoNearby(IEntity Caster, Vector3 Position, float Radius, float Angle, Action<IEntity, float> Lambda)
        {
            var entities = World.Entities;
            for(var i = 0; i< entities.Count; i++)
            {
                if(entities[i] == Caster) continue;
                    
                var toEntity = (entities[i].Position - Position).NormalizedFast();
                var dot = Vector3.Dot(toEntity, Caster.Orientation);
                if(dot >= Angle && (entities[i].Position - Position).LengthSquared() < Radius * Radius)
                {
                    Lambda(entities[i], dot);
                }
            }
        }
        
        public static void DoNearby(IEntity Caster, float Radius, float Angle, Action<IEntity, float> Lambda)
        {
            DoNearby(Caster, Caster.Position, Radius, Angle, Lambda);
        }
        
        public static void DoNearby(IEntity Caster, float Radius, Action<IEntity> Lambda)
        {
            DoNearby(Caster, Radius, -1, (E,F) => Lambda(E));
        }
        
        public static void DoNearby(IEntity Caster, Vector3 Position, float Radius, Action<IEntity> Lambda)
        {
            DoNearby(Caster, Position, Radius, -1, (E,F) => Lambda(E));
        }

        public static bool IsBehindAny(IEntity Caster)
        {
            return IsBehindAny(Caster, out _);
        }
        
        public static bool IsBehindAny(IEntity Caster, out IEntity Entity)
        {
            var entities = World.Entities;
            for (var i = 0; i < entities.Count; ++i)
            {
                if (entities[i] != Caster && IsBehind(Caster, entities[i]))
                {
                    Entity = entities[i];
                    return true;
                }
            }

            Entity = null;
            return false;
        }
        
        public static IEntity GetNearest(IEntity Caster, Vector3 Direction, float Angle, float Radius, Func<IEntity, bool> Lambda)
        {
            var entities = World.Entities;
            var highest = -1f;
            IEntity entity = null;
            for (var i = 0; i < entities.Count; ++i)
            {
                if(entities[i] == Caster) continue;
                var toEntity = (entities[i].Position - Caster.Position).NormalizedFast();
                var dot = Vector3.Dot(toEntity, Direction);
                if(Lambda(entities[i]) && dot > Angle && dot > highest && (entities[i].Position - Caster.Position).LengthSquared() < Radius * Radius)
                {
                    entity = entities[i];
                    highest = dot;
                }
            }
            return entity;
        }
        
        
        public static bool IsBehind(IEntity Caster, IEntity Victim)
        {
            return Caster.InAttackRange(Victim, 2f) && Vector3.Dot(Victim.Orientation, Caster.Orientation) > -.75f 
                                                    && Vector3.Dot((Victim.Position - Caster.Position).Normalized(), Caster.Orientation) > .75f;
        }

        public static void SpawnParticles(Vector3 Position, Vector4 Color)
        {
            World.Particles.Color = Color;
            World.Particles.ParticleLifetime = 1.25f;
            World.Particles.GravityEffect = .0f;
            World.Particles.Scale = new Vector3(.75f, .75f, .75f);
            World.Particles.Position = Position;
            World.Particles.PositionErrorMargin = Vector3.One * 1.75f;
            for (var i = 0; i < 40; i++)
            {
                World.Particles.Direction = new Vector3(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()) * .15f;
                World.Particles.Emit();
            }
        }
        
        public static void DarkSpawnParticles(Vector3 Position)
        {
            SpawnParticles(Position, Vector4.One * .25f);
        }

        public static void DarkContinuousParticles(IEntity Parent)
        {
            World.Particles.Color = new Vector4(.2f, .2f, .2f, .8f);
            World.Particles.VariateUniformly = true;
            World.Particles.Position =
                Parent.Position + Vector3.UnitY * Parent.Model.Height * .3f;
            World.Particles.Scale = Vector3.One * .25f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = -Parent.Orientation * .05f;
            World.Particles.ParticleLifetime = 1.0f;
            World.Particles.GravityEffect = 0.0f;
            World.Particles.PositionErrorMargin = new Vector3(1.25f, Parent.Model.Height * .3f, 1.25f);
            World.Particles.Emit();
        }
    }
}