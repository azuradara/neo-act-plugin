using NeoActPlugin.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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
        private int _offsetCounter = 1;
        private readonly long[] _offsets = { 0x07485098, 0x490, 0x490, 0x670, 0x8, 0x70 };
        private DateTime _lastRefreshTime = DateTime.MinValue;

        // not sure what i should set this to, but i like 4
        private TimeSpan _refreshInterval = TimeSpan.FromSeconds(4);

        public Reader()
        {
            var pid = GetProcessId("BNSR.exe");
            if (!pid.HasValue)
                throw new ArgumentException("Process not found: " + "BNSR");

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

                int currentOffset = 0;
                while (true)
                {
                    var targetAddress = new IntPtr(_currentAddress.ToInt64() + (currentOffset * 0x70));
                    var pointerBuffer = ReadMemory(targetAddress, 8);
                    if (pointerBuffer == null || IsAllZero(pointerBuffer))
                        break;
                    currentOffset++;
                }

                _offsetCounter = currentOffset;
                _lastRefreshTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                PluginMain.WriteLog(LogLevel.Error, "Error refreshing pointers: " + ex.Message);
            }
        }

        public IEnumerable<string> Read()
        {
            if (DateTime.Now - _lastRefreshTime > _refreshInterval)
            {
                RefreshPointers();
            }

            int lastOffset = -1;

            while (true)
            {
                lastOffset = _offsetCounter;

                var targetAddress = new IntPtr(_currentAddress.ToInt64() + (_offsetCounter * 0x70));
                _offsetCounter++;

                var pointerBuffer = ReadMemory(targetAddress, 8);
                if (pointerBuffer == null) yield break;

                Thread.Sleep(1);

                while (IsAllZero(pointerBuffer))
                {
                    Thread.Sleep(100);

                    if (DateTime.Now - _lastRefreshTime > _refreshInterval)
                    {
                        RefreshPointers();
                        targetAddress = new IntPtr(_currentAddress.ToInt64() + ((_offsetCounter - 1) * 0x70));
                    }

                    pointerBuffer = ReadMemory(targetAddress, 8);
                    if (pointerBuffer == null) yield break;
                }

                var nextAddress = new IntPtr(BitConverter.ToInt64(pointerBuffer, 0));
                if (nextAddress == IntPtr.Zero) yield break;

                var stringBuffer = ReadMemory(nextAddress, 2048);
                if (stringBuffer == null) yield break;

                var decoded = DecodeString(stringBuffer);
                if (!string.IsNullOrEmpty(decoded) && _offsetCounter != lastOffset)
                    yield return decoded;
            }
        }

        private byte[] ReadMemory(IntPtr address, int size)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.PROCESS_VM_READ, false, _pid);
            if (processHandle == IntPtr.Zero)
                return null;

            try
            {
                var buffer = new byte[size];
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
            for (int i = 0; i < buffer.Length; i++)
                if (buffer[i] != 0) return false;
            return true;
        }

        private static int? GetProcessId(string processName)
        {
            var processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
            return processes.Length > 0 ? processes[0].Id : (int?)null;
        }

        private IntPtr GetBaseAddress(int pid)
        {
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_INFORMATION | ProcessAccessFlags.PROCESS_VM_READ, false, pid);
            if (hProcess == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            uint bytesNeeded;
            if (!EnumProcessModules(hProcess, null, 0, out bytesNeeded))
            {
                CloseHandle(hProcess);
                return IntPtr.Zero;
            }

            IntPtr[] modules = new IntPtr[bytesNeeded / IntPtr.Size];
            if (!EnumProcessModules(hProcess, modules, bytesNeeded, out bytesNeeded))
            {
                CloseHandle(hProcess);
                return IntPtr.Zero;
            }

            CloseHandle(hProcess);
            return modules.Length > 0 ? modules[0] : IntPtr.Zero;
        }

        private IntPtr FollowPointerChain(int pid, IntPtr baseAddress, long[] offsets)
        {
            var currentAddress = baseAddress;
            for (int i = 0; i < offsets.Length; i++)
            {
                currentAddress = new IntPtr(currentAddress.ToInt64() + offsets[i]);
                if (i >= offsets.Length - 1) continue;

                var buffer = ReadMemory(currentAddress, 8);
                if (buffer == null) return IntPtr.Zero;
                currentAddress = new IntPtr(BitConverter.ToInt64(buffer, 0));
            }
            return currentAddress;
        }
    }
}
