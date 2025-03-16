using NeoActPlugin.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NeoActPlugin.Core
{
    class Reader
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumProcessModules(
            IntPtr hProcess,
            [Out] IntPtr[] lphModule,
            uint cb,
            out uint lpcbNeeded);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_VM_READ = 0x0010,
            PROCESS_QUERY_INFORMATION = 0x0400
        }

        private readonly int _pid;
        private IntPtr _baseAddress;
        private IntPtr _currentAddress;
        private readonly long[] _offsets = { 0x07485098, 0x490, 0x490, 0x670, 0x8 };
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private TimeSpan _refreshInterval = TimeSpan.FromSeconds(4);
        private string[] _lastLines = new string[600];

        public Reader()
        {
            var pid = GetProcessId("BNSR.exe");
            if (!pid.HasValue)
                throw new ArgumentException("Process not found: BNSR");

            _pid = pid.Value;
            RefreshPointers();
        }

        private void RefreshPointers()
        {
            try
            {
                _baseAddress = GetBaseAddress(_pid);
                _currentAddress = FollowPointerChain(_pid, _baseAddress, _offsets);

                if (_currentAddress == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to resolve pointer chain");

                _lastRefreshTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                PluginMain.WriteLog(LogLevel.Error, "Error refreshing pointers: " + ex.Message);
            }
        }

        public string[] Read()
        {
            if (DateTime.Now - _lastRefreshTime > _refreshInterval)
            {
                RefreshPointers();
            }

            string[] currentLines = new string[600];
            for (int i = 0; i < 600; i++)
            {
                IntPtr targetAddress = new IntPtr(_currentAddress.ToInt64() + (i * 0x70));
                byte[] pointerBuffer = ReadMemory(targetAddress, 8);
                if (pointerBuffer == null || IsAllZero(pointerBuffer))
                {
                    currentLines[i] = string.Empty;
                    continue;
                }

                IntPtr nextAddress = new IntPtr(BitConverter.ToInt64(pointerBuffer, 0));
                if (nextAddress == IntPtr.Zero)
                {
                    currentLines[i] = string.Empty;
                    continue;
                }

                byte[] stringBuffer = ReadMemory(nextAddress, 512);
                if (stringBuffer == null)
                {
                    currentLines[i] = string.Empty;
                    continue;
                }

                string decoded = DecodeString(stringBuffer);
                int periodIndex = decoded.IndexOf('.');
                if (periodIndex != -1)
                    decoded = decoded.Substring(0, periodIndex + 1);

                currentLines[i] = decoded;
            }

            List<string> newEntries = new List<string>();
            for (int i = 0; i < 600; i++)
            {
                if (currentLines[i] != _lastLines[i] && !string.IsNullOrEmpty(currentLines[i]))
                    newEntries.Add(currentLines[i]);
            }

            _lastLines = (string[])currentLines.Clone();
            return newEntries.ToArray();
        }

        private byte[] ReadMemory(IntPtr address, int size)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.PROCESS_VM_READ, false, _pid);
            if (processHandle == IntPtr.Zero)
                return null;

            try
            {
                byte[] buffer = new byte[size];
                int bytesRead;
                bool success = ReadProcessMemory(processHandle, address, buffer, size, out bytesRead);
                return success && bytesRead == size ? buffer : null;
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        private static string DecodeString(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                if (buffer[i] == 0 && buffer[i + 1] == 0)
                    return Encoding.Unicode.GetString(buffer, 0, i);
            }
            return Encoding.Unicode.GetString(buffer);
        }

        private static bool IsAllZero(byte[] buffer)
        {
            foreach (byte b in buffer)
                if (b != 0) return false;
            return true;
        }

        private static int? GetProcessId(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
            return processes.Length > 0 ? processes[0].Id : (int?)null;
        }

        private IntPtr GetBaseAddress(int pid)
        {
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_INFORMATION | ProcessAccessFlags.PROCESS_VM_READ, false, pid);
            if (hProcess == IntPtr.Zero)
                return IntPtr.Zero;

            if (!EnumProcessModules(hProcess, null, 0, out uint bytesNeeded))
            {
                CloseHandle(hProcess);
                return IntPtr.Zero;
            }

            IntPtr[] modules = new IntPtr[bytesNeeded / IntPtr.Size];
            if (!EnumProcessModules(hProcess, modules, bytesNeeded, out _))
            {
                CloseHandle(hProcess);
                return IntPtr.Zero;
            }

            CloseHandle(hProcess);
            return modules.Length > 0 ? modules[0] : IntPtr.Zero;
        }

        private IntPtr FollowPointerChain(int pid, IntPtr baseAddress, long[] offsets)
        {
            IntPtr currentAddress = baseAddress;
            foreach (long offset in offsets)
            {
                currentAddress = new IntPtr(currentAddress.ToInt64() + offset);
                byte[] buffer = ReadMemory(currentAddress, 8);
                if (buffer == null) return IntPtr.Zero;
                currentAddress = new IntPtr(BitConverter.ToInt64(buffer, 0));
            }
            return currentAddress;
        }
    }
}