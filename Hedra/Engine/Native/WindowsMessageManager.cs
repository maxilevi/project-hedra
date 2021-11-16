using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Native
{
    public class WindowsMessageManager : IMessageManager
    {
        private const int OkType = 0;
        private const int ErrorIcon = 10;

        public void Show(string Message, string Title)
        {
            MessageBox(IntPtr.Zero, Message, Title, OkType | ErrorIcon);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr h, string m, string c, int type);
    }
}