using System.IO;
using System.Linq;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Microsoft.Scripting.Utils;
using Microsoft.VisualBasic;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace HedraTests.Player;

public class SpawnerSettingsTest
{
    private static readonly string[] _overworldExceptions =
    {
        "RangedBeetle",
        "Ghost",
        "PossessedCow",
        "Skeleton",
        "SkeletonKamikaze",
        "Lich",
        "GiantBeetle",
        "Golem",
        "Troll",
        "SkeletonKing",
        "GorillaWarrior",
        "Pug"
    };

    [Test]
    public void TestOverworldAllMobsAreSpecified()
    {
        var appPath = GameLoader.AppPath;
        AssetManager.Provider = new DummyAssetProvider();
        var mobs = MobLoader.LoadModules(appPath);
        var names = mobs.Select(M => M.Name).ToHashSet();
        var template = SpawnerLoader.Load(appPath, "Overworld");
        var templates = template.Forest.Select(S => S.Type)
            .Concat(template.Mountain.Select(S => S.Type))
            .Concat(template.Plains.Select(S => S.Type))
            .Concat(template.Shore.Select(S => S.Type))
            .Concat(template.MiniBosses.Select(S => S.Type))
            .Concat(_overworldExceptions).ToArray();
        
        
        for (var i = 0; i < templates.Length; ++i)
        {
            if (names.Contains(templates[i]))
                names.Remove(templates[i]);
        }
        
        
        Assert.IsEmpty(names);
    }
}