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
            for(var i = 0; i< World.Entities.Count; i++)
            {
                if(World.Entities[i] == LocalPlayer.Instance) continue;
                    
                var toEntity = (World.Entities[i].Position - Caster.Position).NormalizedFast();
                var dot = Vector3.Dot(toEntity, Caster.Orientation);
                if(dot >= Angle && (World.Entities[i].Position - Caster.Position).LengthSquared < Radius * Radius)
                {
                    World.Entities[i].Damage(Damage * dot, Caster, out var exp, PlaySound);
                    Caster.XP += exp;
                }
            }
        }
    }
}