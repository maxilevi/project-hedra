using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class LineGroundwork : IGroundwork
    {
        public BlockType Type { get; set; }
        public Half Density { get; set; }

        public bool Affects(Vector2 Sample)
        {
            throw new System.NotImplementedException();
        }
    }
}
