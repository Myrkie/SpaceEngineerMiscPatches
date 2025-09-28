using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using VRage.Input;

namespace ClientPlugin.Pulsar_Patches
{
    public static partial class ConsoleManager
    {
        private static bool _consoleVisible;
        private static bool _consoleInitialized;

        public static void Init()
        {
            if (_consoleInitialized) return;

            AllocConsole();
            EnableAnsiSupport();
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));

            // Hide the console window right after allocating it
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, SwHide);
            }

            _consoleInitialized = true;
            _consoleVisible = false;

            Plugin.WriteToPulsarLog("Console initialized", NLog.LogLevel.Info);
        }

        private static bool _allowConsole;
        private static bool _allowConsoleLastFrame;
        public static void Update()
        {
            bool altTilda = MyInput.Static.IsAnyAltKeyPressed() && MyInput.Static.IsKeyPress(MyKeys.OemTilde);
            if (altTilda && !_allowConsoleLastFrame)
            {
                _allowConsole = !_allowConsole;
                ToggleConsole();
            }
            _allowConsoleLastFrame = altTilda;
        }

        public static void ToggleConsole()
        {
            var handle = GetConsoleWindow();
            if (handle == IntPtr.Zero) return;

            if (_consoleVisible)
            {
                ShowWindow(handle, SwHide);
                _consoleVisible = false;
            }
            else
            {
                ShowWindow(handle, SwShow);
                SetForegroundWindow(handle);
                _consoleVisible = true;
            }
        }

        private static void EnableAnsiSupport()
        {
            var handle = GetStdHandle(WmStdOutputHandle);
            if (!GetConsoleMode(handle, out var mode)) return;
            mode |= WmEnableVirtualTerminalProcessing;
            SetConsoleMode(handle, mode);
        }

        // --- Win32 API Imports and Constants ---

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        private const int WmStdOutputHandle = -11;
        private const uint WmEnableVirtualTerminalProcessing = 0x0004;

        private const int SwHide = 0;
        private const int SwShow = 5;
    }
}