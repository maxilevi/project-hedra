using System;
using Hedra.Core;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem
{
    public class SkillUtils
    {
        public static void DamageNearby(IHumanoid Caster, float Damage, float Radius, float Angle = 0, bool PlaySound = true)
        {
            DoNearby(Caster, Radius, Angle, (E, D) =>
            {
                E.Damage(Damage * D, Caster, out var exp, PlaySound);
                Caster.XP += exp;
            });
        }
        
        public static void DoNearby(IHumanoid Caster, float Radius, float Angle, Action<IEntity, float> Lambda)
        {
            var entities = World.Entities;
            for(var i = 0; i< entities.Count; i++)
            {
                if(entities[i] == Caster) continue;
                    
                var toEntity = (entities[i].Position - Caster.Position).NormalizedFast();
                var dot = Vector3.Dot(toEntity, Caster.Orientation);
                if(dot >= Angle && (entities[i].Position - Caster.Position).LengthSquared < Radius * Radius)
                {
                    Lambda(entities[i], dot);
                }
            }
        }

        public static bool IsBehindAny(IHumanoid Caster)
        {
            return IsBehindAny(Caster, out _);
        }
        
        public static bool IsBehindAny(IHumanoid Caster, out IEntity Entity)
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
        
        
        public static bool IsBehind(IHumanoid Caster, IEntity Victim)
        {
            return Caster.InAttackRange(Victim, 2f) && Vector3.Dot(Victim.Orientation, Caster.Orientation) > -.75f 
                                                    && Vector3.Dot((Victim.Position - Caster.Position).Normalized(), Caster.Orientation) > .75f;
        }
    }
}