namespace Hedra.ModelHandlers
{
    public class GhostFactory : ModelFactory
    {
        protected override PartGroup[] Parts => new[]
        {
            new PartGroup
            {
                Paths = new[]
                {
                    "Assets/Chr/Mob/Ghost/GhostBody0.dae",
                    "Assets/Chr/Mob/Ghost/GhostBody1.dae",
                    "Assets/Chr/Mob/Ghost/GhostBody2.dae",
                    "Assets/Chr/Mob/Ghost/GhostBody3.dae",
                    "Assets/Chr/Mob/Ghost/GhostBody4.dae",
                    "Assets/Chr/Mob/Ghost/GhostBody5.dae",
                    "Assets/Chr/Mob/Ghost/GhostBody6.dae",
                    "Assets/Chr/Mob/Ghost/GhostBody7.dae"
                },
                Optional = false
            },
            new PartGroup
            {
                Paths = new[]
                {
                    "Assets/Chr/Mob/Ghost/GhostAccessory0.dae",
                    "Assets/Chr/Mob/Ghost/GhostAccessory1.dae",
                    "Assets/Chr/Mob/Ghost/GhostAccessory2.dae",
                    "Assets/Chr/Mob/Ghost/GhostAccessory3.dae"
                },
                Optional = true,
                Max = 2
            }
        };
    }
}