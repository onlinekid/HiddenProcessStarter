using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace HiddenProcessStarter
{
    internal static class Program
    {
        private static void Main()
        {
            var setting = File.ReadAllText("HiddenProcessStarter.txt");

            var settingsSpl = setting
                .Replace("\r", string.Empty)
                .Split(new[] {"\n"}, StringSplitOptions.None);

            var processName = settingsSpl[0];
            var filePath = settingsSpl[1];
            var waitMilliseconds = int.Parse(settingsSpl[2]);

            var currentlyStartedProcesses = GetAllProcessesByNameOrWindowTitle(processName);
            if (currentlyStartedProcesses.Any())
            {
                foreach (var p in currentlyStartedProcesses)
                {
                    p.Kill();
                }

                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/C \"{filePath}\"",
                WindowStyle = ProcessWindowStyle.Hidden
            });

            var endDate = DateTime.Now.AddMilliseconds(waitMilliseconds);
            while (true)
            {
                if (DateTime.Now > endDate) return;

                var startedProcess = GetAllProcessesByNameOrWindowTitle(processName);

                if (startedProcess.Any(p => p.MainWindowHandle != IntPtr.Zero))
                {
                    foreach (var p in startedProcess)
                    {
                        ShowWindow(p.MainWindowHandle.ToInt32(), 0);
                    }

                    break;
                }

                Thread.Sleep(50);
            }
        }

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hWnd, int nCmdShow);

        private static IReadOnlyList<Process> GetAllProcessesByNameOrWindowTitle(string processName)
        {
            return Process.GetProcesses()
                .Where(p => p.ProcessName == processName || p.MainWindowTitle == processName)
                .ToList();
        }
    }
}