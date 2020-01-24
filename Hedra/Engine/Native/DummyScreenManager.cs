using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Hedra.Engine.Native
{
    public class DummyScreenManager : IScreenManager
    {
        public Vector2[] GetResolutions()
        {
            return Screen.AllScreens.Select(S => new Vector2(S.Bounds.Width, S.Bounds.Height)).ToArray();
        }
    }
}