using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Game;
using OpenTK;

namespace Hedra.AISystem
{
    public class HostileAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        public HostileAIComponent(IEntity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new HostileBehaviour(Parent);
        }

        public override void Update()
        {
            if (Retaliate.Enabled)
            {
                Retaliate.Update();
            }
            else
            {
                Hostile.Update();
                if (!Hostile.Enabled)
                {
                    Roam.Update();
                }
            }
        }
        
        public override void Draw()
        {
            if (GameSettings.DebugAI)
            {
                var grid = TraverseStorage.Instance[Parent];
                for (var x = -grid.DimX / 2; x < grid.DimX / 2; x++)
                {
                    for (var y = -grid.DimY / 2; y < grid.DimY / 2; y++)
                    {
                        var offset = new Vector3(x, 0, y) * 4 + Parent.Position;
                        if(float.IsInfinity(grid.GetCellCost(new Vector2(x + grid.DimX / 2, y + grid.DimY / 2))))
                            BasicGeometry.DrawLine(offset - Vector3.UnitY * 4, offset + Vector3.UnitY * 2, Vector4.One);
                    } 
                }

                if (Retaliate.Enabled)
                    Retaliate.Draw();
                else if(Hostile.Enabled)
                    Hostile.Draw();
            }
        }

        public override AIType Type => AIType.Hostile;
    }
}
