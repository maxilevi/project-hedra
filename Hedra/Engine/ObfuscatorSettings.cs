﻿using System.Reflection;

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: Obfuscation(Exclude = false, StripAfterObfuscation = true, Feature = "+rename(mode=reversible,flatten=false,password='$_G+{M=U85aRq5');")]
/* Revise these */
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.EntitySystem'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.EntitySystem.BossSystem'):-rename")]

/* Template namespaces */
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.ModuleSystem.Templates'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.ItemSystem.Templates'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Engine.StructureSystem.VillageSystem.Templates'):-rename")]

/* API namespaces */
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.API'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Core'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Sound'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.AISystem'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Rendering'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Components'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.BiomeSystem'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.WeaponSystem'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.EntitySystem'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.AISystem.Behaviours'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('Hedra.Rendering.Particles'):-rename")]
