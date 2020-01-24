using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Hedra.Engine.Native
{
    public class WindowsScreenManager : IScreenManager
    {
        private bool _isAware;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public Vector2[] GetResolutions()
        {
            if (!_isAware)
            {
                _isAware = true;
                //SetProcessDPIAware();
            }
            return Screen.AllScreens.Select(S => new Vector2(S.Bounds.Width, S.Bounds.Height)).ToArray();
        }
    }
}