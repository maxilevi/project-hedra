﻿using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Management
{
    public class WindowsConsoleManager : IConsoleManager
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private bool _show;
        
        public bool Show
        {
            get => _show;
            set
            {
                _show = value;
                var handle = GetConsoleWindow();
                ShowWindow(handle, _show ? SW_SHOW : SW_HIDE);
            }
        }
    }
}