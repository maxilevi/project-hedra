using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.Engine.ClassSystem.Templates
{
    public class AbilityTreeTemplate : INamedTemplate
    {
        public string Name { get; set; }
        public string Icon { get; set; } = "$DataFile$/Assets/Skills/HolderSkill.png";
        public string M11 { get; set; }
        public string M12 { get; set; }
        public string M13 { get; set; }
        public string M14 { get; set; }
        public string M21 { get; set; }
        public string M22 { get; set; }
        public string M23 { get; set; }
        public string M24 { get; set; }
        public string M31 { get; set; }
        public string M32 { get; set; }
        public string M33 { get; set; }
        public string M34 { get; set; }

        public string Get(int I, int J)
        {
            return Array[I][J];
        }

        private string[][] Array => new[]
        {
            new []
            {
                M11,
                M12,
                M13,
                M14
            },
            new []
            {
                M21,
                M22,
                M23,
                M24
            },
            new []
            {
                M31,
                M32,
                M33,
                M34
            },
        };
    }
}