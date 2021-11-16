namespace Hedra.BiomeSystem
{
    public class BiomeDesign
    {
        public BiomeColorsDesign ColorDesign { get; set; }
        public BiomeStructureDesign StructureDesign { get; set; }
        public BiomeMobDesign MobDesign { get; set; }
        public BiomeEnviromentDesign EnvironmentDesign { get; set; }
        public BiomeTreeDesign TreeDesign { get; set; }
        public BiomeSkyDesign SkyDesign { get; set; }
        public BiomeGenerationDesign GenerationDesign { get; set; }
    }
}