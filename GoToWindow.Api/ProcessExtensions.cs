﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace GoToWindow.Api
{
    public static class ProcessExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessExtensions).Assembly, "GoToWindow");

        // ReSharper disable InconsistentNaming
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        // ReSharper restore InconsistentNaming

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        public static string GetExecutablePath(this Process process)
        {
            if (process == null || process.Id == 0)
                return null;

            try
            {
                //If running on Vista or later use the new function
                return Environment.OSVersion.Version.Major >= 6
                    ? GetExecutablePathAboveVista(process.Id)
                    : process.MainModule.FileName;
            }
            catch(Win32Exception exc)
            {
                Log.Error($"Error while trying to get executable path of process {process.ProcessName} with PID {process.Id}", exc);
                return null;
            }
        }

        private static string GetExecutablePathAboveVista(int processId)
        {
            int lpdwSize = 1024;
            var buffer = new StringBuilder(lpdwSize);
            var hprocess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);

            if (hprocess == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                if (QueryFullProcessImageName(hprocess, 0, buffer, out lpdwSize))
                {
                    return buffer.ToString();
                }
            }
            finally
            {
                CloseHandle(hprocess);
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}