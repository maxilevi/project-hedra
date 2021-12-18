using System;

namespace Hedra.Engine.ModuleSystem.Templates
{
    public class ModelTemplate : SerializableTemplate<ModelTemplate>
    {
        public string Handler { get; set; }
        public bool AlignWithTerrain { get; set; } = true;
        public bool FlipNormals { get; set; }
        public string Path { get; set; }
        public float Scale { get; set; }
        public bool IsUndead { get; set; }
        public bool IsFlying { get; set; }
        public AnimationTemplate[] IdleAnimations { get; set; }
        public AnimationTemplate[] WalkAnimations { get; set; }
        public AttackAnimationTemplate[] AttackAnimations { get; set; }

        public void Resolve()
        {
            var eachChance = CalculateChanceForUnassigned(AttackAnimations);
            for (var i = 0; i < AttackAnimations.Length; ++i)
                AttackAnimations[i].Chance = Math.Abs(AttackAnimations[i].Chance) < 0.005f
                    ? eachChance
                    : AttackAnimations[i].Chance;
        }

        private static float CalculateChanceForUnassigned(AttackAnimationTemplate[] Templates)
        {
            var used = 0f;
            var unusedCount = 0;
            for (var i = 0; i < Templates.Length; ++i)
            {
                if (Math.Abs(Templates[i].Chance) < 0.005f)
                    unusedCount++;
                used += Templates[i].Chance;
            }

            return unusedCount == 0 ? 0 : (1 - used) / unusedCount;
        }
    }
}