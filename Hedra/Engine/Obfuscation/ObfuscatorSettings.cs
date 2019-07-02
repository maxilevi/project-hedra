using System.Reflection;

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: Obfuscation(Exclude = false, StripAfterObfuscation = true, Feature = "+rename(mode=reversible,flatten=false,password='$_G+{M=U85aRq5');")]

/* Template namespaces */
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.ModuleSystem.Templates'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.ItemSystem.Templates'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.CraftingSystem.Templates'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.StructureSystem.VillageSystem.Templates'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.ClassSystem.Templates'):-rename")]

/* API namespaces */
[assembly: Obfuscation(Exclude = false, Feature = @"match('^Hedra(?!\.Engine).+'):-rename")]
